using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpOverFix : MonoBehaviour
{
    private Collider2D theCollider;
    private GameObject[] jumpOver;
    CarController carController;

    private void Awake()
    {
        theCollider = GetComponent<Collider2D>();
        carController = GetComponentInParent<CarController>();
        jumpOver = GameObject.FindGameObjectsWithTag("JumpOver");
    }

    private void Update()
    {
        if (carController.isJumping)
        {
            foreach (GameObject noCollide in jumpOver)
            {
                noCollide.GetComponent<Collider2D>().enabled = false;
            }
        } else
        {
            foreach (GameObject noCollide in jumpOver)
            {
                if (noCollide.GetComponent<Collider2D>().enabled)
                {
                    break;
                } else
                {
                    noCollide.GetComponent<Collider2D>().enabled = true;
                }
            }
        }
    }
}
