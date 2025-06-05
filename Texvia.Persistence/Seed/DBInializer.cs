using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Texvia.Domain.Conctracts;
using Texvia.Persistence.Contexts;

namespace Texvia.Persistence.Seed
{
    public class DBInializer(TexviaDBContext context) : IDBInializer
    {
        public async Task InializeAsync()
        {
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }
        }
    }
}
