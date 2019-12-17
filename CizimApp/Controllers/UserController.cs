using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CizimApp.Helpers;
using CizimAppData.Repository;
using CizimAppEntity.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CizimApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUserRepository _userRepository;
        private readonly IConnectedUserRepository _connectedUserRepository;
        private readonly IRedisHandler _redisHandler;


        public UserController(IRedisHandler redisHandler, IConnectedUserRepository connectedUserRepository, IUserRepository userRepository)
        {
            _connectedUserRepository = connectedUserRepository;
            _userRepository = userRepository;
            _redisHandler = redisHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserDTO user)
        {

            if (await _redisHandler.IsCached("Userlist:Users"))
            {
                var data = JsonConvert.DeserializeObject<List<User>>(await _redisHandler.GetFromCache("Userlist:Users"));
                var login = data.FirstOrDefault(x => x.Username == user.Username && x.Password == user.Password);
                if (login != null)
                {
                    return await Task.FromResult(Ok(new
                    {
                        Username = login.Username
                    }));
                }
            }
            else
            {
                var users = await _userRepository.GetAll();
                await _redisHandler.AddToCache("Userlist:Users", TimeSpan.FromMinutes(2), JsonConvert.SerializeObject(users));
                var data = users.FirstOrDefault(x => x.Username == user.Username && x.Password == user.Password);

                if (data != null)
                {
                    return await Task.FromResult(Ok(new
                    {
                        Username = data.Username
                    }));
                }
            }

            return await Task.FromResult(NotFound());

        }

        [HttpPost("connect")]
        public async Task<IActionResult> SaveToConnectedUser([FromBody] UserDTO user)
        {
            if (await _redisHandler.IsCached("Userlist:ConnectedUser"))
            {
                var cached = JsonConvert.DeserializeObject<List<ConnectedUser>>(await _redisHandler.GetFromCache("Userlist:ConnectedUser"));
                var connectedUser = cached.FirstOrDefault(x => x.Username == user.Username);
                if (connectedUser != null)
                {
                    connectedUser.ConnectionId = user.ConnectionId;

                }
                else
                {
                    cached.Add(new ConnectedUser
                    {
                        Id = Guid.NewGuid(),
                        Username = user.Username,
                        ConnectionId = user.ConnectionId

                    });
                }

                await _redisHandler.RemoveFromCache("Userlist:ConnectedUser");
                await _redisHandler.AddToCache("Userlist:ConnectedUser", TimeSpan.FromMinutes(2), JsonConvert.SerializeObject(cached));

                return await Task.FromResult(Ok());


            }

            
            var data = await _connectedUserRepository.FirstOrDefault(x => x.Username == user.Username);

            if (data != null)
            {
                data.ConnectionId = user.ConnectionId;
                await _connectedUserRepository.Update(data);
            }
            else
            {
                await _connectedUserRepository.Add(new ConnectedUser
                {
                    Id = Guid.NewGuid(),
                    Username = user.Username,
                    ConnectionId = user.ConnectionId

                }); ;
            }
            await _redisHandler.AddToCache("Userlist:ConnectedUser", TimeSpan.FromMinutes(5), JsonConvert.SerializeObject(await _connectedUserRepository.GetAll()));
            return await Task.FromResult(Ok());

        }
    }
}