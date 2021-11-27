using AltarChase.Networking;

using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviour
{
	[SerializeField, Tooltip("The button that generates the level and allows the host to start the game")] 
	private Button startButton;
	[SerializeField, Tooltip("The input field where the player puts their name")] 
	private TMP_InputField playerNameInputField;
	[SerializeField, Tooltip("The slider that determines the number of level sections that will be generated")] 
	private Slider levelLengthSlider;

	[SerializeField, Tooltip("The text that displays the number of sections the level length slider is currently at")] 
	private TextMeshProUGUI levelLengthSliderText;

	[SerializeField]
	private LevelGenerator levelGenerator;
	public bool IsHost => CustomNetworkManager.Instance.IsHost;

	private RunnerController player;
	
	private void Start()
	{
		//initialize all the variables and add all the listeners
		player = CustomNetworkManager.LocalPlayer;
		playerNameInputField.onValueChanged.AddListener(SetCharacterName);
		if(!IsHost)
		{
			levelLengthSlider.interactable = false;
			startButton.interactable = false;
		}
		else
		{
			levelGenerator = FindObjectOfType<LevelGenerator>();
			startButton.onClick.AddListener(StartMatch);
			levelLengthSlider.onValueChanged.AddListener(delegate { UpdateLevelLengthSliderText(); });
		}
	}

	/// <summary>
	/// change the character name
	/// </summary>
	public void SetCharacterName(string _name)
	{
		player.SetCharacterName(_name);
	}

	/// <summary>
	/// update the text to match the value of the level length slider
	/// </summary>
	public void UpdateLevelLengthSliderText()
	{
		levelLengthSliderText.text = Mathf.RoundToInt(levelLengthSlider.value).ToString();
	}

	/// <summary>
	/// close the lobby, generate the level and start the match
	/// </summary>
	private void StartMatch()
	{
		levelGenerator.RegenerateLevel(Mathf.RoundToInt(levelLengthSlider.value));
		MatchManager.instance.StartMatch();
		gameObject.SetActive(false);
	}
}
