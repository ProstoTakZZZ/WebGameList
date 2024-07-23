using GList.Controllers;
using GList.Models;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GameContext>(options =>
    options.UseSqlite("Data Source=games.db"));

builder.Services.AddTransient<GameController>();

var app = builder.Build();

app.Run(async (context) =>
{
    using (var scope = app.Services.CreateScope())
    {
        var gameController = scope.ServiceProvider.GetRequiredService<GameController>();
        await gameController.HandleRequest(context);
    }
});

app.Run();