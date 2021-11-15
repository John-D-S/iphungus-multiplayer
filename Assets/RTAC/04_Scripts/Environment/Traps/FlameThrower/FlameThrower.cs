using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlameThrower : MonoBehaviour
{
    private ParticleSystem[][] flames;
    [SerializeField] private ParticleSystem[] flame1, flame2, flame3, flame4, flame5;
    [SerializeField] private GameObject[] hitBoxes;
    [SerializeField] private Light[] flamelights;
    [SerializeField] private int nextFlame;
    private float waitTime = 1.2f, ember = 1, blast = 4;


    private void Start()
    {
        flames = new[] {flame1, flame2, flame3, flame4, flame5};

        StartCoroutine(Burn(flames[nextFlame]));
    }

    private IEnumerator Burn(ParticleSystem[] flame)
    {
        while (true)
        {
            //initialisation
            ParticleSystem.VelocityOverLifetimeModule module = flame[0].velocityOverLifetime;
            module.speedModifier = new ParticleSystem.MinMaxCurve(0.2f);

            #region warmup

            flame[0].Play();
            flamelights[nextFlame].intensity = ember;
            yield return new WaitForSeconds(waitTime);

            #endregion

            #region FullFlame

            //light 
            flamelights[nextFlame].intensity = blast;

            //collider
            hitBoxes[nextFlame].SetActive(true);

            //flames
            module.speedModifier = new ParticleSystem.MinMaxCurve(1);
            foreach (ParticleSystem fireElement in flame)
            {
                fireElement.Play();
            }

            yield return new WaitForSeconds(waitTime);

            #endregion

            //light off
            flamelights[nextFlame].intensity = 0;
            //hitbox off
            hitBoxes[nextFlame].SetActive(false);

            //new flame cannot be the same as last flame
            nextFlame = Random.Range(0, flames.Length);

            foreach (ParticleSystem fireElement in flame)
            {
                fireElement.Stop();
            }

            flame = flames[nextFlame];
        }
    }

    public IEnumerator FullIgnition(ParticleSystem[][] allFlames)
    {
        for (int i = 0; i < allFlames.Length; i++)
        {
            ParticleSystem.VelocityOverLifetimeModule module = allFlames[i][0].velocityOverLifetime;
            module.speedModifier = new ParticleSystem.MinMaxCurve(0.2f);
            allFlames[i][0].Play();
            flamelights[i].intensity = ember;
        }

        yield return new WaitForSeconds(waitTime);

        for (int i = 0; i < allFlames.Length; i++)
        {
            ParticleSystem.VelocityOverLifetimeModule module = allFlames[i][0].velocityOverLifetime;
            module.speedModifier = new ParticleSystem.MinMaxCurve(1);

            flamelights[i].intensity = blast;
            foreach (ParticleSystem fire in allFlames[i])
            {
                fire.Play();
            }
        }
        yield return new WaitForSeconds(waitTime);
        
        //killing all flames
        for (int i = 0; i < allFlames.Length; i++)
        {
            foreach (ParticleSystem fireElement in allFlames[i])
            {
                fireElement.Stop();
            }
            flamelights[i].intensity = 0;
        }
        nextFlame = Random.Range(0, flames.Length);
        StartCoroutine(Burn(flames[nextFlame]));
    }
    

    public void FullIgnitionActive()
    {
        StopAllCoroutines();
        foreach (var fireSystem in flames)
        {
            foreach (var flame in fireSystem)
            {
                flame.Stop();
            }
        }
        StartCoroutine(FullIgnition(flames));
    }
}
