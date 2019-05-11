using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : MonoBehaviour
{

    public static Client client;
    #region MonoBehaviour
    // Start is called before the first frame update
    void Start()
    {
        client = LobbyScene.client;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion

    #region Buttons
    public void onClickPlayerReady()
    {
        client.sendReady();
    }
    #endregion
}
