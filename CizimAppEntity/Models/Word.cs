using CizimAppData.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace CizimAppEntity.Models
{
    public class Word : IEntity
    {
        public Guid Id { get ; set ; }
        public string WordName { get; set; }

    }
}
