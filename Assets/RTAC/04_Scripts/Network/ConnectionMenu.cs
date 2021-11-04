using AltarChase.Networking;

using kcp2k;

using Mirror;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace AltarChase
{
	public class ConnectionMenu : MonoBehaviour
	{
		private CustomNetworkManager networkManager;
		private KcpTransport transport;
		
		[SerializeField] private Button hostButton;
		[SerializeField] private TMP_InputField inputField;
		[SerializeField] private Button connectButton;

		[Space] 
		
		[SerializeField] private DiscoveredGame discoveredGameTemplate;

		private Dictionary<IPAddress, DiscoveredGame> discoveredGames = new Dictionary<IPAddress, DiscoveredGame>();
		
		private void Start()
		{
			networkManager = CustomNetworkManager.Instance;
			transport = Transport.activeTransport as KcpTransport;
			
			hostButton.onClick.AddListener(OnClickHost);
			inputField.onEndEdit.AddListener(OnEndEditAddress);
			connectButton.onClick.AddListener(OnClickConnect);

			CustomNetworkDiscovery discovery = networkManager.discovery;
		}

		private void OnClickHost() => networkManager.StartHost();
		private void OnEndEditAddress(string _value) => networkManager.networkAddress = _value;

		private void OnClickConnect()
		{
			string address = inputField.text.Trim((char)8203);
			ushort port = 7777;
			//if the address contains a colon, it has a port
			if(address.Contains(":"))
			{
				//get everything after the colon
				string portID = address.Substring(address.IndexOf(":", StringComparison.Ordinal) + 1);
				//turn it into a port
				port = ushort.Parse(portID);
				//remove the port from the address
				address = address.Substring(0, address.IndexOf(":", StringComparison.Ordinal));
			}
			
			if(!IPAddress.TryParse(address, out IPAddress ipAddress))
			{
				Debug.Log($"Invalid IP: {address}");
				address = "localhost";
			}
			
			transport.Port = port;
			networkManager.networkAddress = address;
			networkManager.StartClient();
		}

		private void OnFoundServer(DiscoveryResponse _response)
		{
			// Have we recieved a server that is bradcasting on the network that we haven't already found?
			if(!discoveredGames.ContainsKey(_response.EndPoint.Address))
			{
				//We haven't found this game already, so make the gameobject
				DiscoveredGame game = Instantiate(discoveredGameTemplate, discoveredGameTemplate.transform.parent);
				game.gameObject.SetActive(true);
				
				//Setup the game using the response and add it to the dictionary
				game.Setup(_response, networkManager, transport);
				discoveredGames.Add(_response.EndPoint.Address, game);
			}
		}
	}
}
