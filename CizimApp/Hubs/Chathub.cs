
using CizimAppData;
using CizimAppData.Helpers;
using CizimAppData.Repository;
using CizimAppEntity.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimApp.Hubs
{
    public class Chathub : Hub
    {
        private readonly AppDbContext _context;
        private readonly IRoomRepository _roomRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IConnectedUserRepository _connectedUserRepository;
        private readonly IRedisHandler _redisHandler;


        public Chathub(IRedisHandler redisHandler, IConnectedUserRepository connectedUserRepository, IChatRepository chatRepository,IRoomRepository roomRepository)
        {
            _connectedUserRepository = connectedUserRepository;
            _chatRepository = chatRepository;
            _roomRepository = roomRepository;
            _redisHandler = redisHandler;
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Client(Context.ConnectionId).SendAsync("SetConnectionId", Context.ConnectionId);
            IEnumerable<Room> data = new List<Room>(); 
            if (await _redisHandler.IsCached("Room:Rooms"))
            {
                data = JsonConvert.DeserializeObject<List<Room>>(await _redisHandler.GetFromCache("Room:Rooms"));
            }
            else
            {
                data = await _roomRepository.GetAll();
            }
            await Clients.All.SendAsync("Notify", data);
            await base.OnConnectedAsync();
        }

        public async Task SendCanvas(string imageBase64,string groupName)
        {
            await Clients.Group(groupName).SendAsync("GetCanvas", imageBase64);
        }

        public async Task GetAllChatMessage(string groupName)
        {
            
            var data = await _chatRepository.GetWhere(x => x.RoomName == groupName);
            await Clients.Group(groupName).SendAsync("RoomMessage", data);
        }

        public async Task GiveHint(int hint,string groupName)
        {
            
            await Clients.Group(groupName).SendAsync("ReceivedHint", hint);
        }





        public async Task SendMessageToGroup(string groupName, string message,string username,string conId)
        {
            var data = new Chat
            {
                Message = message,
                RoomName = groupName,
                Username = username
            };

            if (await _redisHandler.IsCached($"Room:StartedGame:{groupName}"))
            {
                var gameRoom = JsonConvert.DeserializeObject<StartedGame>(await _redisHandler.GetFromCache($"Room:StartedGame:{groupName}"));
                if (gameRoom.word.WordName == message)
                {
                    data.Answer = true;

                    if (await _chatRepository.CountWhere(x => x.Answer == true && x.RoomName == groupName) == 0)
                    {
                        
                        gameRoom.userList.FirstOrDefault(x => x.Username == username).GamePoint += 10;
                        await Clients.Client(conId).SendAsync("DisableChat", true);

                    }
                    else if (await _chatRepository.CountWhere(x => x.Answer == true && x.RoomName == groupName) == 1)
                    {
                        gameRoom.userList.FirstOrDefault(x => x.Username == username).GamePoint += 8;
                        await Clients.Client(conId).SendAsync("DisableChat", true);
                    }
                    else if (await _chatRepository.CountWhere(x => x.Answer == true && x.RoomName == groupName) == 2)
                    {
                        gameRoom.userList.FirstOrDefault(x => x.Username == username).GamePoint += 5;
                        await Clients.Client(conId).SendAsync("DisableChat", true);
                    }
                    else if (await _chatRepository.CountWhere(x => x.Answer == true && x.RoomName == groupName) > 2)
                    {
                        gameRoom.userList.FirstOrDefault(x => x.Username == username).GamePoint += 3;
                        await Clients.Client(conId).SendAsync("DisableChat", true);
                    }
                    gameRoom.userList[0].GamePoint += 2;

                    await _redisHandler.RemoveFromCache($"Room:StartedGame:{groupName}");

                    await _redisHandler.AddToCache($"Room:StartedGame:{groupName}", TimeSpan.FromMinutes(10), JsonConvert.SerializeObject(gameRoom));
                }
            }

            await _chatRepository.Add(data);

            await Clients.Group(groupName).SendAsync("GroupMessage", data);

        }

        public async Task<string> GetConnecionId()
        {
            return await Task.FromResult(Context.ConnectionId);
        }



    }
}
