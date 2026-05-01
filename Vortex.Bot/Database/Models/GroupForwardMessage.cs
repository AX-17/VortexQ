using LinqToDB.Mapping;

namespace Vortex.Bot.Database.Models;

[Table("GroupForwardMessage")]
public class GroupForwardMessage
{
    [PrimaryKey]
    public long GroupUin { get; set; }

    [Column]
    public string ServerName { get; set; } = string.Empty;


    public static IDataContext<GroupForwardMessage> DataContext => RecordBase.GetContext<GroupForwardMessage>("GroupForwardMessage");

    public static void Set(long groupUin, string serverName)
    {
        var existing = DataContext.Records.FirstOrDefault(x => x.GroupUin == groupUin);
        if (existing != null)
        {
            existing.ServerName = serverName;
            DataContext.Update(existing);
        }
        else
        {
            DataContext.Insert(new GroupForwardMessage
            {
                GroupUin = groupUin,
                ServerName = serverName
            });
        }
    }

    public static void SetServerGroups(string serverName, IEnumerable<long> groupUins)
    {
        var existing = DataContext.Records.Where(x => x.ServerName == serverName).ToList();
        foreach (var item in existing)
        {
            DataContext.Delete(x => x.GroupUin == item.GroupUin);
        }

        foreach (var groupUin in groupUins.Distinct())
        {
            DataContext.Insert(new GroupForwardMessage
            {
                GroupUin = groupUin,
                ServerName = serverName
            });
        }
    }

    public static void AddGroupToServer(string serverName, long groupUin)
    {
        var existing = DataContext.Records.FirstOrDefault(x => x.GroupUin == groupUin);
        if (existing != null)
        {
            existing.ServerName = serverName;
            DataContext.Update(existing);
        }
        else
        {
            DataContext.Insert(new GroupForwardMessage
            {
                GroupUin = groupUin,
                ServerName = serverName
            });
        }
    }

    public static void AddGroupsToServer(string serverName, IEnumerable<long> groupUins)
    {
        foreach (var groupUin in groupUins.Distinct())
        {
            AddGroupToServer(serverName, groupUin);
        }
    }

    public static GroupForwardMessage? Get(long groupUin)
    {
        return DataContext.Records.FirstOrDefault(x => x.GroupUin == groupUin);
    }

    public static string? GetServerName(long groupUin)
    {
        return Get(groupUin)?.ServerName;
    }

    public static bool Exists(long groupUin)
    {
        return DataContext.Records.Any(x => x.GroupUin == groupUin);
    }

    public static List<GroupForwardMessage> GetAll()
    {
        return DataContext.Records.ToList();
    }

    public static List<GroupForwardMessage> GetByServerName(string serverName)
    {
        return DataContext.Records.Where(x => x.ServerName == serverName).ToList();
    }

    public static List<long> GetGroupUinsByServer(string serverName)
    {
        return DataContext.Records
            .Where(x => x.ServerName == serverName)
            .Select(x => x.GroupUin)
            .ToList();
    }

    public static List<string> GetAllServerNames()
    {
        return DataContext.Records
            .Select(x => x.ServerName)
            .Distinct()
            .ToList();
    }

    public static void Delete(long groupUin)
    {
        DataContext.Delete(x => x.GroupUin == groupUin);
    }

    public static void DeleteByServerName(string serverName)
    {
        DataContext.Delete(x => x.ServerName == serverName);
    }

    public static void RemoveGroupFromServer(string serverName, long groupUin)
    {
        DataContext.Delete(x => x.ServerName == serverName && x.GroupUin == groupUin);
    }
}
