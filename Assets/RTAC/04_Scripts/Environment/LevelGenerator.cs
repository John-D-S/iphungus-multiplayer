using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
	[SerializeField] private List<StartSection> startSections;
	[SerializeField] private List<NormalSection> normalSections; 
	[SerializeField] private List<TrapSection> levelSections;
	[SerializeField] private List<CheckpointSection> checkpointSections;
	[SerializeField] private List<EndSection> endSections;

	private void GenerateLevel()
	{
	}

	private void Start()
	{
		
	}
}