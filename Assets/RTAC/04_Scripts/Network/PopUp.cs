using Mirror;

using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;

public class PopUp : NetworkBehaviour
{
    public TMP_Text popupText;

    [Command(requiresAuthority = false)]
    public void CmdPopupText(string _text) => RpcPopupText(_text);

    /// <summary>
    /// Popup text to display item status to all clients.
    /// </summary>
    /// <param name="_text">Message to display</param>
    [ClientRpc]
    public void RpcPopupText(string _text)
    {
        popupText.text = _text;
        popupText.gameObject.SetActive(true);
        Invoke(nameof(HidePopup),3);
    }

    /// <summary>
    /// Hides the popup.
    /// </summary>
    public void HidePopup() => popupText.gameObject.SetActive(false);

    private void Awake()
    {
        HidePopup();
    }
}
