[System.Serializable]
public class Net_ReadyUp : NetMsg
{
    public Net_ReadyUp()
    {
        OP = NetOP.Ready;
    }

    public bool ready { set; get; }
}