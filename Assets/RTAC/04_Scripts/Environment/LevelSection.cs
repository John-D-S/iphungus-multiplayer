using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSection : MonoBehaviour
{
    [SerializeField] private List<Sphere> localSpheres = new List<Sphere>();

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
    
    
}
