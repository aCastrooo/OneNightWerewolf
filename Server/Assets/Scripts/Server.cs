using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public class Server : MonoBehaviour
{
    private byte reliableChannel;
    private byte error;

    private string[] playerList;
    private string[] middleCards;

    private int hostId;
    private int webHostId;
    private int numCurrPlayers;

    //Server started
    private bool isStarted;
    //Game started
    private bool gameStarted;

    private const int MAX_USER = 10;
    private const int PORT = 26000;
    private const int WEB_PORT = 26001;
    private const int BYTE_SIZE = 1024;

    #region Monobehaviour
    private void Start()
    {
        gameStarted = false;
        playerList = new string[MAX_USER];

        for (int i=0; i < MAX_USER; i++)
        {
            playerList[i] = "";
        }

        DontDestroyOnLoad(gameObject);
        Init();

        numCurrPlayers = 0;
    }

    private void Update()
    {
        UpdateMessagePump();
    }
    #endregion

    public void Init()
    {
        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();
        reliableChannel = cc.AddChannel(QosType.Reliable);

        HostTopology topo = new HostTopology(cc, MAX_USER);

        //Server
        hostId = NetworkTransport.AddHost(topo, PORT, null);
        webHostId = NetworkTransport.AddWebsocketHost(topo, WEB_PORT, null);

        Debug.Log(string.Format("Opening connection on port {0} and webport {1}", PORT, WEB_PORT));

        Debug.Log(string.Format("My current IP address is: {0}", IPManager.GetIP(ADDRESSFAM.IPv6)));
        isStarted = true;
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
                Debug.Log(string.Format("User {0} has connected has connected through host {1}", connectionId, recHostId));
                numCurrPlayers += 1;
                Debug.Log(string.Format("Num players: {0}", numCurrPlayers));
                SendUpdatedPlayerList(connectionId, recHostId);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log(string.Format("User {0} has disconnected.", connectionId));
                break;
            case NetworkEventType.DataEvent:
                NetMsg msg = recieveMsg(recBuffer);
                Debug.Log(msg);
                Debug.Log(string.Format("{0} {1} {2} {3}", connectionId, channelId, recHostId, msg.OP));
                OnData(connectionId, channelId, recHostId, msg);
                break;
            default:
                Debug.Log("???");
                break;
        }

    }

    #region OnData
    private NetMsg recieveMsg(byte[] recBuffer)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(recBuffer);
        return (NetMsg)formatter.Deserialize(ms);
    }

    private void OnData(int conId, int chanId, int recHostId, NetMsg msg)
    {
        switch (msg.OP)
        {
            case NetOP.createAccount:
                createAccount(conId,  recHostId, (Net_CreateAcc)msg);
                break;
            case NetOP.StartGame:
                StartGameInstance();
                break;
            default:
                Debug.Log("Nothing to see");
                break;
            
        }
    }

    private void createAccount(int conId, int recHostId, Net_CreateAcc msg)
    {
        Debug.Log(string.Format("{0}", msg.Username));
        string username = msg.Username;
        for (int i=0; i<MAX_USER; i++)
        {
            if (playerList[i].Equals(""))
            {
                playerList[i] = msg.Username;
                break;
            }
        }
        SendUpdatedPlayerList(conId, recHostId);

    }
    #endregion


    #region SendRequests
    public void SendClient(int recHost, int connectionId, NetMsg msg)
    {
        byte[] buffer = new byte[BYTE_SIZE];

        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, msg);

        if (recHost == 0)
            NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, BYTE_SIZE, out error);
        else
            NetworkTransport.Send(webHostId, connectionId, reliableChannel, buffer, BYTE_SIZE, out error);
    }

    private void SendUpdatedPlayerList(int connectionId, int recHostId)
    {
        for (int i = 1; i <= numCurrPlayers; i++) {
            Net_PlayerList newMsg = new Net_PlayerList();
            newMsg.playerList = playerList;
            SendClient(recHostId, i, (Net_PlayerList)newMsg);
        }
    }
    #endregion




    #region GamePlay
    private void StartGameInstance()
    {
        isStarted = true;
        gameBoard Game = new gameBoard(playerList);
        Game.shuffle();
        string[] nothing = Game.giveOutCards();
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