using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelTrailHandler : MonoBehaviour
{
    CarController carController;
    TrailRenderer trailRenderer;

    private void Awake()
    {
        carController = GetComponentInParent<CarController>();
        trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.emitting = false;
    }

    private void Update()
    {
        if (carController.IsTireScreeching(out float lateralVelocity, out bool isBraking) && !carController.isJumping)
        {
            trailRenderer.emitting = true;
        } else
        {
            trailRenderer.emitting = false;
        }
    }
}
