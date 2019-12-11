using CizimApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimApp.Models
{
    public class ConnectedUser: IEntity
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string ConnectionId { get; set; }
        public string ConnectedRoomName { get; set; }
        public DateTime ConnectionTime { get; set; }
    }
}
