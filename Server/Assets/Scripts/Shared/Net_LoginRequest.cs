[System.Serializable]
public class Net_LoginRequest : NetMsg
{
    public Net_LoginRequest()
    {
        OP = NetOP.LoginRequest;
    }

    public string Username { set; get; }
}
