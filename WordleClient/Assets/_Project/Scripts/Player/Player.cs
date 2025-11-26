public class Player
{
    public string username { get; private set; }
    public int userId;
    public bool isReady;

    public void SetUsername(string username)
    {
        this.username = username;
        NetworkManager.network.SetUsername(username);
    }
}
