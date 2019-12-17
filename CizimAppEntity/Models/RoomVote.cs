using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimAppEntity.Models
{
    public class RoomVote
    {
        public Guid Id { get; set; }
        public string ConnectionId { get; set; }
        public string Username { get; set; }
        public int Yes { get; set; }
        public int No { get; set; }
    }
}
