using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MywebAPI.Data;
using MywebAPI.Data.Entities;
using MywebAPI.Services;
using MywebAPI.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// connectionstring staat in appsettings.json
builder.Services.AddDbContext<PortfolioDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PortfolioDb")));

builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<PortfolioDbContext>();

    builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

    builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.KanSchrijven, Policies.KanSchrijvenRegel);

// scoped = nieuwe instantie per request, net als de DbContext
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<BlogpostService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    await GebruikerSeeder.SeedAsync(app.Services, app.Configuration);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
