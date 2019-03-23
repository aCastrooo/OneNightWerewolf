[System.Serializable]
public class Net_CreateAcc : NetMsg
{
    public Net_CreateAcc()
    {
        OP = NetOP.createAccount;
    }

    public string Username { set; get; }
}
