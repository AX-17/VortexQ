using Lagrange.Core.Message;

namespace Vortex.Bot.Extension;

public static class MessageBuilderExtension
{
    extension(MessageBuilder builder)
    {
        public static MessageBuilder Create()
        {
            return new MessageBuilder();
        }
    }
}
