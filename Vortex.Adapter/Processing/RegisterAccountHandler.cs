using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Packets;
using TShockAPI.DB;
using TShockAPI;

namespace Vortex.Adapter.Processing;

public class RegisterAccountHandler(Net.VortexClient client) : RequestHandlerBase<AccountRegistrationPacket, AccountRegistrationPacketResponse>(client)
{
    public override AccountRegistrationPacketResponse Handle(AccountRegistrationPacket request)
    {
        try
        {
            var account = new UserAccount()
            {
                Name = request.Name,
                Group = request.Group
            };
            account.CreateBCryptHash(request.Password);
            TShock.UserAccounts.AddUserAccount(account);
            return CreateSuccessResponse(request, "注册成功");
        }
        catch (Exception ex)
        {
            return CreateFailureResponse(request, ex.Message);
        }
    }
}
