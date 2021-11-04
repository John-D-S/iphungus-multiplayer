using JetBrains.Annotations;

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

using System.Collections.Generic;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/
namespace AltarChase.Networking
{
	public class CustomNetworkManager : NetworkManager
	{
	    // Since there are multiple versions of StartServer, StartClient and StartHost, to reliably customize
	    // their functionality, users would need override all the versions. Instead these callbacks are invoked
	    // from all versions, so users only need to implement this one case.

	    /// <summary> a reference to the CustomNetworkManager version of the singleton </summary>
	    public static CustomNetworkManager Instance => singleton as CustomNetworkManager;

	    /// <summary> Attempts to find a player using the passed NetID, this can return null. </summary>
	    /// <param name="_id">The NetID of the player that we are trying to find</param>
	    [CanBeNull]
	    public static NetworkPlayer FindPlayer(uint _id)
	    {
		    Instance.players.TryGetValue(_id, out NetworkPlayer _player);
		    return _player;
	    }

	    /// <summary> Adds a player to the dictionary. </summary>
	    public static void AddPlayer([NotNull] NetworkPlayer _player) => Instance.players.Add(_player.netId, _player);

	    /// <summary> removes a player from the dictionary. </summary>
	    /// <param name="_player"></param>
	    public static void RemovePlayer([NotNull] NetworkPlayer _player) => Instance.players.Remove(_player.netId);
	    
	    private static NetworkPlayer localPlayer = null;
	    /// <summary> A reference to the localplayer of the game. </summary>
	    private static NetworkPlayer LocalPlayer
	    {
		    get
		    {
			    // if the internal localPlayer instance is null
			    if(localPlayer == null)
			    {
				    // loop through each player in the game and check if it is a local player
				    foreach(NetworkPlayer networkPlayer in Instance.players.Values)
				    {
					    if(networkPlayer.isLocalPlayer)
					    {
						    // Set local player to this player as it is the local player
						    localPlayer = networkPlayer;
						    break;
					    }
				    }
			    }
			    
			    // Return the cached local player
			    return localPlayer;
		    }
	    }
	    
	    public bool IsHost { get; private set; } = false;

	    public CustomNetworkDiscovery discovery;

	    private readonly Dictionary<uint, NetworkPlayer> players = new Dictionary<uint, NetworkPlayer>();
	    
	    /// <summary>
	    /// This is invoked when a host is started.
	    /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
	    /// </summary>
	    public override void OnStartHost()
	    {
		    IsHost = true;
		    // This makes it visible on the network
		    discovery.AdvertiseServer();
	    }

	    /// <summary>
	    /// This is called when a host is stopped.
	    /// </summary>
	    public override void OnStopHost()
	    {
		    IsHost = false;
	    }
	}
}
