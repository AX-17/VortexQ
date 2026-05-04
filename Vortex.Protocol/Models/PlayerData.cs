namespace Vortex.Protocol.Models;

public class PlayerData
{
    public bool OnlineStatus { get; set; }
    public string Username { get; set; } = string.Empty;
    public int StatLifeMax { get; set; }
    public int StatLife { get; set; }
    public int StatManaMax { get; set; }
    public int StatMana { get; set; }
    public int[] BuffType { get; set; } = [];
    public int[] BuffTime { get; set; } = [];
    public Item[] Inventory { get; set; } = [];
    public Item[] MiscEquip { get; set; } = [];
    public Item[] MiscDye { get; set; } = [];
    public Suits[] Loadout { get; set; } = [];
    public Item[] TrashItem { get; set; } = [];
    public Item[] Piggy { get; set; } = [];
    public Item[] Safe { get; set; } = [];
    public Item[] Forge { get; set; } = [];
    public Item[] VoidVault { get; set; } = [];
}
