using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemScript : MonoBehaviour
{
    private ParticleSystem particleSystem;

    void Start()
    {
        // Assuming the Particle System is attached to the same GameObject
        particleSystem = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        // Check if the particle system is not playing
        if (!particleSystem.isPlaying)
        {
            Destroy(transform.parent.gameObject);
        }
    }
}
