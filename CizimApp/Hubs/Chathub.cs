using CizimApp.Models;
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
        public Chathub(AppDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Client(Context.ConnectionId).SendAsync("SetConnectionId", Context.ConnectionId);
            var data = await _context.Rooms.AsNoTracking().ToListAsync();
            await Clients.All.SendAsync("Notify", data);
            await base.OnConnectedAsync();
        }

        public async Task GetAllChatMessage(string groupName)
        {
            var data = await _context.Chats.AsNoTracking().Where(x => x.RoomName == groupName).ToArrayAsync();
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
            await _context.Chats.AddAsync(data);

            await _context.SaveChangesAsync();
            await Clients.Group(groupName).SendAsync("GroupMessage", data);

        }
        public async Task AddToGroup(string groupName)
        {
            var user = await _context.ConnectedUsers.FirstOrDefaultAsync(x => x.ConnectionId == Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("GroupJoined", $"{Context.ConnectionId}-{user.Username} has joined the group.");
        }
        public async Task RemoveFromGroup(string groupName)
        {
            var user = await _context.ConnectedUsers.FirstOrDefaultAsync(x => x.ConnectionId == Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("GroupLeaved", $"{Context.ConnectionId}-{user.Username} has left the group {groupName}.");
        }

    }
}
