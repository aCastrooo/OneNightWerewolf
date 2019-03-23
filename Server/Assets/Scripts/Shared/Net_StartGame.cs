[System.Serializable]
public class Net_StartGame : NetMsg
{
    public Net_StartGame()
    {
        OP = NetOP.StartGame;
    }

    public bool gameStart { set; get; }
}
