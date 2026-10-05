using LMS.API.Extensions;
using LMS.API.Services;
using LMS.Infrastructure.Data;
using LMS.Presentation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // "ActiveConnection" picks which connection string to use; unset means the developer's local database.
        var activeConnection = builder.Configuration["ActiveConnection"];
        var connectionName = string.IsNullOrWhiteSpace(activeConnection) ? "ApplicationDbContext" : activeConnection;
        var connectionString = builder.Configuration.GetConnectionString(connectionName) ?? throw new InvalidOperationException($"Connection string '{connectionName}' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        builder.Services.AddControllers(opt =>
        {
            opt.ReturnHttpNotAcceptable = true;
            opt.Filters.Add(new ProducesAttribute("application/json"));
        })
        .AddApplicationPart(typeof(AssemblyReference).Assembly);

        builder.Services.AddHostedService<DataSeedService>();
        builder.Services.ConfigureSwagger();
        builder.Services.AddRepositories();
        builder.Services.AddServiceLayer();
        builder.Services.ConfigureAuthentication(builder.Configuration);
        builder.Services.ConfigureIdentity();
        builder.Services.ConfigurePolicys();

        var app = builder.Build();

        app.Logger.LogInformation("Using connection string '{ConnectionName}'", connectionName);

        app.ConfigureExceptionHandler();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();

            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers()
           .RequireAuthorization("Default");

        app.Run();
    }
}