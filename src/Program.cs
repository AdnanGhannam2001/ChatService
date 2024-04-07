using System.Security.Claims;
using ChatService.Data;
using ChatService.Endpoints;
using ChatService.Extensions;
using ChatService.Hubs;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddRealtimeConnection()
    .AddAuth()
    .AddRabbitMQ()
    .RegisterServices();

var app = builder.Build();

if (args.Contains("--create-db")) {
    using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
    var dbConnection = scope.ServiceProvider.GetRequiredService<DapperDbConnection>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Creating Tables...");
    await Database.CreateTablesAsync(dbConnection);
    logger.LogInformation("Tables Were Created Successfully");
    return;
}

if (args.Contains("--seed")) {
    using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
    var dbConnection = scope.ServiceProvider.GetRequiredService<DapperDbConnection>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Seeding...");
    await Database.SeedAsync(dbConnection);
    logger.LogInformation("Database Were Seeded Successfully");
    return;
}

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Mock Login
app.MapGet("/login/{id}", async (string id, HttpContext context) => {
    var cookies = app.Configuration["Cookies"];

    var claim = new Claim(ClaimTypes.NameIdentifier, id);

    var identity = new ClaimsIdentity([claim], cookies);

    var principal = new ClaimsPrincipal(identity);

    await context.SignInAsync(principal);

    return Results.Ok("Logged In");
});

app.MapHub<ChatHub>("/websocket/chat");

app.MapGroup("api/chats")
    .MapChatEndpoints()
    .RequireAuthorization();

app.Run();