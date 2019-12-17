
using CizimAppEntity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimAppData.Repository
{
    public class ChatRepository : EfRepository<Chat>,IChatRepository
    {
        public ChatRepository(AppDbContext _context) : base(_context)
        {

        }
    }
}
