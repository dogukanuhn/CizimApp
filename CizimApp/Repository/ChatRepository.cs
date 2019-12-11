using CizimApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimApp.Repository
{
    public class ChatRepository : EfRepository<Chat>,IChatRepository
    {
        public ChatRepository(AppDbContext _context) : base(_context)
        {

        }
    }
}
