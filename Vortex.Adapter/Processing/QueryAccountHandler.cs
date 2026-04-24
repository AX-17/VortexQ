using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Models;
using Vortex.Protocol.Packets;
using TShockAPI;

namespace Vortex.Adapter.Processing;

public class QueryAccountHandler(Net.VortexClient client) : RequestHandlerBase<AccountQueryPacket, AccountQueryPacketResponse>(client)
{
    public override AccountQueryPacketResponse Handle(AccountQueryPacket request)
    {
        var response = CreateSuccessResponse(request, "查询成功");

        if (string.IsNullOrEmpty(request.Target))
        {
            TShock.UserAccounts.GetUserAccounts().ForEach(account =>
            {
                response.Accounts.Add(new Account
                {
                    Group = account.Group,
                    Name = account.Name,
                    IP = account.KnownIps,
                    UUID = account.UUID,
                    ID = account.ID,
                    Password = account.Password,
                    RegisterTime = account.Registered?.ToString() ?? "",
                    LastLoginTime = account.LastAccessed?.ToString() ?? ""
                });
            });
        }
        else
        {
            var target = TShock.UserAccounts.GetUserAccountByName(request.Target);
            if (target != null)
            {
                response.Accounts.Add(new Account
                {
                    Group = target.Group,
                    Name = target.Name,
                    IP = target.KnownIps,
                    UUID = target.UUID,
                    ID = target.ID,
                    Password = target.Password,
                    RegisterTime = target.Registered?.ToString() ?? "",
                    LastLoginTime = target.LastAccessed?.ToString() ?? ""
                });
            }
            else
            {
                return CreateFailureResponse(request, "目标用户不存在");
            }
        }

        return response;
    }
}
