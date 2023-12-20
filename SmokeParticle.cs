using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeParticle : MonoBehaviour
{
    float particleEmmisionRate = 0;
    CarController carController;
    ParticleSystem smokeParticleSystem;
    ParticleSystem.EmissionModule particleEmmisionModule;

    private void Awake()
    {
        carController = GetComponentInParent<CarController>();
        smokeParticleSystem = GetComponent<ParticleSystem>();
        particleEmmisionModule = smokeParticleSystem.emission;

        particleEmmisionModule.rateOverTime = 0;
    }

    private void Update()
    {
        particleEmmisionRate = Mathf.Lerp(particleEmmisionRate, 0, Time.deltaTime * 5);
        particleEmmisionModule.rateOverTime = particleEmmisionRate;

        /*if (carController.IsTireScreeching(out float lateralVelocity, out bool isBraking))
        {
            if (isBraking)
            {
                particleEmmisionRate = 30;
            } else
            {
                particleEmmisionRate = Mathf.Abs(lateralVelocity) * 2;
            }
        }*/
        if (carController.accelerationInput > 0)
        {
            particleEmmisionRate = 30;
        } else
        {
            particleEmmisionRate = 0;
        }
    }
}
