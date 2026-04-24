namespace Vortex.Protocol.Models;

public class PlayerData
{
    public bool OnlineStatus { get; set; }
    public string Username { get; set; } = string.Empty;
    public int StatLifeMax { get; set; }
    public int StatLife { get; set; }
    public int StatManaMax { get; set; }
    public int StatMana { get; set; }
    public int[] BuffType { get; set; } = Array.Empty<int>();
    public int[] BuffTime { get; set; } = Array.Empty<int>();
    public Item[] Inventory { get; set; } = Array.Empty<Item>();
    public Item[] MiscEquip { get; set; } = Array.Empty<Item>();
    public Item[] MiscDye { get; set; } = Array.Empty<Item>();
    public Suits[] Loadout { get; set; } = Array.Empty<Suits>();
    public Item[] TrashItem { get; set; } = Array.Empty<Item>();
    public Item[] Piggy { get; set; } = Array.Empty<Item>();
    public Item[] Safe { get; set; } = Array.Empty<Item>();
    public Item[] Forge { get; set; } = Array.Empty<Item>();
    public Item[] VoidVault { get; set; } = Array.Empty<Item>();
}
