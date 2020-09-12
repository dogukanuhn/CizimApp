
using CizimAppData.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimAppEntity.Models
{
    public class Chat : IEntity
    {
        public Guid Id { get; set; }
        public string RoomName { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public bool Answer { get; set; }

        public DateTime Date { get; set; }



    }
}
