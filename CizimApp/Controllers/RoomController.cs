using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CizimApp.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using CizimAppEntity;
using CizimAppEntity.Models;
using CizimAppData.Repository;
using CizimAppData.Helpers;
using CizimAppData;

namespace CizimApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IHubContext<Chathub> _hubContext;
        private readonly IWordRepository _wordRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IConnectedUserRepository _connectedUserRepository;
        private readonly IRedisHandler _redisHandler;
        public RoomController(IWordRepository wordRepository, IRedisHandler redisHandler, IConnectedUserRepository connectedUserRepository, IHubContext<Chathub> hubContext, IRoomRepository roomRepository, AppDbContext context)
        {
            _wordRepository = wordRepository;
            _redisHandler = redisHandler;
            _roomRepository = roomRepository;
            _hubContext = hubContext;
            _connectedUserRepository = connectedUserRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Insert([FromBody] Room room)
        {
            room.Id = Guid.NewGuid();
            List<Room> rooms = new List<Room>();

            if (await _redisHandler.IsCached("Room:Rooms"))
            {
                rooms = JsonConvert.DeserializeObject<List<Room>>(await _redisHandler.GetFromCache("Room:Rooms"));

            }

            rooms.Add(room);
            await _redisHandler.RemoveFromCache("Room:Rooms");
            await _redisHandler.AddToCache("Room:Rooms", TimeSpan.FromMinutes(2), JsonConvert.SerializeObject(rooms));
            await _hubContext.Clients.All.SendAsync("Notify", rooms);
            return await Task.FromResult(Ok(room));
        }

        [HttpPost("remove")]
        public async Task<IActionResult> Remove([FromBody] Room room)
        {
            var data = JsonConvert.DeserializeObject<List<Room>>(await _redisHandler.GetFromCache("Room:Rooms"));
            data.Remove(room);
            await _redisHandler.RemoveFromCache("Room:Rooms");
            await _redisHandler.AddToCache("Room:Rooms", TimeSpan.FromMinutes(2), JsonConvert.SerializeObject(data));
            await _hubContext.Clients.All.SendAsync("Notify", data);

            await _hubContext.Clients.Group(room.roomName).SendAsync("IsClosed", true);
            return await Task.FromResult(Ok(true));

        }
        [HttpGet("{groupName}")]
        public async Task<IActionResult> Get(string groupName)
        {
            var data = JsonConvert.DeserializeObject<List<Room>>(await _redisHandler.GetFromCache("Room:Rooms"));
            data.FirstOrDefault(x => x.roomName == groupName);

            if (data != null)
            {
                return await Task.FromResult(Ok(true));
            }
            return await Task.FromResult(Ok(false));
        }

        [HttpGet("roomuser/{groupName}")]
        public async Task<IActionResult> RoomPlayer(string groupName)
        {
            if (await _redisHandler.IsCached("Userlist:ConnectedUser"))
            {
                var rooms = JsonConvert.DeserializeObject<List<ConnectedUser>>(await _redisHandler.GetFromCache("Userlist:ConnectedUser"));
                var data = rooms.Where(x => x.ConnectedRoomName == groupName).ToList();
                if (data != null)
                {
                    return await Task.FromResult(Ok(data));
                }
            }
            else
            {
                var data = await _connectedUserRepository.GetWhere(x => x.ConnectedRoomName == groupName);
                if (data != null)
                {
                    return await Task.FromResult(Ok(data));
                }
            }
            return await Task.FromResult(Ok(false));
        }

        [HttpPost("joinroom")]
        public async Task<IActionResult> JoinRoom(ConnectedUser user)
        {
            if (await _redisHandler.IsCached("Room:Rooms"))
            {
                var rooms = JsonConvert.DeserializeObject<List<Room>>(await _redisHandler.GetFromCache("Room:Rooms"));
                var room = rooms.FirstOrDefault(x => x.roomName == user.ConnectedRoomName);
                if (room != null)
                {
                    room.roomUserCount += 1;



                    ConnectedUser lab = new ConnectedUser();

                    if (await _redisHandler.IsCached("Userlist:ConnectedUser"))
                    {
                        var connectedUsers = JsonConvert.DeserializeObject<List<ConnectedUser>>(await _redisHandler.GetFromCache("Userlist:ConnectedUser"));
                        lab = connectedUsers.FirstOrDefault(x => x.ConnectionId == user.ConnectionId);
                        if (user.ConnectedRoomName != null)
                        {
                            lab.ConnectedRoomName = user.ConnectedRoomName;
                        }
                        await _redisHandler.RemoveFromCache("Userlist:ConnectedUser");
                        await _redisHandler.AddToCache("Userlist:ConnectedUser", TimeSpan.FromMinutes(2), JsonConvert.SerializeObject(connectedUsers));
                    }
                    else
                    {
                        var connectedUsers = await _connectedUserRepository.GetAll();
                        lab = connectedUsers.FirstOrDefault(x => x.ConnectionId == user.ConnectionId);

                        if (user.ConnectedRoomName != null)
                        {
                            lab.ConnectedRoomName = user.ConnectedRoomName;
                        }
                        await _connectedUserRepository.Update(lab);
                        await _redisHandler.AddToCache("Userlist:ConnectedUser", TimeSpan.FromMinutes(2), JsonConvert.SerializeObject(connectedUsers));

                    }

                    await _redisHandler.RemoveFromCache("Room:Rooms");
                    await _redisHandler.AddToCache("Room:Rooms", TimeSpan.FromMinutes(2), JsonConvert.SerializeObject(rooms));

                    await _hubContext.Groups.AddToGroupAsync(lab.ConnectionId, lab.ConnectedRoomName);
                    await _hubContext.Clients.Group(user.ConnectedRoomName).SendAsync("GroupJoined", $"{lab.Username} has joined the group.");
                    await _hubContext.Clients.All.SendAsync("Notify", rooms);


                    if (room.roomUserCount == 3)
                    {
                        await _hubContext.Clients.Group(room.roomName).SendAsync("StartGameTimer", true);
                    }


                    return await Task.FromResult(Ok(true));
                }
            }
            else
            {
                var room = await _roomRepository.FirstOrDefault(x => x.roomName == user.ConnectedRoomName);
                if (room != null)
                {
                    room.roomUserCount += 1;
                    await _roomRepository.Update(room);


                    var data = await _roomRepository.GetAll();


                    ConnectedUser lab = new ConnectedUser();

                    if (await _redisHandler.IsCached("Userlist:ConnectedUser"))
                    {
                        var connectedUsers = JsonConvert.DeserializeObject<List<ConnectedUser>>(await _redisHandler.GetFromCache("Userlist:ConnectedUser"));
                        lab = connectedUsers.FirstOrDefault(x => x.ConnectionId == user.ConnectionId);
                        lab.ConnectedRoomName = user.ConnectedRoomName;
                        await _redisHandler.RemoveFromCache("Userlist:ConnectedUser");
                        await _redisHandler.AddToCache("Userlist:ConnectedUser", TimeSpan.FromMinutes(2), JsonConvert.SerializeObject(connectedUsers));
                    }
                    else
                    {
                        var connectedUsers = await _connectedUserRepository.GetAll();
                        lab = connectedUsers.FirstOrDefault(x => x.ConnectionId == user.ConnectionId);
                        lab.ConnectedRoomName = user.ConnectedRoomName;
                        await _connectedUserRepository.Update(lab);
                        await _redisHandler.AddToCache("Userlist:ConnectedUser", TimeSpan.FromMinutes(2), JsonConvert.SerializeObject(connectedUsers));

                    }


                    await _redisHandler.RemoveFromCache("Room:Rooms");
                    await _redisHandler.AddToCache("Room:Rooms", TimeSpan.FromMinutes(2), JsonConvert.SerializeObject(data));
                    await _hubContext.Groups.AddToGroupAsync(lab.ConnectionId, lab.ConnectedRoomName);

                    await _hubContext.Clients.Group(user.ConnectedRoomName).SendAsync("GroupJoined", $"{lab.Username} has joined the group.");
                    await _hubContext.Clients.All.SendAsync("Notify", data);


                    if (room.roomUserCount == 3)
                    {
                        await _hubContext.Clients.Group(room.roomName).SendAsync("StartGameTimer", true);
                    }

                    return await Task.FromResult(Ok(true));

                }
            }

            return await Task.FromResult(Ok(false));

        }

        [HttpPost("quitroom")]
        public async Task<IActionResult> QuitRoom(ConnectedUser user)
        {
            if (await _redisHandler.IsCached("Room:Rooms"))
            {
                var rooms = JsonConvert.DeserializeObject<List<Room>>(await _redisHandler.GetFromCache("Room:Rooms"));
                var room = rooms.FirstOrDefault(x => x.roomName == user.ConnectedRoomName);

                List<ConnectedUser> connectedUserList = new List<ConnectedUser>();
                ConnectedUser lab = new ConnectedUser();
                if (await _redisHandler.IsCached("Userlist:ConnectedUser"))
                {
                    connectedUserList = JsonConvert.DeserializeObject<List<ConnectedUser>>(await _redisHandler.GetFromCache("Userlist:ConnectedUser"));
                    lab = connectedUserList.FirstOrDefault(x => x.ConnectionId == user.ConnectionId);
                    lab.ConnectedRoomName = null;
                    await _redisHandler.RemoveFromCache("Userlist:ConnectedUser");
                }
                else
                {
                    var connectedUsers = await _connectedUserRepository.GetAll();
                    connectedUserList = connectedUsers.ToList();
                    lab = connectedUserList.FirstOrDefault(x => x.ConnectionId == user.ConnectionId);
                    lab.ConnectedRoomName = null;
                    await _connectedUserRepository.Update(lab);


                }


                if (room != null)
                {
                    room.roomUserCount -= 1;
                    if (room.roomAdmin == lab.Username)
                    {
                        var admin = connectedUserList.FirstOrDefault(x => x.ConnectedRoomName == room.roomName);
                        if (admin == null)
                        {
                            await _hubContext.Clients.Group(user.ConnectedRoomName).SendAsync("IsClosed", true);
                            rooms.Remove(room);
                            if (await _roomRepository.CountWhere(x => x.roomName == room.roomName) > 0)
                            {
                                await _roomRepository.Remove(room);
                            }
                        }
                        else
                        {
                            //test
                            room.roomAdmin = admin.Username;
                            await _hubContext.Clients.Client(admin.ConnectionId).SendAsync("AdminCall", true);

                        }
                    }
                }
                await _redisHandler.RemoveFromCache("Userlist:ConnectedUser");
                await _redisHandler.AddToCache("Userlist:ConnectedUser", TimeSpan.FromMinutes(2), JsonConvert.SerializeObject(connectedUserList));

                await _redisHandler.RemoveFromCache("Room:Rooms");
                await _redisHandler.AddToCache("Room:Rooms", TimeSpan.FromMinutes(2), JsonConvert.SerializeObject(rooms));

                await _hubContext.Groups.RemoveFromGroupAsync(user.ConnectionId, user.ConnectedRoomName);
                await _hubContext.Clients.Group(user.ConnectedRoomName).SendAsync("GroupLeaved", $"{lab.Username} has left the group.");


                if (room.roomUserCount < 3)
                {
                    await _hubContext.Clients.Group(room.roomName).SendAsync("StartGameTimer", false);
                }

                await _hubContext.Clients.All.SendAsync("Notify", rooms);
                return await Task.FromResult(Ok());
            }
            else
            {
                var room = await _roomRepository.FirstOrDefault(x => x.roomName == user.ConnectedRoomName);

                List<ConnectedUser> connectedUserList = new List<ConnectedUser>();
                ConnectedUser lab = new ConnectedUser();
                if (await _redisHandler.IsCached("Userlist:ConnectedUser"))
                {
                    connectedUserList = JsonConvert.DeserializeObject<List<ConnectedUser>>(await _redisHandler.GetFromCache("Userlist:ConnectedUser"));
                    lab = connectedUserList.FirstOrDefault(x => x.ConnectionId == user.ConnectionId);
                    lab.ConnectedRoomName = null;
                    await _redisHandler.RemoveFromCache("Userlist:ConnectedUser");
                }
                else
                {
                    var connectedUsers = await _connectedUserRepository.GetAll();
                    connectedUserList = connectedUsers.ToList();
                    lab = connectedUserList.FirstOrDefault(x => x.ConnectionId == user.ConnectionId);
                    lab.ConnectedRoomName = null;
                    await _connectedUserRepository.Update(lab);


                }

                if (room != null)
                {
                    room.roomUserCount -= 1;
                    if (room.roomAdmin == lab.Username)
                    {
                        var admin = connectedUserList.FirstOrDefault(x => x.ConnectedRoomName == room.roomName);
                        if (admin == null)
                        {
                            await _hubContext.Clients.Group(user.ConnectedRoomName).SendAsync("IsClosed", true);
                            await _roomRepository.Remove(room);
                        }
                        else
                        {
                            //test
                            room.roomAdmin = admin.Username;
                            await _hubContext.Clients.Client(admin.ConnectionId).SendAsync("AdminCall", true);
                            await _roomRepository.Update(room);
                        }
                    }
                }

                await _redisHandler.RemoveFromCache("Userlist:ConnectedUser");
                await _redisHandler.AddToCache("Userlist:ConnectedUser", TimeSpan.FromMinutes(2), JsonConvert.SerializeObject(connectedUserList));

                await _redisHandler.RemoveFromCache("Room:Rooms");
                await _redisHandler.AddToCache("Room:Rooms", TimeSpan.FromMinutes(2), JsonConvert.SerializeObject(await _roomRepository.GetAll()));



                await _hubContext.Groups.RemoveFromGroupAsync(user.ConnectionId, user.ConnectedRoomName);
                await _hubContext.Clients.Group(user.ConnectedRoomName).SendAsync("GroupLeaved", $"{lab.Username} has left the group.");
                var data = await _roomRepository.GetAll();
                await _hubContext.Clients.All.SendAsync("Notify", data);
                return await Task.FromResult(Ok());
            }

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
            if (await _redisHandler.IsCached("Userlist:ConnectedUser"))
            {
                var users = JsonConvert.DeserializeObject<List<ConnectedUser>>(await _redisHandler.GetFromCache("Userlist:ConnectedUser"));
                var cuser = users.FirstOrDefault(x => x.ConnectionId == user.ConnectionId);
                var req = new
                {
                    Username = cuser.Username,
                    ConnectionId = cuser.ConnectionId,
                };
                await _hubContext.Clients.Group(cuser.ConnectedRoomName).SendAsync("KickStart", req);
                return await Task.FromResult(Ok(req));
            }
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
                    Id = user.Id
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



        [HttpPost("startgame")]
        public async Task<IActionResult> StartGame(Room room)
        {
            var wordCount = await _wordRepository.CountAll();
            Random rnd = new Random();
            var randomWord = rnd.Next(0, wordCount);
            var data = await _wordRepository.GetAll();
            var word = data.ToList()[randomWord];

            var roomPlayerList = JsonConvert.DeserializeObject<List<ConnectedUser>>(await _redisHandler.GetFromCache("Userlist:ConnectedUser")).Where(x => x.ConnectedRoomName == room.roomName).ToList();
            var shuffledList = ShuffleList(roomPlayerList);

            await _redisHandler.AddToCache($"Room:StartedGame:{room.roomName}", TimeSpan.FromMinutes(10), JsonConvert.SerializeObject(new StartedGame
            {
                word = word,
                userList = shuffledList,
                point = 120,
                turn = 1
            }));

            await _hubContext.Clients.Client(shuffledList[0].ConnectionId).SendAsync("YourTurn", true);
            await _hubContext.Clients.Group(room.roomName).SendAsync("StartTurnTimer", true);

            return await Task.FromResult(Ok(word));
        }
        [HttpPost("nextturn")]
        public async Task<IActionResult> NextTurn(Room room)
        {
            var data = JsonConvert.DeserializeObject<StartedGame>(await _redisHandler.GetFromCache($"Room:StartedGame:{room.roomName}"));
            var psuedoPlayer = data.userList[0];

            data.userList.RemoveAt(0);
            data.userList.Add(psuedoPlayer);
            data.turn += 1;

            await _hubContext.Clients.Client(psuedoPlayer.ConnectionId).SendAsync("YourTurn", false);
            await _hubContext.Clients.Client(data.userList[0].ConnectionId).SendAsync("YourTurn", true);
            await _hubContext.Clients.Group(room.roomName).SendAsync("StartTurnTimer", true);


            await _redisHandler.RemoveFromCache($"Room:StartedGame:{room.roomName}");
 
            await _redisHandler.AddToCache($"Room:StartedGame:{room.roomName}", TimeSpan.FromMinutes(10), JsonConvert.SerializeObject(data));

            return await Task.FromResult(Ok(data.userList[0]));
        }


        private List<E> ShuffleList<E>(List<E> inputList)
        {
            List<E> randomList = new List<E>();

            Random r = new Random();
            int randomIndex = 0;
            while (inputList.Count > 0)
            {
                randomIndex = r.Next(0, inputList.Count); //Choose a random object in the list
                randomList.Add(inputList[randomIndex]); //add it to the new, random list
                inputList.RemoveAt(randomIndex); //remove to avoid duplicates
            }

            return randomList; //return the new random list
        }

    }
}