using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandmineOuterShell : MonoBehaviour
{
    private Landmine parentClass;
    private void Start()
    {
        parentClass = GetComponentInParent<Landmine>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        parentClass.StartCoroutine(parentClass.Stage1());
        parentClass.currentRoutine = 1;
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            parentClass.StartCoroutine(parentClass.Stage0());
            parentClass.currentRoutine = 0;
        }

    }
}
