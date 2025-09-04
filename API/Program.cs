using BusinessLogic.Services;
using DataAcc_ess.Repositories.Interfaces;
using DataAccess.Domain;
using DataAccess.Repositories;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Reader.Services;
using Reader.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<IGroupScheduleReader, GroupScheduleReader>();
builder.Services.AddSingleton<IElectiveScheduleReader, ElectiveScheduleReader>();

builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IElectiveLessonRepository, ElectiveLessonRepository>();
builder.Services.AddScoped<IScheduleLessonRepository, ScheduleLessonRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRemoveGroupRepository, RemoveGroupRepository>();

builder.Services.AddHostedService<DailyScheduleUpdateService>();
builder.Services.AddHttpClient<DailyScheduleUpdateService>(c => c.BaseAddress = new Uri("https://portal.nau.edu.ua"));

builder.Services.AddDbContext<ScheduleDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ScheduleDBConnection")));
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("UserDBConnection")));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();