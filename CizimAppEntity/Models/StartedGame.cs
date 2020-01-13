using System;
using System.Collections.Generic;
using System.Text;

namespace CizimAppEntity.Models
{
    public class StartedGame
    {
        public Word word { get; set; }
        public List<ConnectedUser> userList { get; set; }
        public int turn { get; set; }
        public int point { get; set; }
    }
}
