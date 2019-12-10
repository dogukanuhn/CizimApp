using CizimApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimApp.Services
{
    public interface IUserService
    {

        bool Login(UserDTO user);
        void Logout(UserDTO user);



    }
}
