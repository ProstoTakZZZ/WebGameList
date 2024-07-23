using GList.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace GList.Controllers
{
    public class GameController
    {
        private readonly GameContext _context;

        public GameController(GameContext context)
        {
            _context = context;
        }

        public async Task HandleRequest(HttpContext context)
        {
            var response = context.Response;
            var request = context.Request;
            var path = request.Path;

            string expressionForGuid = @"^/api/games/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";

            if (path == "/api/games" && request.Method == "GET")
            {
                await GetAllGames(response);
            }
            else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "GET")
            {
                string? id = path.Value?.Split("/")[3];
                await GetGame(id, response);
            }
            else if (path == "/api/games" && request.Method == "POST")
            {
                await CreateGame(response, request);
            }
            else if (path == "/api/games" && request.Method == "PUT")
            {
                await UpdateGame(response, request);
            }
            else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "DELETE")
            {
                string? id = path.Value?.Split("/")[3];
                await DeleteGame(id, response);
            }
            else
            {
                response.ContentType = "text/html; charset=utf-8";
                await response.SendFileAsync("html/index.html");
            }
        }

        private async Task GetAllGames(HttpResponse response)
        {
            var games = await _context.Games.ToListAsync();
            await response.WriteAsJsonAsync(games);
        }

        private async Task GetGame(string? id, HttpResponse response)
        {
            var game = await _context.Games.FindAsync(id);
            if (game != null)
                await response.WriteAsJsonAsync(game);
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "Игра не найдена" });
            }
        }

        private async Task DeleteGame(string? id, HttpResponse response)
        {
            var game = await _context.Games.FindAsync(id);
            if (game != null)
            {
                _context.Games.Remove(game);
                await _context.SaveChangesAsync();
                await response.WriteAsJsonAsync(game);
            }
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "Игра не найдена" });
            }
        }

        private async Task CreateGame(HttpResponse response, HttpRequest request)
        {
            try
            {
                var game = await request.ReadFromJsonAsync<Game>();
                if (game != null)
                {
                    game.Id = Guid.NewGuid().ToString();
                    _context.Games.Add(game);
                    await _context.SaveChangesAsync();
                    await response.WriteAsJsonAsync(game);
                }
                else
                {
                    throw new Exception("Некорректные данные");
                }
            }
            catch (Exception)
            {
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new { message = "Некорректные данные" });
            }
        }

        private async Task UpdateGame(HttpResponse response, HttpRequest request)
        {
            try
            {
                var gameData = await request.ReadFromJsonAsync<Game>();
                if (gameData != null)
                {
                    var game = await _context.Games.FindAsync(gameData.Id);
                    if (game != null)
                    {
                        game.Name = gameData.Name;
                        game.Rating = gameData.Rating;
                        game.Completed = gameData.Completed;
                        await _context.SaveChangesAsync();
                        await response.WriteAsJsonAsync(game);
                    }
                    else
                    {
                        response.StatusCode = 404;
                        await response.WriteAsJsonAsync(new { message = "Игра не найдена" });
                    }
                }
                else
                {
                    throw new Exception("Некорректные данные");
                }
            }
            catch (Exception)
            {
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new { message = "Некорректные данные" });
            }
        }
    }
}
