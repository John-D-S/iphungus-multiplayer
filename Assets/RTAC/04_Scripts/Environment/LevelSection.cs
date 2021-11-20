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
    [SerializeField] private PosRot tempFixedPosRot = new PosRot();

    private void OnValidate()
    {
        float oldPosRotX = nextPosRot.position.x;
        float oldPosRotZ = nextPosRot.position.z;
        float oldPosRotY = nextPosRot.position.y;
        Vector3 newPosition = new Vector3(oldPosRotZ, oldPosRotY, -oldPosRotX);
        tempFixedPosRot = new PosRot(newPosition, nextPosRot.yRot + 90);
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
