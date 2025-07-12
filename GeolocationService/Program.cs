using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://*:80");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Agregamos Redis como singleton
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect("redis:6379")); // "redis" será el nombre del contenedor de Redis en Docker

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers(); //habilitamos los controladores

app.Run();
