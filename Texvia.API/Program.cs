
using Microsoft.EntityFrameworkCore;
using Texvia.Domain.Conctracts;
using Texvia.Persistence.Contexts;
using Texvia.Persistence.Seed;

namespace Texvia.API
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddDbContext<TexviaDBContext>(options =>
            {
                var ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

                options.UseSqlServer(ConnectionString);
            }
);
            builder.Services.AddScoped<IDBInializer, DBInializer>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            using var scope = app.Services.CreateScope();
            var dbInializer = scope.ServiceProvider.GetRequiredService<IDBInializer>();
            await dbInializer.InializeAsync();


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
