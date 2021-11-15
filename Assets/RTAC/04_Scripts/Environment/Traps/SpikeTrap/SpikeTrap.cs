using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class SpikeTrap : MonoBehaviour
{
    [SerializeField] private Transform[] spikeExtensions;
    [SerializeField] private GameObject spikes;
    private int fastMoving = 12, slowMoving = 1, vibMax = 20;
    private float extensionPause = 0.5f;
    public bool resting;
    private bool complete;

    public IEnumerator Shanking()
    {
        resting = false;
        // initialisation
        float[] vibRange = new float[3];
        int vibCount = 0;
        Vector3 basePos = spikeExtensions[0].position;
        Vector3 shankPos = spikeExtensions[1].position;
        float minDist = 0.2f, shudderIncrease = 0.005f;

        void Vibration()
        {
            float vibCalc = minDist + shudderIncrease * vibCount;
            for (int i = 0; i < vibRange.Length; i++)
            {
                vibRange[i] = Random.Range(-vibCalc, vibCalc);
            }
            vibCount++;
            spikes.transform.position = new Vector3(basePos.x + vibRange[0], basePos.y + vibRange[1], 
                basePos.z + vibRange[2]);
        }

        void Movement(Vector3 currentPos, Vector3 endPoint, int speed)
        {
             spikes.transform.position = Vector3.Lerp(currentPos, endPoint, Time.fixedDeltaTime * speed);
        }

        //this trap has three stages, vibration, shanking, and reset
        //Vibration
        while (vibCount <= vibMax)
        {
            Vibration();
            yield return new WaitForFixedUpdate();
            if (vibCount > vibMax)
            {
                yield return null;
            }
        }
        
        //shanking
        spikes.transform.position = basePos;
        while (Vector3.Distance(spikes.transform.position, shankPos) > minDist && !complete)
        {
            Movement(spikes.transform.position, shankPos, fastMoving);
            yield return new WaitForFixedUpdate();
            if (Vector3.Distance(spikes.transform.position, shankPos) <= minDist)
            {
                complete = true;
                yield return null;
            }
        }

        yield return new WaitForSeconds(extensionPause);
        
        //resetting
        while (Vector3.Distance(spikes.transform.position, basePos) > minDist && complete)
        {
            Movement(spikes.transform.position, basePos, slowMoving);
            yield return new WaitForFixedUpdate();
            if (Vector3.Distance(spikes.transform.position, basePos) <= minDist)
            {
                complete = false;
                resting = true;
                yield return null;
            }
        }
    }
    
    public void Kill()
    {
        StartCoroutine(Shanking());
    }
}
