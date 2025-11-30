public class Player
{
    public string username { get; private set; }
    public int userId { get; private set; }
    public bool isReady;

    public void SetUsername(string username)
    {
        this.username = username;
        NetworkManager.Instance.SetUsername(username);
    }

    public void SetUserId(int userId)
    {
        this.userId = userId;
    }
}
