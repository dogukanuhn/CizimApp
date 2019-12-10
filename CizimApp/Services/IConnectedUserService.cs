using CizimApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimApp.Services
{
    public interface IConnectedUserService
    {
        ConnectedUser GetNickByConnectionID(UserDTO user);
        bool SaveUserToConnectedList(UserDTO user);
        ConnectedUser GetConnectionIdByNick(UserDTO user);




    }
}
