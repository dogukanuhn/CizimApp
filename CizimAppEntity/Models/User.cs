
using CizimAppData.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CizimAppEntity.Models
{
    public class User:IEntity
    {
        public Guid Id { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }


    }
}
