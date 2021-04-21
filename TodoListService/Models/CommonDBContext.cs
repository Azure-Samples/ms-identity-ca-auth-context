using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
//using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListService.Models
{
    public class CommonDBContext : DbContext
    {
        public CommonDBContext(DbContextOptions<CommonDBContext> options) : base(options)
        {

        }
        public DbSet<Todo> Todo{get;set;}
        public DbSet<AuthContext> AuthContext { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AuthContext>().HasKey(x => new { x.TenantId, x.AuthContextType });
        }
    }
}
