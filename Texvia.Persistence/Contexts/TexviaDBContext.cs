using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Texvia.Domain.Models;

namespace Texvia.Persistence.Contexts
{
    public class TexviaDBContext(DbContextOptions<TexviaDBContext> options) : IdentityDbContext(options)
    {


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
        DbSet<Contact> Contacts { get; set; }
        DbSet<Industry> Industries { get; set; }
        DbSet<Solution> Solutions { get; set; }
        DbSet<SolutionProviders> SolutionProviders { get; set; }
    }
}
