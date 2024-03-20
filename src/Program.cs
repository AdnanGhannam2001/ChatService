var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var cookies = builder.Configuration["Cookies"] ?? throw new Exception("`Cookies` should be defined in `appsettings.json`");

builder.Services.AddAuthentication(cookies)
    .AddCookie(cookies);
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
