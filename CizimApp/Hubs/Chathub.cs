
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
            //var data = await _context.Chats.AsNoTracking().Where(x => x.RoomName == groupName).ToArrayAsync();
            var data = await _chatRepository.GetWhere(x => x.RoomName == groupName);
            await Clients.Group(groupName).SendAsync("RoomMessage", data);
        }

       

        public async Task SendMessageToGroup(string groupName, string message,string username)
        {
            var data = new Chat
            {
                Message = message,
                RoomName = groupName,
                Username = username

            };
            await _chatRepository.Add(data);
            //await _context.Chats.AddAsync(data);

            await Clients.Group(groupName).SendAsync("GroupMessage", data);

        }
        public async Task<string> GetConnecionId()
        {
            return await Task.FromResult(Context.ConnectionId);
        }



    }
}
