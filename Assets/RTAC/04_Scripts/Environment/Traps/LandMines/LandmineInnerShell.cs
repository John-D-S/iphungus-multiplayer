using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandmineInnerShell : MonoBehaviour
{
    private Landmine parentClass;
    
    private void Start()
    {
        parentClass = GetComponentInParent<Landmine>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            parentClass.Explosion();
        }
    }
}
