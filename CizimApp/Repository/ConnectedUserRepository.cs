using CizimApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimApp.Repository
{
    public class ConnectedUserRepository : EfRepository<ConnectedUser>,IConnectedUserRepository
    {
        public ConnectedUserRepository(AppDbContext _context) : base(_context)
        {

        }
    }
}
