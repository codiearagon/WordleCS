using System.Collections.Generic;

public class RoomData
{
    public string roomName { get; private set; }
    public List<Player> players { get; private set; } = new List<Player>();
    public int hostId { get; private set; }
}
