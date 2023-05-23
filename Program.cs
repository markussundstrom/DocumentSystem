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

        app.Run();
    }
}
