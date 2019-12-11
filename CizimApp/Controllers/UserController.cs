using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CizimApp.Models;
using CizimApp.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CizimApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUserRepository _userRepository;
        private readonly IConnectedUserRepository _connectedUserRepository;

        private readonly AppDbContext _context;

        public UserController(AppDbContext context, IConnectedUserRepository connectedUserRepository, IUserRepository userRepository)
        {
            _connectedUserRepository = connectedUserRepository;
            _userRepository = userRepository;
            _context = context;

        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserDTO user)
        {

            var data = await _userRepository.Login(user);
            if (data != null)
            {
                return await Task.FromResult(Ok(new
                {
                    Username = data.Username
                }));
            }

            return await Task.FromResult(NotFound());

        }

        [HttpPost("connect")]
        public async Task<IActionResult> SaveToConnectedUser([FromBody] UserDTO user)
        {
            await _connectedUserRepository.Add(new ConnectedUser
            {
                Username = user.Username,
                ConnectionId = user.ConnectionId

            });

            return await Task.FromResult(Ok());

        }
    }
}