using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSection : MonoBehaviour
{
    [SerializeField] private List<Sphere> localSpheres = new List<Sphere>();
    [SerializeField] private PosRot nextPosRot = new PosRot();
    public PosRot NextPosRot;

    public List<Sphere> SpheresWhenSectionAtPosRot(PosRot _posRot)
    {
        List<Sphere> returnVal = new List<Sphere>();
        foreach(Sphere sphere in localSpheres)
        {
            returnVal.Add(new Sphere(_posRot.position + _posRot.Rotation * (_posRot.position + sphere.position), sphere.radius));
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
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Vector3 spawnGizmoPos = transform.position + transform.rotation * nextPosRot.position;
        Gizmos.DrawSphere(spawnGizmoPos, 0.25f);
        Gizmos.DrawLine(spawnGizmoPos, transform.rotation * nextPosRot.Rotation * Vector3.forward + spawnGizmoPos);
    }
}
