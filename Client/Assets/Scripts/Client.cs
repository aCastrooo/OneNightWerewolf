using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Client : MonoBehaviour
{
    public static Client Instance { set; get; }
    public GameObject playerTextPrefab;

    private byte reliableChannel;
    private byte error;

    private int connectionId;
    private int hostId;

    //For the server connection
    private bool isStarted;
    //For the game instance
    private static bool gameStarted;
    private bool shuffleCards;
    private bool turn;

    private Button startGameButton;

    private string playerCard;
    private string myUsername;

    private string[] middleCards;

    private const int MAX_USER = 10;
    private const int PORT = 31415;
    private const int WEB_PORT = 31414;
    private const int BYTE_SIZE = 1024;
    private string SERVER_IP;

    private List<string> localPlayers;

    #region Monobehaviour
    private void Start()
    {
        Instance = this;
        gameStarted = false;
        turn = true;

        DontDestroyOnLoad(gameObject);
        SERVER_IP = "192.168.1.112";
        Init();

        localPlayers = new List<string>();
        try
        {
            startGameButton = GameObject.Find("StartGameButton").GetComponent<Button>();
        }
        catch (System.NullReferenceException ex)
        {
            Debug.Log("StartGameButton not found.");
        }
    }

    private void Update()
    {
        UpdateMessagePump();
        if (localPlayers.Count < 4)
        {
            //startGameButton.interactable = false;
        }

        if (shuffleCards)
        {
            float speed = 5.0f;
            Vector3 target = new Vector3(71.01112f, 311.769f, -297.632f);

            GameObject currCard = null;
            if (playerCard.Equals("Villager") || playerCard.Equals("Mason") || playerCard.Equals("Werewolf"))
            {
                GameObject[] taggedCards = GameObject.FindGameObjectsWithTag(playerCard);
                currCard = taggedCards[0];
            }
            else
            {
                currCard = GameObject.Find(playerCard);
            }
            
            Vector3 currPos = currCard.transform.position;
            float step = speed * Time.deltaTime;
            currCard.transform.position = Vector3.Lerp(currPos, target, step);
        }

    }
    #endregion

    public void Init()
    {
        if (!gameStarted)
        {
            NetworkTransport.Init();

            ConnectionConfig cc = new ConnectionConfig();
            reliableChannel = cc.AddChannel(QosType.Reliable);

            HostTopology topo = new HostTopology(cc, MAX_USER);

            //Client
            hostId = NetworkTransport.AddHost(topo, 0);

#if UNITY_WEBGL && !UNITY_EDITOR
        //web
        connectionId = NetworkTransport.Connect(hostId, SERVER_IP, WEB_PORT, 0, out error);
        Debug.Log("Connecting from web.");
#else
            connectionId = NetworkTransport.Connect(hostId, SERVER_IP, PORT, 0, out error);
            Debug.Log("Connecting from a client.");
#endif

            Debug.Log(string.Format("Attempting to connect to server: {0}", SERVER_IP));
            isStarted = true;
        }
    }
    public void Shutdown()
    {
        isStarted = false;
        NetworkTransport.Shutdown();
    }


    private void UpdateMessagePump()
    {
        if (!isStarted)
            return;

        int recHostId;
        int connectionId;
        int channelId;

        byte[] recBuffer = new byte[BYTE_SIZE];
        int dataSize;

        NetworkEventType type = NetworkTransport.Receive(out recHostId,
                                                         out connectionId,
                                                         out channelId,
                                                         recBuffer,
                                                         BYTE_SIZE,
                                                         out dataSize,
                                                         out error);
        switch (type)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log(string.Format("Server connection established", connectionId));
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log(string.Format("Disconnected from the server", connectionId));
                break;
            case NetworkEventType.DataEvent:
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(recBuffer);
                NetMsg msg = (NetMsg)formatter.Deserialize(ms);
                OnData(connectionId, channelId, recHostId, msg);
                Debug.Log(msg.OP);
                break;
            default:
                Debug.Log("???");
                break;
        }

    }


    #region SendRequests
    public void SendServer(NetMsg msg)
    {
        byte[] buffer = new byte[BYTE_SIZE];

        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, msg);

        NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, BYTE_SIZE, out error);
    }

    public void SendNameCreate(string username)
    {
        Net_CreateAcc ca = new Net_CreateAcc();
        ca.Username = username;
        myUsername = username;

        SendServer(ca);
        Button submitNameButt = GameObject.Find("SendNameButton").GetComponent<Button>();
        submitNameButt.interactable = false;
    }

    public void SendStartGame()
    {
        Net_StartGame sg = new Net_StartGame();
        sg.gameStart = true;

        Debug.Log("Starting game!");

        SendServer(sg);
    }

    public void sendReady()
    {
        Net_ReadyUp msg = new Net_ReadyUp();
        msg.ready = true;
        SendServer(msg);

        Button readyButton = GameObject.Find("ReadyButton").GetComponent<Button>();
        readyButton.interactable = false;
    }
    #endregion

    #region OnData
    private void OnData(int conId, int chanId, int recHostId, NetMsg msg)
    {
        
        switch (msg.OP)
        {
            case NetOP.PlayerList:
                addPlayer(msg);
                break;
            case NetOP.RemovePlayer:
                removePlayer(msg);
                break;
            case NetOP.SendCard:
                setupCards(msg);
                break;
            case NetOP.StartGame:
                loadGameScene();
                break;
            default:
                Debug.Log("Nothing to see");
                break;

        }
    }

    private void removePlayer(NetMsg msg)
    {
        Net_RemovePlayer rm = (Net_RemovePlayer)msg;
        string playerToRm = rm.playerToRemove;
        GameObject currPlayers = GameObject.Find("CurrPlayers");

        localPlayers.Remove(playerToRm);
        Transform player = currPlayers.transform.Find(playerToRm);
        player.parent = null;
        Destroy(player.gameObject);
    }

    private void setupCards(NetMsg msg)
    {
        Net_Card cardMsg = (Net_Card)msg;
        Debug.Log(string.Format("Got card!"));
        Debug.Log(string.Format("My card is {0}", cardMsg.card));

        string card = cardMsg.card;
        playerCard = card;
        shuffleCards = true;
    }

    private void addPlayer(NetMsg msg)
    {
        Net_PlayerList playerListMsg = (Net_PlayerList)msg;
        string[] playerList = playerListMsg.playerList;
        GameObject currPlayers = GameObject.Find("CurrPlayers");

        foreach (string player in playerList)
        {
            if (!player.Equals(""))
            {
                Debug.Log(player);
                //Logic for taking care of players already in the list
                if (!localPlayers.Contains(player))
                {
                    localPlayers.Add(player);

                    GameObject newPlayerName = Instantiate(playerTextPrefab) as GameObject;
                    newPlayerName.name = player;
                    newPlayerName.transform.SetParent(currPlayers.GetComponent<Transform>());
                    newPlayerName.GetComponent<TMP_Text>().text = player;
                    newPlayerName.transform.SetSiblingIndex(1);

                    float currHeight = currPlayers.GetComponent<RectTransform>().rect.height;
                    currPlayers.GetComponent<RectTransform>().sizeDelta = new Vector2(600, currHeight + 100);
                }
            }
        }
    }

    private static void loadGameScene()
    {
        LobbyScene.gameStart();
        gameStarted = true;
    }
    #endregion
}


public class IPManager
{
    public static string GetIP(ADDRESSFAM Addfam)
    {
        //Return null if ADDRESSFAM is Ipv6 but Os does not support it
        if (Addfam == ADDRESSFAM.IPv6 && !Socket.OSSupportsIPv6)
        {
            return null;
        }

        string output = "";

        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
            NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

            if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
#endif 
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    //IPv4
                    if (Addfam == ADDRESSFAM.IPv4)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }

                    //IPv6
                    else if (Addfam == ADDRESSFAM.IPv6)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
        }
        return output;
    }
}
public enum ADDRESSFAM
{
    IPv4, IPv6
}