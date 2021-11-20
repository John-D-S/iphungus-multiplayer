using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class Landmine : MonoBehaviour
{
    [SerializeField] private Light proximityLight;
    [SerializeField] private Color32 orange = new Color32(255,110,0,255);
    [SerializeField] private int activeBrightness = 2;
    [SerializeField] private float flickerTime = 0.5f, fastFlicker = 0.25f;
    [SerializeField] private ParticleSystem[] explosion;
    public int currentRoutine;
    private void Start()
    {
        proximityLight.color = Color.green;
        StartCoroutine(Stage0());
    }

    public IEnumerator Stage0()
    {
        proximityLight.color = Color.green;
        proximityLight.intensity = activeBrightness;
        yield break;
    }
    
    public IEnumerator Stage1()
    {
        StopCoroutine(Stage1());
        proximityLight.color = orange;
        proximityLight.intensity = activeBrightness;
        yield return new WaitForSeconds(fastFlicker);
        proximityLight.intensity = 0;
        yield return new WaitForSeconds(fastFlicker);
        if (currentRoutine == 1)
        {
            StartCoroutine(Stage1());
        }
    }

    public IEnumerator Boom()
    {
        currentRoutine = 2;
        proximityLight.color = Color.red;
        proximityLight.intensity = activeBrightness;
        yield return new WaitForSeconds(flickerTime);
        foreach (ParticleSystem shrapnel in explosion)
        {
            shrapnel.Play();
        }
        
        proximityLight.intensity = 0;
        yield return new WaitForSeconds(flickerTime);
        foreach (ParticleSystem shrapnel in explosion)
        {
            shrapnel.Stop();
        }
        StartCoroutine(Stage0());
    }
    
    public void Explosion()
    {
        StartCoroutine(Boom());
    }
}
