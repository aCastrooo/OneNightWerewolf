[System.Serializable]
public class Net_RemovePlayer : NetMsg
{
    public Net_RemovePlayer()
    {
        OP = NetOP.RemovePlayer;
    }

    public string playerToRemove { set; get; }
}
