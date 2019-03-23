[System.Serializable]
public class Net_PlayerList : NetMsg
{
    public Net_PlayerList()
    {
        OP = NetOP.PlayerList;
    }

    public string[] playerList { set; get; }
}
