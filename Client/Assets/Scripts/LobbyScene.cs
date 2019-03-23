using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class LobbyScene : MonoBehaviour
{
    public void OnClickSendName()
    {
        string username = GameObject.Find("CreateUsername").GetComponent<TMP_InputField>().text;

        Client.Instance.SendNameCreate(username);
    }
    /*
    public void OnClickStartGame()
    {
        Client.Instance.StartGame();
    }
    */
}
