using Vortex.Protocol.Interfaces;
using Vortex.Protocol.Packets;
using TShockAPI.DB;
using TShockAPI;

namespace Vortex.Adapter.Processing;

public class ResetPasswordHandler(Net.VortexClient client) : RequestHandlerBase<PasswordResetPacket, PasswordResetPacketResponse>(client)
{
    public override PasswordResetPacketResponse Handle(PasswordResetPacket request)
    {
        var account = new UserAccount { Name = request.Name };
        try
        {
            TShock.UserAccounts.SetUserAccountPassword(account, request.Password);
            return CreateSuccessResponse(request, "密码重置成功");
        }
        catch (UserAccountNotExistException)
        {
            return CreateFailureResponse(request, "玩家账户不存在");
        }
        catch (ArgumentOutOfRangeException)
        {
            return CreateFailureResponse(request, $"密码必须不少于{TShock.Config.Settings.MinimumPasswordLength}个字符");
        }
        catch (Exception ex)
        {
            return CreateFailureResponse(request, ex.Message);
        }
    }
}
