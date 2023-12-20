using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAudioHandler : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource tireScreechingAudioSource;
    public AudioSource engineAudioSource;
    public AudioSource carHitAudioSource;
    public AudioSource carJumpAudioSource;
    public AudioSource carLandAudioSource;
    
    CarController carController;

    float desiredEnginePitch = 0.5f;
    float tireScreechPitch = 0.5f;

    private void Awake()
    {
        carController = GetComponentInParent<CarController>();
    }

    private void Update()
    {
        UpdateEngineSounds();
        UpdateTireSounds();
    }

    void UpdateEngineSounds()
    {
        float velocityMagnitude = carController.GetVelocityMagnitude();

        float desiredEngineVolume = velocityMagnitude * 0.05f;
        desiredEngineVolume = Mathf.Clamp(desiredEngineVolume, 0.2f, 1.0f);

        engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, desiredEngineVolume, Time.deltaTime * 10);

        desiredEnginePitch = velocityMagnitude * 0.2f;
        desiredEnginePitch = Mathf.Clamp(desiredEnginePitch, 0.5f, 2f);
        engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, desiredEnginePitch, Time.deltaTime * 1.5f);
    }

    void UpdateTireSounds()
    {
        if (carController.IsTireScreeching(out float lateralVelocity, out bool isBraking))
        {
            if (isBraking)
            {
                tireScreechingAudioSource.volume = Mathf.Lerp(tireScreechingAudioSource.volume, 1.0f, Time.deltaTime * 10);
                tireScreechPitch = Mathf.Lerp(tireScreechPitch, 0.5f, Time.deltaTime * 10);
            } else
            {
                tireScreechingAudioSource.volume = Mathf.Abs(lateralVelocity) * 0.05f;
                tireScreechPitch = Mathf.Abs(lateralVelocity) * 0.1f;
            }
        } else
        {
            tireScreechingAudioSource.volume = Mathf.Lerp(tireScreechingAudioSource.volume, 0, Time.deltaTime * 10);
        }
    }

    public void PlayJumpSFX()
    {
        carJumpAudioSource.Play();
    }

    public void PlayLandingSFX()
    {
        carLandAudioSource.Play();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float relativeVelocity = collision.relativeVelocity.magnitude;

        float volume = relativeVelocity * 0.1f;

        carHitAudioSource.pitch = Random.Range(0.95f, 1.05f);
        carHitAudioSource.volume = volume;

        if (!carHitAudioSource.isPlaying)
        {
            carHitAudioSource.Play();
        }
    }
}
