using CizimApp.Data;
using CizimApp.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimApp.Services
{
    public class UserService : IUserService
    {

        private readonly IUserRepository _userRepository;
        //private readonly IConnectedUserRepository _connectedUserRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public bool Login(UserDTO user)
        {
            var userData = _userRepository.Find(x => x.Username == user.Username && x.Password == user.Password);
            if (userData != null )
            {
                
                return true;
            }
            return false;

        }

        public void Logout(UserDTO user)
        {

        }
    }
}
