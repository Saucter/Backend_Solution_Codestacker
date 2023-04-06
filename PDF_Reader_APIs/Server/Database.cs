using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PDF_Reader_APIs.Shared;
using Microsoft.EntityFrameworkCore;

namespace PDF_Reader_APIs.Server
{
    public class Database : DbContext
    {
        public Database(DbContextOptions<Database> options) : base(options)
        {
            
        }

        public DbSet<PDF> PDF {get; set;}
    }
}