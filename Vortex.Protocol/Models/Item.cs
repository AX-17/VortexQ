namespace Vortex.Protocol.Models;

public class Item
{
    public int NetId { get; set; }
    public int Prefix { get; set; }
    public int Stack { get; set; }

    public Item() { }

    public Item(int netId, int prefix, int stack)
    {
        NetId = netId;
        Prefix = prefix;
        Stack = stack;
    }
}
