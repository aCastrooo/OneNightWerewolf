[System.Serializable]
public class Net_Card : NetMsg
{
    public Net_Card()
    {
        OP = NetOP.SendCard;
    }

    public string card { set; get; }
}
