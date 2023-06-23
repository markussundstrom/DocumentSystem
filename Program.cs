using Microsoft.EntityFrameworkCore;
using DocumentSystem.Models;
using DocumentSystem.Services;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace DocumentSystem;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        JWTOptions jwtOptions = builder.Configuration
            .GetSection("JWT").Get<JWTOptions>();
        // Add services to the container.

        string connstr = 
            builder.Configuration.GetConnectionString("DefaultConnection");
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
        builder.Services.AddSingleton(jwtOptions);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => {
                    byte[] signingKeyBytes = Encoding.UTF8
                        .GetBytes(jwtOptions.SigningKey);

                    options.TokenValidationParameters = 
                            new TokenValidationParameters() {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,
                                ValidIssuer = jwtOptions.Issuer,
                                ValidAudience = jwtOptions.Audience,
                                IssuerSigningKey = 
                                    new SymmetricSecurityKey(signingKeyBytes)
                            };
            })
        ;
        builder.Services.AddAuthorization();
        


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
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
public class JWTOptions {
    public string Issuer {get; set;}
    public string Audience {get; set;}
    public string SigningKey {get; set;}
    public int ExpirationSeconds {get; set;}
}
