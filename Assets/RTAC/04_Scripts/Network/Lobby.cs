using AltarChase.Networking;

using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviour
{
	[SerializeField] private Button startButton;
	[SerializeField] private TMP_InputField playerNameInputField;
	[SerializeField] private Slider levelLengthSlider;

	[SerializeField] private TextMeshProUGUI levelLengthSliderText;

	private LevelGenerator levelGenerator;
	public bool IsHost => CustomNetworkManager.Instance.IsHost;

	private void Start()
	{
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

	public void SetCharacterName(string _name)
	{
		RunnerController localPlayer = CustomNetworkManager.LocalPlayer;
		localPlayer.CmdSetCharacterName(_name);
	}

	public void UpdateLevelLengthSliderText()
	{
		levelLengthSliderText.text = Mathf.RoundToInt(levelLengthSlider.value).ToString();
	}

	private void StartMatch()
	{
		SetCharacterName(playerNameInputField.text);
		levelGenerator.RegenerateLevel(Mathf.RoundToInt(levelLengthSlider.value));
		MatchManager.instance.StartMatch();
		gameObject.SetActive(false);
	}
}
