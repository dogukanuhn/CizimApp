
using CizimAppEntity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimAppData.Repository
{
    public class RoomRepository : EfRepository<Room>,IRoomRepository
    {
        public RoomRepository(AppDbContext _context): base(_context)
        {

        }
    }
}
