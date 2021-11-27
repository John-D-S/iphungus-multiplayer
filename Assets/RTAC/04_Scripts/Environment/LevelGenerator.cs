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
	[Header("-- Level Section Settings --")]
	[SerializeField, Tooltip("The sections that can be placed at the start of the course")] 
	private List<StartSection> startSections;
	[SerializeField, Tooltip("The sections which do not have any particular functionality")] 
	private List<NormalSection> normalSections; 
	[SerializeField, Tooltip("The sections which have traps on them.")] 
	private List<TrapSection> trapSections;
	[SerializeField, Tooltip("The sections which have checkpoints on them")] 
	private List<CheckpointSection> checkpointSections;
	[SerializeField, Tooltip("The sections that can be placed at the end of the course. They should have a finishline")] 
	private List<EndSection> endSections;
	[SerializeField, Tooltip("after the first tile and before the last tile, the track will place tiles in this order, repeating.")] 
	private List<LevelSectionType> tileOrders;
	//[SerializeField] private int numberOfSections = 20;

	//the level sections that will be placed
	private List<LevelSection> levelSectionsToPlace;

	/// <summary>
	/// The pos rots of a chain of level sections if they were connected to each other
	/// </summary>
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

	// these variables are not in the NoIntersectingSpheres function so that they can be displayed with gizmos
	private List<PosRot> apparentLevelSectionPosRots = new List<PosRot>();
	private List<Sphere> nonCollidingSpheres = new List<Sphere>();
	private List<Sphere> collidingSpheres = new List<Sphere>();
	
	/// <summary>
	/// returns true if the list of level sections has no intersecting spheres
	/// </summary>
	private bool NoIntersectingSpheres(List<LevelSection> _levelSections)
	{
		//clear colliding spheres and posrots
		nonCollidingSpheres.Clear();
		collidingSpheres.Clear();
		apparentLevelSectionPosRots.Clear();
		//set the posRots of the level sections to use later
		List<PosRot> levelSectionPosRots = LevelSectionPosRots(_levelSections);
		apparentLevelSectionPosRots = levelSectionPosRots;
		
		//set the las section index
		int lastSectionIndex = _levelSections.Count - 1;
		//check each sphere in each level section against each other sphere in all the other level sections
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
	
	/// <summary>
	/// calculate the sequence of tiles that should be placed into the world
	/// </summary>
	/// <param name="_numberOfSections">how many sections should be placed</param>
	private void CalculateSections(int _numberOfSections)
	{
		//a function that recursively places random sections in the right sequence such that there are no intersecting sections
		bool TryPlaceSections(ref List<LevelSection> _currentSections, int _targetNoOfIterations)
		{
			//set the number of recursions this function has already gone through
			int noOfIterations = _currentSections.Count;
			//if the number of iterations is greater than the target number of iterations, the function is complete, so return true
			if(noOfIterations > _targetNoOfIterations)
			{
				return true;
			}
			List<LevelSection> possibleSections = new List<LevelSection>();
			//if the current number of iterations is 0, the only tiles that can be placed are start sections,
			if(noOfIterations == 0)
			{
				foreach(StartSection startSection in startSections)
					possibleSections.Add(startSection);
			}
			//if the current number of iterations is equal to the target number, only an endsection can be placed
			else if(noOfIterations == _targetNoOfIterations)
			{
				foreach(EndSection endSection in endSections)
					possibleSections.Add(endSection);
			}
			else
			{
				//add the appropriate collection of section types according to the number of iterations and tileOrders
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
			
			// shuffle the possible sections so that each one is tred in a random order 
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
		TryPlaceSections(ref levelSectionsToPlace, _numberOfSections);
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

	//place the level sections online for everyone to see
	private void PlaceLevelSectionsOnline()
	{
		//get the level posrots to place the levelsections at.
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

	public void RegenerateLevel(int _numberOfSections)
	{
		//calculate the sections, and place them
		CalculateSections(_numberOfSections);
		PlaceLevelSectionsOnline();
	}
}	