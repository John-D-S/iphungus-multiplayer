using Mirror;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

namespace AltarChase.Networking
{
	[RequireComponent(typeof(PlayerController))]
	public class NetworkPlayer : NetworkBehaviour
	{
		[SerializeField] private GameObject enemyToSpawn;
		[SyncVar(hook = nameof(OnSetColor)), SerializeField] private Color cubeColor;

		//rider doesn't recognise this as a serilizable type but it is
		[SerializeField] private SyncList<float> synchedFloats = new SyncList<float>();

		// SyncVarHooks get called in the order the VARIABLES are defined not the functions
		// In this case it is x, y, z not z, x, y
		// [SyncVar(hook = "SetX")] public float x;
		// [SyncVar(hook = "SetY")] public float y;
		// [SyncVar(hook = "SetZ")] public float z;
		//
		// [Command]
		// public void CmdSetPosition(float _x, float _y, float _z)
		// {
		//     z = _z;
		//     x = _x;
		//     y = _y;
		// }
		
		private Material cachedMaterial;
		
		//synchVarHooks typically have an On at the front of them
		private void OnSetColor(Color _old, Color _new)
		{
			if(cachedMaterial == null)
			{
				cachedMaterial = gameObject.GetComponent<MeshRenderer>().material;
			}

			cachedMaterial.color = _new;
		}
		
		private void Update()
		{
			Debug.Log(synchedFloats);
			MeshRenderer render = gameObject.GetComponent<MeshRenderer>();
			render.material.color = cubeColor;
			
			// First determine if this function is being run on the local player
			if(isLocalPlayer)
			{
				if(Input.GetKeyDown(KeyCode.Space))
				{
					// Run a function that tells every client to change the colour of this gameobject
					CmdRandomColor(Random.Range(0f, 1f));
				}

				if(Input.GetKeyDown(KeyCode.E))
				{
					CmdSpawnEnemy();			
				}
			}
		}

		[Command]
		public void CmdSpawnEnemy()
		{
			// NetworkServer.Spawn requires an instance of the object in the server's scene to be present
			// so if the object being spawned is a prefab, instantiate needs to be called first
			GameObject newEnemy = Instantiate(enemyToSpawn);
			NetworkServer.Spawn(newEnemy);
		}
		
		// Commands shoot from client to server
		// IMPORTANT - RULES FOR COMMANDS:
		// 1. Cannot return anything
		// 2. Must follow the correct naming convention: The function name MUST start with 'Cmd' exactly like that
		// 3. The function must have the attribute [Command] found in the Mirror namespace
		// 4. Can only be certain serializable types (see Command in the Mirror documentation at https://mirror-networking.gitbook.io/docs/guides/data-types)
		[Command]
		public void CmdRandomColor(float _hue)
		{
			//this is running on the server
			cubeColor = Random.ColorHSV(0, 1, 0.75f, 1, 1, 1);
		}
		
		// client RPCs shoot from server to client
		// IMPORTANT - RULES FOR CLIENT RPC:
		// 1. Cannot return anything
		// 2. Must follow the correct naming convention: The function name MUST start with 'Rpc' exactly like that
		// 3. The function must have the attribute [ClientRpc] found in the Mirror namespace
		// 4.Can only be certain serializable types (see Command in the Mirror documentation at https://mirror-networking.gitbook.io/docs/guides/data-types)
		/*[ClientRpc]
		public void RpcRandomColor(float _hue)
		{
			// This is running on every instance of the same object that the client was calling from.
			// i.e. Red GO on Red Client runs Cmd, Red GO on Red, Green and Blue client's run Rpc
			MeshRenderer rend = gameObject.GetComponent<MeshRenderer>();
			rend.material.color = Color.HSVToRGB(_hue, 1, 1);
		}*/

		// This is run via the network starting and the player connecting
		// NOT unity
		// It is run when the object is spawned via the networking system NOT when Unity
		// instantiates the object
		public override void OnStartLocalPlayer()
		{
			// this is run if we are the local player and NOT a remote player
		}

		// This is run via the network starting and the player connecting
		// NOT unity
		// It is run when the object is spawned via the networking system NOT when Unity
		// instantiates the object
		public override void OnStartClient()
		{
			// this is run Regardless if we are the local or remote player
			// isLocalPlayer is true if this object is the client's local player otherwise it's false
			PlayerController controller = gameObject.GetComponent<PlayerController>();
			controller.enabled = isLocalPlayer;
			
			CustomNetworkManager.AddPlayer(this);
		}

		public override void OnStopClient()
		{
			CustomNetworkManager.RemovePlayer(this);
		}

		// this runs when the server starts... On the server
		// In the case of a host-client situation, this only runs when the host launches because the host is the server
		// use this for initializing pooling systems
		public override void OnStartServer()
		{
			for(int i = 0; i < 10; i++)
			{
				synchedFloats.Add(Random.Range(0, 10));
			}
		}
	}
}
