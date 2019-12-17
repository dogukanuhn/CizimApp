
using CizimAppEntity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimAppData.Repository
{
    public class UserRepository : EfRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {

        }
        public Task<User> Login(UserDTO user)
        {
            var data =  FirstOrDefault(x => x.Username == user.Username && x.Password == user.Password);
            if (data != null)
            {
                return  data;
            }
            return null;
        }
    }
}
