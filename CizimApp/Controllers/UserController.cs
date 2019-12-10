using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CizimApp.Models;
using CizimApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CizimApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        //private readonly IUserService _userService;
        //private readonly IConnectedUserService _connectedUserService;
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
            //_userService = userService;
            //_connectedUserService = connectedUserService;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserDTO user)
        {
            //var data = _userService.Login(user);
            var data = await _context.Users.FirstOrDefaultAsync(x => x.Username == user.Username && x.Password == user.Password);
  
            if(data != null)
            {
                return await Task.FromResult(Ok(new { 
                  Username= data.Username
                }));
            }

            return await Task.FromResult(NotFound());

        }

        [HttpPost("connect")]
        public async Task<IActionResult> SaveToConnectedUser([FromBody] UserDTO user)
        {
            var result = _context.ConnectedUsers.Add(new ConnectedUser
            {
                Username = user.Username,
                ConnectionId = user.ConnectionId

            });
            await _context.SaveChangesAsync();





            return await Task.FromResult(Ok());

        }
    }
}