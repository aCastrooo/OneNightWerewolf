using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyScene : MonoBehaviour
{

    public static Client client;
    public void OnClickSendName()
    {
        string username = GameObject.Find("CreateUsername").GetComponent<TMP_InputField>().text;

        client = Client.Instance;
        DontDestroyOnLoad(client);
        client.SendNameCreate(username);
    }

    public static void gameStart()
    {
        SceneManager.LoadScene("Game");
    }
}
