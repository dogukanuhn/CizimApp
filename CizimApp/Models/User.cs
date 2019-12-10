using CizimApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimApp.Models
{
    public class User:IEntity
    {
        public Guid Id { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }


    }
}
