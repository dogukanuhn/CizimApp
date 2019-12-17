
using CizimAppEntity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimAppData.Repository
{
    public class ConnectedUserRepository : EfRepository<ConnectedUser>,IConnectedUserRepository
    {
        public ConnectedUserRepository(AppDbContext _context) : base(_context)
        {

        }
    }
}
