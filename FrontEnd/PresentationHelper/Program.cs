using AppCommon.Services;
using PsqlAccess.SecListMaintain;
using PsqlAccess;

const string ApplicationName = "PresentationHelper";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
IConfiguration? Configuration = ServiceHandler.GetConfiguration(ApplicationName);
ServiceHandler.SetupDatabaseConnection(builder.Services, Configuration, forceConnect: true);
ServiceHandler.SetupLogger(builder.Services, Configuration, ApplicationName);
SetupDI(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

void SetupDI(IServiceCollection services)
{
    services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
}