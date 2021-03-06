﻿
using CizimAppData.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimAppEntity.Models
{
    public class Room : IEntity
    {
       
        public Guid Id { get; set; }
        public string roomName { get; set; }
        public int roomUserCount { get; set; }
        public int roomMaxUserCount { get; set; }
        public int roomPoint { get; set; }
        public bool roomIsGameStart { get; set; }
        public string roomPassword { get; set; }
        public string roomAdmin { get; set; }
        public DateTime CreationDate { get; set; }

    }
}
