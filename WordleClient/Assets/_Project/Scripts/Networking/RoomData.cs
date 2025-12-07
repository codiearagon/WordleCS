using System.Collections.Generic;

public class RoomData
{
    public string roomName;
    public int playerCount;
    public List<Player> players  = new List<Player>();
    public bool inGame;
    public int hostId;
}
