using System.Security.Claims;
using ChatService.Consumers;
using ChatService.Data;
using MassTransit;
using MassTransit.Configuration;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var cookies = builder.Configuration["Cookies"] ?? throw new Exception("`Cookies` should be defined in `appsettings.json`");

builder.Services.AddAuthentication(cookies)
    .AddCookie(cookies);
builder.Services.AddAuthorization();

builder.Services.AddMassTransit(config => {
    config.RegisterConsumer<GroupCreatedEventConsumer>();

    config.UsingRabbitMq((context, rmqConfig) => {
        rmqConfig.ReceiveEndpoint("group-created-event", e => e.ConfigureConsumer<GroupCreatedEventConsumer>(context));
    });
});

builder.Services.AddScoped<DapperDbConnection>();

var app = builder.Build();


if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Mock Login
app.MapGet("/login/{id}", async (string id, HttpContext context) => {
    var claim = new Claim("id", id);

    var identity = new ClaimsIdentity([claim], cookies);

    var principal = new ClaimsPrincipal(identity);

    await context.SignInAsync(principal);

    return Results.Ok("Logged In");
});

app.MapGet("unauthorized", () => Results.Ok("UnAuthorized"));

app.MapGet("secret", () => Results.Ok("Secret")).RequireAuthorization();

app.Run();
