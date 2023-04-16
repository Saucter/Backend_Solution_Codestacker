using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PDF_Reader_APIs.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace PDF_Reader_APIs.Server
{
    public class Database : DbContext //Inherit's from EF Core's DBContext. Allows migrations to be added through the class
    {
        public Database(){}
        public Database(DbContextOptions<Database> options) : base(options){}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PDF>().HasMany(x => x.Sentences).WithOne(y => y.PDF).HasForeignKey(z => z.PDFid); //Creates a One-To-Many relationship between PDF and Sentences   
            base.OnModelCreating(modelBuilder);
        }
        
        //Maps the PDF and Sentences classes into the DB
        public DbSet<PDF> PDFs {get; set;} 
        public DbSet<Sentences> Sentences {get; set;}
    }
}