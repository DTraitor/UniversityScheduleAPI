using BusinessLogic.Services;
using BusinessLogic.Services.Interfaces;
using DataAccess.Domain;
using DataAccess.Repositories;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using BusinessLogic.Services.Readers;
using BusinessLogic.Services.Readers.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<HostOptions>(opts =>
    opts.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);

// Add services to the container.

builder.Services.AddSingleton<IGroupsListReader, GroupsListReader>();
builder.Services.AddSingleton<IGroupScheduleReader, GroupScheduleReader>();
builder.Services.AddSingleton<IElectiveScheduleReader, ElectiveScheduleReader>();


builder.Services.AddScoped<IScheduleService, ScheduleService> ();
builder.Services.AddScoped<IUserService, UserService> ();

builder.Services.AddScoped<IPersistentDataRepository, PersistentDataRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IElectiveLessonRepository, ElectiveLessonRepository>();
builder.Services.AddScoped<IScheduleLessonRepository, ScheduleLessonRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserLessonRepository, UserLessonRepository>();
builder.Services.AddScoped<IUserLessonOccurenceRepository, UserLessonOccurenceRepository>();

builder.Services.AddHttpClient();
builder.Services.AddHostedService<DailyScheduleUpdateService>();
builder.Services.AddHostedService<OccurrencesUpdaterService>();

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