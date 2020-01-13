using CizimAppEntity.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CizimAppData.Repository
{
    public class WordRepository : EfRepository<Word>, IWordRepository
    {
        public WordRepository(AppDbContext context) : base(context)
        {

        }
    }
}
