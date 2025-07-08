using Stockat.Core.Entities;
using Stockat.Core.IRepositories;

namespace Stockat.EF.Repositories
{
    public class ChatBotMessageRepository : BaseRepository<ChatBotMessage>, IChatBotMessageRepository
    {
        public ChatBotMessageRepository(StockatDBContext context) : base(context)
        {
        }
    }
} 