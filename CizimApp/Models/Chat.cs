using CizimApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimApp.Models
{
    public class Chat : IEntity
    {
        public Guid Id { get; set; }
        public string RoomName { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }



    }
}
