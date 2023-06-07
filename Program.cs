using Microsoft.EntityFrameworkCore;
using DocumentSystem.Models;
using DocumentSystem.Services;
namespace DocumentSystem;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        string connstr = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddScoped<DocumentSystemService>();
        builder.Services.AddScoped<FileService>();
        builder.Services.AddScoped<UserService>();
        builder.Services.AddControllers();
        builder.Services.AddDbContext<DocumentSystemContext>(
            opt => opt.UseMySql(
                connstr, ServerVersion.AutoDetect(connstr)
            )
        );
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
//        builder.Services.AddScoped(DocumentSystemService);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        CreateDbIfNotExists(app);

        app.Run();
    }
     private static void CreateDbIfNotExists(IHost host) {
        using (var scope = host.Services.CreateScope()) {
            var services = scope.ServiceProvider;
            try {
                var context = services.GetRequiredService<DocumentSystemContext>();
                DbInitializer.Seed(context);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred creating the DB.");
            }
        }
    }
}
