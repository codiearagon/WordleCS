public class Player
{
    public string username { get; private set; }
    public int userId { get; private set; }
    public bool isReady;
    public bool finished = false;

    public int guessCount { get; private set; } = 0;

    public void SetUsername(string username)
    {
        this.username = username;
    }

    public void SetUserId(int userId)
    {
        this.userId = userId;
    }

    public void SetGuessCount(int guessCount)
    {
        this.guessCount = guessCount;
    }
}
