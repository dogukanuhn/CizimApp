using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CizimApp.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Data.SqlClient;
using CizimApp.Models;
using Microsoft.EntityFrameworkCore;
using CizimApp.Repository;
using CizimApp.Helpers;
using Newtonsoft.Json;
using System.Buffers.Text;

namespace CizimApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IHubContext<Chathub> _hubContext;

        private readonly IRoomRepository _roomRepository;
        private readonly IConnectedUserRepository _connectedUserRepository;
        private readonly IRedisHandler _redisHandler;
        public RoomController(IRedisHandler redisHandler, IConnectedUserRepository connectedUserRepository, IHubContext<Chathub> hubContext, IRoomRepository roomRepository, AppDbContext context)
        {
            _redisHandler = redisHandler;
            _roomRepository = roomRepository;
            _hubContext = hubContext;
            _connectedUserRepository = connectedUserRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Insert([FromBody] Room room)
        {
            room.Id = Guid.NewGuid();
            await _roomRepository.Add(room);
            var data = await _roomRepository.GetAll();
            await _hubContext.Clients.All.SendAsync("Notify", data);

            return await Task.FromResult(Ok(room));
        }

        [HttpPost("remove")]
        public async Task<IActionResult> Remove([FromBody] Room room)
        {
            await _roomRepository.Remove(room);

            var data = await _roomRepository.GetAll();
            await _hubContext.Clients.All.SendAsync("Notify", data);
            await _hubContext.Clients.Group(room.roomName).SendAsync("IsClosed", true);
            return await Task.FromResult(Ok(true));

        }
        [HttpGet("{groupName}")]
        public async Task<IActionResult> Get(string groupName)
        {
            var data = await _roomRepository.FirstOrDefault(x => x.roomName == groupName);
            if (data != null)
            {
                return await Task.FromResult(Ok(true));
            }
            return await Task.FromResult(Ok(false));
        }

        [HttpGet("roomuser/{groupName}")]
        public async Task<IActionResult> RoomPlayer(string groupName)
        {
            var data = await _connectedUserRepository.GetWhere(x => x.ConnectedRoomName == groupName);
            if (data != null)
            {
                return await Task.FromResult(Ok(data));
            }
            return await Task.FromResult(Ok(false));
        }

        [HttpPost("joinroom")]
        public async Task<IActionResult> JoinRoom(ConnectedUser user)
        {
            var room = await _roomRepository.FirstOrDefault(x => x.roomName == user.ConnectedRoomName);
            if (room != null)
            {
                room.roomUserCount += 1;
                await _roomRepository.Update(room);


                var data = await _roomRepository.GetAll();
                await _hubContext.Clients.All.SendAsync("Notify", data);


                var lab = await _connectedUserRepository.FirstOrDefault(x => x.ConnectionId == user.ConnectionId);

                if (lab != null)
                {
                    lab.ConnectedRoomName = user.ConnectedRoomName;
                    await _connectedUserRepository.Update(lab);
                    //DATA NULL GELİYOR VE PATLIYOR
                    await _hubContext.Groups.AddToGroupAsync(lab.ConnectionId, lab.ConnectedRoomName);

                    await _hubContext.Clients.Group(user.ConnectedRoomName).SendAsync("GroupJoined", $"{lab.Username} has joined the group.");
                    return await Task.FromResult(Ok(true));
                }
                
            }

            return await Task.FromResult(Ok(false));

        }

        [HttpPost("quitroom")]
        public async Task<IActionResult> QuitRoom(ConnectedUser user)
        {
            var room = await _roomRepository.FirstOrDefault(x => x.roomName == user.ConnectedRoomName);
            room.roomUserCount -= 1;

            var lab = await _connectedUserRepository.FirstOrDefault(x => x.ConnectionId == user.ConnectionId);
            lab.ConnectedRoomName = null;
            await _connectedUserRepository.Update(lab);
            if (room.roomAdmin == lab.Username)
            {
                var admin = await _connectedUserRepository.FirstOrDefault(x => x.ConnectedRoomName == room.roomName);
                if (admin == null)
                {
                    await _hubContext.Clients.Group(lab.ConnectedRoomName).SendAsync("IsClosed", true);

                }
                else
                {
                    //test
                    room.roomAdmin = admin.Username;
                    await _hubContext.Clients.Client(admin.ConnectionId).SendAsync("AdminCall", true);
                    await _roomRepository.Update(room);
                }
            }


            await _hubContext.Groups.RemoveFromGroupAsync(lab.ConnectionId, lab.ConnectedRoomName);
            await _hubContext.Clients.Group(lab.ConnectedRoomName).SendAsync("GroupLeaved", $"{lab.Username} has left the group.");
            var data = await _roomRepository.GetAll();
            await _hubContext.Clients.All.SendAsync("Notify", data);
            return await Task.FromResult(Ok());

        }

        [HttpPost("kickstatus")]
        public async Task<IActionResult> KickStatus(ConnectedUser user)
        {
            var kick = JsonConvert.DeserializeObject<RoomVote>(await _redisHandler.GetFromCache($"kicklist:{user.Username}"));
            if (kick != null)
            {   
                if (Math.Abs(kick.Yes - kick.No) == 1)
                {
                    await _redisHandler.RemoveFromCache($"kicklist:{user.Username}");
                    return await Task.FromResult(Ok("Atılma işlemi olmadı"));
                }
                else
                {
                    if (kick.Yes > kick.No)
                    {
                        await _redisHandler.RemoveFromCache($"kicklist:{user.Username}");
                        await _hubContext.Clients.Client(user.ConnectionId).SendAsync("KickedFromRoom", true);
                        return await Task.FromResult(Ok("Atılma işlemi gerçekleşti"));
                    }
                }
            }
            await _redisHandler.RemoveFromCache($"kicklist:{user.Username}");
            return await Task.FromResult(Ok("Atılma işlemi olmadı"));
        }

        [HttpPost("kickstart")]
        public async Task<IActionResult> KickStart(ConnectedUser user)
        {
            var data = await _connectedUserRepository.FirstOrDefault(x => x.ConnectionId == user.ConnectionId);
            if (data != null)
            {
                var req = new
                {
                    Username = data.Username,
                    ConnectionId = data.ConnectionId,
                };
                await _hubContext.Clients.Group(data.ConnectedRoomName).SendAsync("KickStart", req);
                return await Task.FromResult(Ok(req));
            }
            return await Task.FromResult(Ok(false));


        }

        [HttpPost("kickvoteyes")]
        public async Task<IActionResult> KickVoteYes(ConnectedUser user)
        {

            if (!(await _redisHandler.IsCached($"kicklist:{user.Username}")))
            {
                var data = new RoomVote
                {
                    Yes = 1,
                    Username = user.Username,
                    No = 0,
                    Id=user.Id
                };
                await _redisHandler.AddToCache($"kicklist:{user.Username}", TimeSpan.FromMinutes(5), JsonConvert.SerializeObject(data));
                return await Task.FromResult(Ok(true));

            }
            else
            {
                
                var kick = JsonConvert.DeserializeObject<RoomVote>(await _redisHandler.GetFromCache($"kicklist:{user.Username}"));
                kick.Yes += 1;
                await _redisHandler.RemoveFromCache($"kicklist:{user.Username}");
                await _redisHandler.AddToCache($"kicklist:{user.Username}", TimeSpan.FromMinutes(5), JsonConvert.SerializeObject(kick));

                return await Task.FromResult(Ok(true));

            }

            
        }

        [HttpPost("kickvoteno")]
        public async Task<IActionResult> KickVoteNo(ConnectedUser user)
        {

            if (!(await _redisHandler.IsCached($"kicklist:{user.Username}")))
            {
                var data = new RoomVote
                {
                    Yes = 0,
                    Username = user.Username,
                    No = 1,
                    Id = user.Id
                };
                await _redisHandler.AddToCache($"kicklist:{user.Username}", TimeSpan.FromMinutes(5), JsonConvert.SerializeObject(data));
            }
            else
            {
                var kick = JsonConvert.DeserializeObject<RoomVote>(await _redisHandler.GetFromCache($"kicklist:{user.Username}"));
                kick.No += 1;
                //CACHE GÜNCELLEMİYOR
                await _redisHandler.RemoveFromCache($"kicklist:{user.Username}");
                await _redisHandler.AddToCache($"kicklist:{user.Username}", TimeSpan.FromMinutes(5), JsonConvert.SerializeObject(kick));

                return await Task.FromResult(Ok(true));

            }

            return await Task.FromResult(Ok(false));
        }


    }
}