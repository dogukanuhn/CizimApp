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
        public string firstHint { get; set; }
        public string secondHint { get; set; }
        public string thirdHint { get; set; }
        public int point { get; set; }
    }
}
