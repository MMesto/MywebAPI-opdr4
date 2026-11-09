using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MywebAPI.Data;
using MywebAPI.Data.Entities;
using MywebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// connectionstring staat in appsettings.json
builder.Services.AddDbContext<PortfolioDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PortfolioDb")));

// Identity: gebruikers en rollen opslaan via EF Core in dezelfde database
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<PortfolioDbContext>();

// scoped = nieuwe instantie per request, net als de DbContext
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<BlogpostService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
