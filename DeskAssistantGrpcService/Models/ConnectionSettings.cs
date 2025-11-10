using Telegram.Bot.Types;

namespace DeskAssistantGrpcService.Models
{
    public class ConnectionSettings
    {
        public string DefaultConnection { get; set; } = string.Empty;

        public string BotClientToken { get; set; } = string.Empty;

        public string BotChatId { get; set; } = string.Empty;
    }
}
