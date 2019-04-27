public static class NetOP
{
    public const int None = 0;
    public const int createAccount = 1;
    public const int LoginRequest = 2;
    public const int PlayerList = 3;
    public const int StartGame = 4;
    public const int SendCard = 5;
    public const int RemovePlayer = 6;
}

[System.Serializable]
public abstract class NetMsg
{ 
    public byte OP { set; get; }

    public NetMsg()
    {
        OP = NetOP.None;
    }
}
