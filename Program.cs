
using IndProBackend.Context;
using IndProBackend.Interfaces;
using IndProBackend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace IndProBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            //registering user service
            builder.Services.AddScoped<IUserService, UserService>();

            //configure authentication service
            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    //ClockSkew = TimeSpan.Zero,
                   
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        //context.NoResult();
                        //context.Response.StatusCode = 500;
                        //context.Response.ContentType = "text/plain";
                        //if (builder.Environment.IsDevelopment())
                        //{
                        //    return context.Response.WriteAsync(context.Exception.ToString());
                        //}
                        //return context.Response.WriteAsync("An error occurred processing your authentication.");
                        Console.WriteLine("Token authnetication failed: {0}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    //OnTokenValidated = context =>
                    //{
                    //    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    //    logger.LogInformation("Token validated successfully");
                    //    return Task.CompletedTask;
                    //},
                    //OnChallenge = context =>
                    //{
                    //    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    //    logger.LogWarning("OnChallenge error: {}", context.Error);
                    //    return Task.CompletedTask;
                    //},
                    //OnMessageReceived = context =>
                    //{
                    //    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    //    logger.LogInformation("Received token: {}", context.Token);
                    //    return Task.CompletedTask;
                    //}
                };
            });

            //registering db context
            builder.Services.AddDbContext<MyContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ConString")));


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ecom api", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    //Type = SecuritySchemeType.ApiKey,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
