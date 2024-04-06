using System.Security.Claims;
using ChatService.Consumers;
using ChatService.Data;
using ChatService.Endpoints;
using ChatService.Interfaces;
using ChatService.Services;
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
    config.RegisterConsumer<GroupDeletedEventConsumer>();
    config.RegisterConsumer<FriendshipCreatedEventConsumer>();
    config.RegisterConsumer<FriendshipDeletedEventConsumer>();
    config.RegisterConsumer<MemberJoinedEventConsumer>();
    config.RegisterConsumer<MemberLeavedEventConsumer>();
    config.RegisterConsumer<MemberRoleChangedEventConsumer>();

    config.UsingRabbitMq((context, rmqConfig) => {
        rmqConfig.ReceiveEndpoint("group-created-event", e => e.ConfigureConsumer<GroupCreatedEventConsumer>(context));
        rmqConfig.ReceiveEndpoint("group-deleted-event", e => e.ConfigureConsumer<GroupCreatedEventConsumer>(context));
        rmqConfig.ReceiveEndpoint("friendship-created-event", e => e.ConfigureConsumer<FriendshipCreatedEventConsumer>(context));
        rmqConfig.ReceiveEndpoint("friendship-deleted-event", e => e.ConfigureConsumer<FriendshipDeletedEventConsumer>(context));
        rmqConfig.ReceiveEndpoint("member-joined-event", e => e.ConfigureConsumer<MemberJoinedEventConsumer>(context));
        rmqConfig.ReceiveEndpoint("member-leaved-event", e => e.ConfigureConsumer<MemberLeavedEventConsumer>(context));
        rmqConfig.ReceiveEndpoint("member-role-changed-event", e => e.ConfigureConsumer<MemberRoleChangedEventConsumer>(context));
    });
});

builder.Services.AddScoped<DapperDbConnection>();
builder.Services.AddScoped<IChatsService, ChatsService>();

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
    var claim = new Claim(ClaimTypes.NameIdentifier, id);

    var identity = new ClaimsIdentity([claim], cookies);

    var principal = new ClaimsPrincipal(identity);

    await context.SignInAsync(principal);

    return Results.Ok("Logged In");
});

app.Run();
