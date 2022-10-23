using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particules : MonoBehaviour
{
    ParticleSystem _particleSystem;

    private void Awake()
    {
        _particleSystem=GetComponent<ParticleSystem>(); 
    }

    private void Update()
    {
        
    }
}
