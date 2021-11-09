using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sphere
{
	public Vector3 position;
	public float radius;
	
	public Sphere(Vector3 _position, float _radius)
	{
		position = _position;
		radius = _radius;
	}

	private bool IntersectsWithSphere(Sphere other)
	{
		if(Vector3.Distance(position, other.position) > radius + other.radius)
			return true;
		return false;
	}
	
}
