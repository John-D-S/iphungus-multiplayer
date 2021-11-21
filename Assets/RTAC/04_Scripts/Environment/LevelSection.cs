using Mirror;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class LevelSection : NetworkBehaviour
{
    [SerializeField] private List<Sphere> localSpheres = new List<Sphere>();
    [SerializeField] private PosRot nextPosRot = new PosRot();
    public PosRot NextPosRot => nextPosRot;


    /*
    [SerializeField] private bool fixScale = false;
    [SerializeField, HideInInspector] private bool alreadyFixed = false;

    private void OnValidate()
    {
        if(fixScale && !alreadyFixed)
        {
            for(int i = 0; i < localSpheres.Count; i++)
            {
                localSpheres[i] = new Sphere(localSpheres[i].position * 2.5f, localSpheres[i].radius * 2.5f);
            }
            nextPosRot = new PosRot(nextPosRot.position * 2.5f, nextPosRot.yRot);
            alreadyFixed = true;
        }
    }
    */  

    public List<Sphere> SpheresWhenSectionAtPosRot(PosRot _posRot)
    {
        List<Sphere> returnVal = new List<Sphere>();
        foreach(Sphere sphere in localSpheres)
        {
            returnVal.Add(new Sphere(_posRot.position + _posRot.Rotation * sphere.position, sphere.radius));
        }
        return returnVal;
    }
    
    public List<Sphere> GlobalSpheres
    {
        get
        {
            List<Sphere> returnValues = new List<Sphere>();
            foreach(Sphere localSphere in localSpheres)
            {
                returnValues.Add(new Sphere(transform.rotation * localSphere.position + transform.position, localSphere.radius));
            }
            return returnValues;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 spawnGizmoPos = transform.position + transform.rotation * nextPosRot.position;
        Gizmos.DrawSphere(spawnGizmoPos, 0.25f);
        Gizmos.DrawLine(spawnGizmoPos, transform.rotation * nextPosRot.Rotation * Vector3.forward + spawnGizmoPos);

        Gizmos.color = new Color(0, 1, 0, 0.5f);
        foreach(Sphere localSphere in localSpheres)
        {
            Vector3 spherePos = transform.position + transform.rotation * localSphere.position;
            Gizmos.DrawSphere(spherePos, localSphere.radius);
        }
    }
}
