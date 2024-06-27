using ChatService.Endpoints;
using ChatService.Extensions;
using ChatService.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddHttpContextAccessor()
    .AddRealtimeConnection()
    .AddAuth()
    .AddCors()
#if DEBUG && !NO_RABBIT_MQ
    .AddRabbitMQ()
#endif
    .RegisterServices();

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