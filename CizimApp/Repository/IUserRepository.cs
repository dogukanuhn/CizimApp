using CizimApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimApp.Repository
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> Login(UserDTO user);

    }
}
