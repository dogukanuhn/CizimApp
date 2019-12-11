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

namespace CizimApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IHubContext<Chathub> _hubContext;

        private readonly IRoomRepository _roomRepository;
        public RoomController(IHubContext<Chathub> hubContext, IRoomRepository roomRepository, AppDbContext context)
        {
            _roomRepository = roomRepository;
            _hubContext = hubContext;

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

    }
}