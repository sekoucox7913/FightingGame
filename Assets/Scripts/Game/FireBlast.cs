using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBlast : MonoBehaviour
{
    [SerializeField]
    private new ParticleSystem particleSystem;
    public void EnableParticles()
    {
        gameObject.SetActive(true);
        particleSystem.Play();
    }

    public void DisableParticles()
    {
        particleSystem.Stop();
        gameObject.SetActive(false);
    }
}
