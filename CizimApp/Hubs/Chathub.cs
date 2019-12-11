using CizimApp.Models;
using CizimApp.Repository;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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


        public Chathub(AppDbContext context, IConnectedUserRepository connectedUserRepository, IChatRepository chatRepository,IRoomRepository roomRepository)
        {
            _connectedUserRepository = connectedUserRepository;
            _chatRepository = chatRepository;
            _roomRepository = roomRepository;
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Client(Context.ConnectionId).SendAsync("SetConnectionId", Context.ConnectionId);
            var data = await _roomRepository.GetAll();
            //var data = await _context.Rooms.AsNoTracking().ToListAsync();
            await Clients.All.SendAsync("Notify", data);
            await base.OnConnectedAsync();
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
        public async Task AddToGroup(string groupName)
        {
            var room = await _roomRepository.FirstOrDefault(x => x.roomName == groupName);
            if (room != null)
            {
                room.roomUserCount += 1;
                await _roomRepository.Update(room);


                var data = await _roomRepository.GetAll();
                await Clients.All.SendAsync("Notify", data);


                var user = await _connectedUserRepository.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
                user.ConnectedRoomName = groupName;
                await _connectedUserRepository.Update(user);

                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

                await Clients.Group(groupName).SendAsync("GroupJoined", $"{Context.ConnectionId}-{user.Username} has joined the group.");
            }
        }

        public async Task RemoveFromGroup(string groupName)
        {
            
            var room = await _roomRepository.FirstOrDefault(x => x.roomName == groupName);
            room.roomUserCount-=1;

            var user = await _connectedUserRepository.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            user.ConnectedRoomName = null;
            await _connectedUserRepository.Update(user);
            if (room.roomAdmin == user.Username)
            {
                var admin = await _connectedUserRepository.FirstOrDefault(x => x.ConnectedRoomName == room.roomName);
                if (admin == null)
                {
                    await Clients.Group(groupName).SendAsync("IsClosed", true);
                    await _roomRepository.Remove(room);
                }
                else
                {
                    //test
                    room.roomAdmin = admin.Username;
                    await Clients.Client(admin.ConnectionId).SendAsync("AdminCall", true);
                    await _roomRepository.Update(room);
                }
            }


            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("GroupLeaved", $"{Context.ConnectionId}-{user.Username} has left the group {groupName}.");
            var data = await _roomRepository.GetAll();
            await Clients.All.SendAsync("Notify", data);


        }

    }
}
