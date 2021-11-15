using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrigger : MonoBehaviour
{
    private SpikeTrap parent;
    
    private void Start()
    {
        parent = GetComponentInParent<SpikeTrap>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
            StartCoroutine(parent.Shanking());
    }
}
