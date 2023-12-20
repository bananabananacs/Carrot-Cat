using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    public float driftFactor = 0.95f;
    public float accelerationFactor = 30.0f;
    public float turnFactor = 3.5f;
    public float maxSpeed = 20;

    float startTurnFactor = 0;

    [Header("Sprites")]
    public SpriteRenderer carSpriteRenderer;
    public SpriteRenderer carShadowSpriteRenderer;

    [Header("Jumping")]
    public AnimationCurve jumpCurve;
    public ParticleSystem landingParticleSystem;

    public float accelerationInput = 0;
    float steeringInput = 0;

    float rotationAngle = 0;

    float velocityVsUp = 0;

    public bool isJumping = false;

    Rigidbody2D carRigidbody2D;

    CarAudioHandler carAudioHandler;

    private void Awake()
    {
        carRigidbody2D = GetComponent<Rigidbody2D>();
        carAudioHandler = GetComponentInChildren<CarAudioHandler>();
        startTurnFactor = turnFactor;
    }

    private void FixedUpdate()
    {
        ApplyEngineForce();
        KillOrthogonalVelocity();
        ApplySteering();
    }

    void ApplyEngineForce()
    {
        velocityVsUp = Vector2.Dot(transform.up, carRigidbody2D.velocity);

        if (velocityVsUp > maxSpeed && accelerationInput > 0)
        {
            return;
        }

        if (velocityVsUp < -maxSpeed * 0.5f && accelerationInput < 0)
        {
            return;
        }

        if (carRigidbody2D.velocity.sqrMagnitude > maxSpeed * maxSpeed && accelerationInput > 0 && !isJumping)
        {
            return;
        }

        if (accelerationInput == 0)
        {
            carRigidbody2D.drag = Mathf.Lerp(carRigidbody2D.drag, 3.0f, Time.fixedDeltaTime * 3);
        } else
        {
            carRigidbody2D.drag = 0;
        }

        Vector2 engineForceVector = transform.up * accelerationInput * accelerationFactor;

        carRigidbody2D.AddForce(engineForceVector, ForceMode2D.Force);
    }

    void ApplySteering()
    {
        if (isJumping)
        {
            turnFactor = startTurnFactor / 2f;
        } else
        {
            turnFactor = startTurnFactor;
        }

        float minSpeedBeforeApplyTurningFactor = (carRigidbody2D.velocity.magnitude / 8);
        minSpeedBeforeApplyTurningFactor = Mathf.Clamp01(minSpeedBeforeApplyTurningFactor);

        rotationAngle -= steeringInput * turnFactor * minSpeedBeforeApplyTurningFactor;

        carRigidbody2D.MoveRotation(rotationAngle);
    }

    void KillOrthogonalVelocity()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(carRigidbody2D.velocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(carRigidbody2D.velocity, transform.right);

        carRigidbody2D.velocity = forwardVelocity + rightVelocity * driftFactor;
    }

    float GetLateralVelocity()
    {
        return Vector2.Dot(transform.right, carRigidbody2D.velocity);
    }

    public float GetVelocityMagnitude()
    {
        return carRigidbody2D.velocity.magnitude;
    }

    public bool IsTireScreeching(out float lateralVelocity, out bool isBraking)
    {
        lateralVelocity = GetLateralVelocity();
        isBraking = false;

        if (accelerationInput < 0 && velocityVsUp > 0 && !isJumping)
        {
            isBraking = true;
            return true;
        }

        if (Mathf.Abs(GetLateralVelocity()) > 2.0f && !isJumping)
        {
            return true;
        }

        return false;
    }

    public void SetInputVector(Vector2 inputVector)
    {
        steeringInput = inputVector.x;
        accelerationInput = inputVector.y;
    }

    public void Jump(float jumpHeightScale, float jumpPushScale)
    {
        if (!isJumping)
        {
            StartCoroutine(JumpCo(jumpHeightScale, jumpPushScale));
        }
    }

    private IEnumerator JumpCo(float jumpHeightScale, float jumpPushScale)
    {
        isJumping = true;

        float jumpStartTime = Time.time;
        float jumpDuration = carRigidbody2D.velocity.magnitude * 0.5f;

        jumpHeightScale = jumpHeightScale * carRigidbody2D.velocity.magnitude * 0.5f;
        jumpHeightScale = Mathf.Clamp(jumpHeightScale, 0.0f, 1.0f);

        carAudioHandler.PlayJumpSFX();

        carRigidbody2D.AddForce(carRigidbody2D.velocity.normalized * jumpPushScale * 10, ForceMode2D.Impulse);

        while (isJumping)
        {
            float jumpCompletedPercentage = (Time.time - jumpStartTime) / jumpDuration;
            jumpCompletedPercentage = Mathf.Clamp01(jumpCompletedPercentage);

            carSpriteRenderer.transform.localScale = new Vector3(0.4f, 0.4f, 0f) + new Vector3(0.4f, 0.4f, 0) * jumpCurve.Evaluate(jumpCompletedPercentage) * jumpHeightScale;
            
             
            carShadowSpriteRenderer.transform.localScale = carSpriteRenderer.transform.localScale * 1.6f;

            carShadowSpriteRenderer.transform.localPosition = new Vector3(1, -1f, 0.0f) * 3 * jumpCurve.Evaluate(jumpCompletedPercentage) * jumpHeightScale;
            
            if (jumpCompletedPercentage == 1.0f)
            {
                break;
            }

            yield return null;
        }

        carSpriteRenderer.transform.localScale = new Vector3(0.4f, 0.4f, 0);

        carShadowSpriteRenderer.transform.localPosition = Vector3.zero;
        carShadowSpriteRenderer.transform.localScale = new Vector3(0.75f, 0.75f, 0);

        if (jumpHeightScale > 0.2f)
        {
            landingParticleSystem.Play();
            carAudioHandler.PlayLandingSFX();
        }

        isJumping = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Jump"))
        {
            JumpData jumpData = collision.GetComponent<JumpData>();
            Jump(jumpData.jumpHeightScale, jumpData.jumpPushScale);
        }
    }
}
