using AltarChase.Networking;

using Mirror;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchManager : NetworkBehaviour
{
    public static MatchManager instance = null;

    [SyncVar(hook  = nameof(OnRecievedMatchStarted))] public bool matchStarted = false;

    public void StartMatch()
    {
        if(hasAuthority)
        {
            CmdStartMatch();
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdStartMatch()
    {
        matchStarted = true;
    }
    
    private void OnRecievedMatchStarted(bool _old, bool _new)
    {
        if(_new)
        {
            SceneManager.UnloadSceneAsync("Lobby");

            SetCursorLock(true);
            RunnerController player = CustomNetworkManager.LocalPlayer;
            Transform startPos = CustomNetworkManager.Instance.GetStartPosition();
            player.transform.position = new Vector3(startPos.position.x, startPos.position.y + 2, startPos.position.z);
            player.transform.rotation = startPos.rotation;
            player.ZeroVelocity();
        }
    }

    /// <summary>
    /// Stops the Host and Server and Loads the Main Menu.
    /// </summary>
    /// <param name="_seconds">The amount of time in seconds until the main menu scene loads.</param>
    public void CallLoadMainMenu(int _seconds) => Invoke(nameof(RpcLoadMainMenu), _seconds);

    [ClientRpc]
    public void RpcLoadMainMenu()
    {
        SetCursorLock(false);
        CustomNetworkManager.Instance.StopHost();
        //CustomNetworkManager.Instance.StopServer();
        SceneManager.LoadScene("MainMenu");
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
