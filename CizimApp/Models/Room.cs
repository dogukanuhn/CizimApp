using CizimApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimApp.Models
{
    public class Room : IEntity
    {
       
        public Guid Id { get; set; }
        public string roomName { get; set; }
        public int roomUserCount { get; set; }
        public int roomMaxUserCount { get; set; }
        public string roomPassword { get; set; }
        public string roomAdmin { get; set; }
        public DateTime CreationDate { get; set; }

    }
}
