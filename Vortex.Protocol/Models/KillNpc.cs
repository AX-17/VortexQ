namespace Vortex.Protocol.Models;

public class KillNpc
{
    public int Id { get; set; }
    public int MaxLife { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<PlayerStrike> Strikes { get; set; } = new();
    public DateTime KillTime { get; set; } = DateTime.Now;
    public DateTime SpawnTime { get; set; } = DateTime.Now;
    public bool IsAlive { get; set; } = true;
}
