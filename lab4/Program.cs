using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder();
var app = builder.Build();

app.UseRouting();

    var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

app.Map("/Library/Books", booksApp =>
{
    booksApp.Run(async context =>
    {
        var booksConfigFile = config["BooksConfigFile"];
        var books = File.ReadAllLines(booksConfigFile);
        await context.Response.WriteAsync(string.Join("\n", books));
    });
});

app.MapGet("/Library/Profile/{id?}", async (context) =>
{
    var id = context.Request.RouteValues["id"]?.ToString();
    var profilesConfigFile = config["ProfilesConfigFile"];

    if (string.IsNullOrEmpty(id))
    {
        var allprofJson = File.ReadAllText(profilesConfigFile);
        var all = JsonConvert.DeserializeObject<Dictionary<string, UserProfile>>(allprofJson);

        await context.Response.WriteAsync("Information about all library users:\n");
        foreach (var userProfile in all)
        {
            await context.Response.WriteAsync($"id: {userProfile.Key}\n");
            await context.Response.WriteAsync($"Name: {userProfile.Value.Name}\n");
            await context.Response.WriteAsync($"Email: {userProfile.Value.Email}\n");
            await context.Response.WriteAsync("\n");
        }
    }
    else if (int.TryParse(id, out var userId) && userId >= 0 && userId <= 5)
    {
        var allprofJson = File.ReadAllText(profilesConfigFile);
        var all = JsonConvert.DeserializeObject<Dictionary<string, UserProfile>>(allprofJson);

        if (all.TryGetValue(id, out var userProfile))
        {
            await context.Response.WriteAsync($"Information about library user with ID {userId}:\n");
            await context.Response.WriteAsync($"id: {id}\n");
            await context.Response.WriteAsync($"Name: {userProfile.Name}\n");
            await context.Response.WriteAsync($"Email: {userProfile.Email}\n");
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync("User not found.");
        }
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("not found id.");
    }
});

app.Map("/Library", libraryApp =>
{
    libraryApp.Run(async context =>
    {
        await context.Response.WriteAsync("Welcome to the Library!");
    });
});

app.Run();

public class UserProfile
{
    public string Name { get; set; }
    public string Email { get; set; }
}