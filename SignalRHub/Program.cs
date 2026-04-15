using Scalar.AspNetCore;
using SignalRHub;
using SignalRHub.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
var redisConnectionString = builder.Configuration["Redis:ConnectionString"];

var signalRBuilder = builder.Services.AddSignalR();
if (!string.IsNullOrEmpty(redisConnectionString))
    signalRBuilder.AddStackExchangeRedis(redisConnectionString);
builder.Services.AddSingleton<FilterService>();

builder.Services.AddCors(options =>
    options.AddPolicy("VueDev", policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors("VueDev");
app.UseAuthorization();

app.MapControllers();
app.MapHub<MessageHub>("/hub");

app.Run();