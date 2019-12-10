using CizimApp.Data;
using CizimApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimApp.Services
{
    public class ConnectedUserService : IConnectedUserService
    {
        public readonly IConnectedUserRepository _connectedUserRepository;
        public ConnectedUserService(IConnectedUserRepository connectedUserRepository)
        {
            _connectedUserRepository = connectedUserRepository;
        }
        public ConnectedUser GetConnectionIdByNick(UserDTO user)
        {
            throw new NotImplementedException();
        }

        public ConnectedUser GetNickByConnectionID(UserDTO user)
        {
            throw new NotImplementedException();
        }

        public bool SaveUserToConnectedList(UserDTO user)
        {
            var data = _connectedUserRepository.Save(new ConnectedUser
            {
                Username = user.Username,
                ConnectionId = user.ConnectionId
            });
            if (data != null)
            {
                return true;
            }
            return false;

        }
    }
}
