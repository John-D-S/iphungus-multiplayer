using AltarChase.Networking;

using Mirror;

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using UnityEngine;
using UnityEngine.SceneManagement;

using Debug = UnityEngine.Debug;

public class MatchManager : NetworkBehaviour
{
    public static MatchManager instance = null;

    [SyncVar(hook  = nameof(StartMatch))] private bool matchStarted = false;
    [SyncVar] public bool matchFinished = false;
    
    public void StartMatch()
    {
        CmdStartMatch();
    }

    [Command(requiresAuthority = false)]
    public void CmdStartMatch()
    {
        matchStarted = true;
    }
    
    private void StartMatch(bool _old, bool _new)
    {
        if(_new)
        {
            SceneManager.UnloadSceneAsync("Lobby");

            SetCursorLock(true);
            RunnerController player = CustomNetworkManager.LocalPlayer;
            player.ReturnToLastCheckpoint();
        }
    }

    /// <summary>
    /// Stops the Host and Server and Loads the Main Menu.
    /// </summary>
    /// <param name="_seconds">The amount of time in seconds until the main menu scene loads.</param>
    [Command(requiresAuthority = false)]
    public void CmdCallLoadMainMenu(int _seconds) => Invoke(nameof(RpcLoadMainMenu), _seconds);

    [ClientRpc]
    public void RpcLoadMainMenu()
    {
        SetCursorLock(false);
        CustomNetworkManager.Instance.StopHost();
        //CustomNetworkManager.Instance.StopServer();
        SceneManager.LoadScene("Offline Scene");
    }

    [ClientRpc]
    public void RpcSetCursorLock(bool _locked) => SetCursorLock(_locked);

    public void SetCursorLock(bool _locked)
    {
        Cursor.visible = !_locked;
        Cursor.lockState = _locked? CursorLockMode.Locked : CursorLockMode.None;
    }

    protected void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // Anything else you want to do in awake
        
    }   
}
