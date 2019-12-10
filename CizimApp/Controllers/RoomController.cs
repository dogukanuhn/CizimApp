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

namespace CizimApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IHubContext<Chathub> _hubContext;
        private readonly AppDbContext _context;
        public RoomController(IHubContext<Chathub> hubContext, AppDbContext context) 
        {
            _hubContext = hubContext;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Insert([FromBody] string roomName)
        {
            await _context.Rooms.AddAsync(new Room {roomName=roomName });
            await _context.SaveChangesAsync();

            var data = await _context.Rooms.AsNoTracking().ToListAsync();
            await _hubContext.Clients.All.SendAsync("Notify", data);

            return await Task.FromResult(Ok());
        }

    }
}