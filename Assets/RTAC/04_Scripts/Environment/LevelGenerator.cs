using Mirror;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using UnityEngine;

using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public enum LevelSectionType
{
	Normal,
	Trap,
	Checkpoint
}

[System.Serializable]
public struct PosRot
{
	public Vector3 position;
	public float yRot;
	public Quaternion Rotation => Quaternion.Euler(0, yRot, 0);
	
	public PosRot(Vector3 _position, float _yRot)
	{
		position = _position;
		yRot = _yRot;
	}
	
	public static PosRot operator +(PosRot a, PosRot b) => new PosRot(a.position + a.Rotation * b.position, a.yRot + b.yRot);
}

public class LevelGenerator : NetworkBehaviour
{
	[SerializeField] private List<StartSection> startSections;
	[SerializeField] private List<NormalSection> normalSections; 
	[SerializeField] private List<TrapSection> trapSections;
	[SerializeField] private List<CheckpointSection> checkpointSections;
	[SerializeField] private List<EndSection> endSections;
	[SerializeField] private List<LevelSectionType> tileOrders;
	[SerializeField] private int numberOfSections = 20;

	private List<LevelSection> levelSectionsToPlace;

	private List<PosRot> LevelSectionPosRots(List<LevelSection> _levelSections)
	{
		List<PosRot> returnValue = new List<PosRot>();
		PosRot lastPosRot = new PosRot(Vector3.zero, 0);
		returnValue.Add(lastPosRot);
		foreach(LevelSection levelSection in _levelSections)
		{
			returnValue.Add(lastPosRot + levelSection.NextPosRot);
			lastPosRot = returnValue[returnValue.Count - 1];
		}
		return returnValue;
	}

	private List<PosRot> apparentLevelSectionPosRots = new List<PosRot>();
	private List<Sphere> nonCollidingSpheres = new List<Sphere>();
	private List<Sphere> collidingSpheres = new List<Sphere>();
	private bool NoIntersectingSpheres(List<LevelSection> _levelSections)
	{
		nonCollidingSpheres.Clear();
		collidingSpheres.Clear();
		apparentLevelSectionPosRots.Clear();
		List<PosRot> levelSectionPosRots = LevelSectionPosRots(_levelSections);
		apparentLevelSectionPosRots = levelSectionPosRots;
		/*
		int i = 0;
		foreach(LevelSection levelSection in _levelSections)
		{
			foreach(Sphere sphere in levelSection.SpheresWhenSectionAtPosRot(levelSectionPosRots[i]))
			{
				apparentSpheres.Add(sphere);
			}
			i++;
		}
		*/
		int lastSectionIndex = _levelSections.Count - 1;
		foreach(Sphere sphere in _levelSections[lastSectionIndex].SpheresWhenSectionAtPosRot(levelSectionPosRots[lastSectionIndex]))
		{
			//We don't check to see if the last level section is intersecting with itself or the section before it.
			for(int i = 0; i < _levelSections.Count - 2; i++)
			{
				foreach(Sphere otherSphere in _levelSections[i].SpheresWhenSectionAtPosRot(levelSectionPosRots[i]))
				{
					if(sphere.IntersectsWithSphere(otherSphere))
					{
						collidingSpheres.Add(otherSphere);
						return false;
					}
					else
					{
						nonCollidingSpheres.Add(otherSphere);
					}
				} 
			}
		}
		return true;
	}
	
	private void CalculateSections()
	{
		bool TryPlaceSections(ref List<LevelSection> _currentSections, int _targetNoOfIterations)
		{
			int noOfIterations = _currentSections.Count;
			if(noOfIterations > _targetNoOfIterations)
			{
				return true;
			}
			List<LevelSection> possibleSections = new List<LevelSection>();
			if(noOfIterations == 0)
			{
				foreach(StartSection startSection in startSections)
					possibleSections.Add(startSection);
			}
			else if(noOfIterations == _targetNoOfIterations - 1)
			{
				foreach(EndSection endSection in endSections)
					possibleSections.Add(endSection);
			}
			else
			{
				LevelSectionType sectionType = tileOrders[noOfIterations % tileOrders.Count];
				switch(sectionType)
				{
					case LevelSectionType.Normal:
						foreach(NormalSection normalSection in normalSections)
							possibleSections.Add(normalSection);
						break;
					case LevelSectionType.Trap:
						foreach(TrapSection trapSection in trapSections)
							possibleSections.Add(trapSection);
						break;
					case LevelSectionType.Checkpoint:
						foreach(CheckpointSection checkpointSection in checkpointSections)
							possibleSections.Add(checkpointSection);
						break;
				}
			}
			
			List<LevelSection> shuffledPossibleSections = possibleSections.OrderBy(i => Guid.NewGuid()).ToList();
			
			for(int i = 0; i < shuffledPossibleSections.Count; i++)
			{
				//Debug.Log(_currentSections.Count);
				_currentSections.Add(shuffledPossibleSections[i]);
				if(NoIntersectingSpheres(_currentSections))
				{
					//Debug.Log($"No Intersecting Spheres");
					if(TryPlaceSections(ref _currentSections, _targetNoOfIterations))
					{
						return true;
					}
				}
				_currentSections.RemoveAt(_currentSections.Count - 1);
			}
			
			return false;
		}

		levelSectionsToPlace = new List<LevelSection>();
		TryPlaceSections(ref levelSectionsToPlace, numberOfSections);
	}

	private void PlaceLevelSectionsOffline()
	{
		List<PosRot> allLevelPosRots = LevelSectionPosRots(levelSectionsToPlace);
		for(int i = 0; i < levelSectionsToPlace.Count; i++)
		{
			LevelSection sectionToPlace = levelSectionsToPlace[i];
			Vector3 position = allLevelPosRots[i].position;
			Quaternion rotation = allLevelPosRots[i].Rotation;
			GameObject gameObjectToInstantiate = sectionToPlace.gameObject;
			GameObject instantiatedGameObject = Instantiate(gameObjectToInstantiate, position, rotation, transform);
			Debug.Log(instantiatedGameObject);
		}
	}

	private void PlaceLevelSectionsOnline()
	{
		List<PosRot> allLevelPosRots = LevelSectionPosRots(levelSectionsToPlace);
		for(int i = 0; i < levelSectionsToPlace.Count; i++)
		{
			LevelSection sectionToPlace = levelSectionsToPlace[i];
			Vector3 position = allLevelPosRots[i].position;
			Quaternion rotation = allLevelPosRots[i].Rotation;
			GameObject gameObjectToInstantiate = sectionToPlace.gameObject;
			GameObject instantiatedGameObject = Instantiate(gameObjectToInstantiate, position, rotation, transform);
			NetworkServer.Spawn(instantiatedGameObject);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		foreach(PosRot posRot in apparentLevelSectionPosRots)
		{
			Gizmos.DrawSphere(posRot.position, 0.25f);
		}
		Gizmos.color = Color.green;
		foreach(Sphere nonCollidingSphere in nonCollidingSpheres)
		{
			Gizmos.DrawSphere(nonCollidingSphere.position, nonCollidingSphere.radius);
		}
		Gizmos.color = Color.red;
		foreach(Sphere collidingSphere in collidingSpheres)
		{
			Gizmos.DrawSphere(collidingSphere.position, collidingSphere.radius);
		}
	}

	private void Start()
	{
		CalculateSections();
		PlaceLevelSectionsOnline();
	}
}	