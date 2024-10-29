using System.Reflection;
using ChatService.Constants;
using ChatService.Endpoints;
using ChatService.Extensions;
using ChatService.Hubs;
using PR2.RabbitMQ.Extensions;

var builder = WebApplication.CreateBuilder(args);

{
    var rmqConfig = builder.Configuration.GetSection(CommonConstants.RabbitMqSettings);

    builder.Services
        .AddEndpointsApiExplorer()
        .AddSwaggerGen()
        .AddHttpContextAccessor()
        .AddRealtimeConnection()
        .AddAuth()
        .AddCors()
    #if !NO_RABBIT_MQ
        .AddRabbitMQ(Assembly.GetExecutingAssembly(),
            rmqConfig["Host"]!,
            rmqConfig["Username"]!,
            rmqConfig["Password"]!)
    #endif
        .RegisterServices();
}

var app = builder.Build();
app.HandleCommandArguments(args);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(x =>
{
    x
        .SetIsOriginAllowed(x => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/websocket/chat");

app.MapGroup("api/chats")
    .MapChatEndpoints()
    .RequireAuthorization()
    .WithOpenApi();

app.Run();