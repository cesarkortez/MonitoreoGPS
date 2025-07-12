using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://*:80");

// Registramos controladores
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registramos el DbContext con PostgreSQL
builder.Services.AddDbContext<AuditDbContext>(opt =>
    opt.UseNpgsql("Host=postgres;Port=5432;Database=gpsdb;Username=cesar;Password=1234567890"));

var app = builder.Build();

//  Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthorization();

// Mapeamos controladores
app.MapControllers();

app.Run();
