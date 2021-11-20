using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootFallTrigger : MonoBehaviour
{
    private FootFall parent;

    private void Start()
    {
        parent = GetComponentInParent<FootFall>();
    }

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(parent.PlateAction());
    }
}
