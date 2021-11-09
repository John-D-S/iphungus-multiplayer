using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrapPart
{
    public GameObject trapPartObject;
    public Vector3 trapStartLocalPos;
	public Vector3 trapActivatedLocalPos;
	public float partLerpspeed = 0.25f;
}

public class Trap : MonoBehaviour
{
    public List<TrapPart> trapParts = new List<TrapPart>();

    private bool trapActivated;

    public void Activate() => trapActivated = true;
    public void Deactivate() => trapActivated = false;

    public void Update()
    {
	    if(Input.GetKeyDown(KeyCode.T))
	    {
		    trapActivated = !trapActivated;
		    Debug.Log($"Trap Activated: {trapActivated}");
	    }
    }

    private void FixedUpdate()
    {
	    foreach(TrapPart trapPart in trapParts)
	    {
		    if(trapPart.trapPartObject)
		    {
			    Vector3 partParentPosition = trapPart.trapPartObject.transform.parent.position;
			    Quaternion partParentRotation = trapPart.trapPartObject.transform.parent.rotation;
			    Vector3 partTargetPosition = partParentPosition + partParentRotation * (trapActivated ? trapPart.trapStartLocalPos : trapPart.trapActivatedLocalPos);
			    trapPart.trapPartObject.transform.position = Vector3.Lerp(trapPart.trapPartObject.transform.position, partTargetPosition, trapPart.partLerpspeed);
		    }
	    }
    }

    private void OnValidate()
    {
	    foreach(TrapPart trapPart in trapParts)
	    {
		    if(trapPart.trapPartObject)
		    {
			    Vector3 partParentPosition = trapPart.trapPartObject.transform.parent.position;
			    Quaternion partParentRotation = trapPart.trapPartObject.transform.parent.rotation;
				trapPart.trapPartObject.transform.position = partParentPosition + partParentRotation * trapPart.trapStartLocalPos;
		    }
	    }
    }

    private void OnDrawGizmos()
    {
	    foreach(TrapPart trapPart in trapParts)
	    {
		    Vector3 partParentPosition = trapPart.trapPartObject.transform.parent.position;
		    Quaternion partParentRotation = trapPart.trapPartObject.transform.parent.rotation;
		    Gizmos.color = Color.red;
		    Gizmos.DrawSphere(partParentPosition + partParentRotation * trapPart.trapStartLocalPos, 0.25f);
		    Gizmos.color = Color.green;
		    Gizmos.DrawSphere(partParentPosition + partParentRotation * trapPart.trapActivatedLocalPos, 0.25f);
		    Gizmos.color = Color.grey;
	    }
    }
}
