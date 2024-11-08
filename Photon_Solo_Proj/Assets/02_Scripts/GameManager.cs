using Photon.Pun;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] private TMP_InputField _chatIF;
    [SerializeField] private TMP_Text _chatTxt;
    [SerializeField] private Button _sendBtn;

    IEnumerator Start()
    {
        _sendBtn.onClick.AddListener(() => SendBtnClick());
        yield return new WaitForSeconds(0.2f);
        CreateRabbit();
    }

    private void SendBtnClick()
    {
        string msg = $"<color=#00ff00>[{PhotonNetwork.NickName}]</color> {_chatIF.text}";
        SendMessageByRPC(msg);
        _chatIF.text = "";
    }

    private void SendMessageByRPC(object msg)
    {
       photonView.RPC(nameof(DisplayMessage),RpcTarget.AllBufferedViaServer, msg);
    }

    private void CreateRabbit()
    {
        Vector3 pos = new Vector3(0, 0, 0);
        PhotonNetwork.Instantiate("Rabbit", pos, Quaternion.identity);
    }

    [PunRPC]
    private void DisplayMessage(string msg)
    {
        _chatTxt.text += (msg + "\n");
    }
    
}
