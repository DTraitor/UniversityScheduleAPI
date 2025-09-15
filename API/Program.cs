using BusinessLogic.Jobs;
using BusinessLogic.Services;
using BusinessLogic.Services.ElectiveLessons;
using BusinessLogic.Services.GroupLessons;
using BusinessLogic.Services.Interfaces;
using DataAccess.Domain;
using DataAccess.Repositories;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using API.Extensions;
using DataAccess.Models;
using DataAccess.Models.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGroupService, GroupService>();

// Repositories
builder.Services.AddScoped<IPersistentDataRepository, PersistentDataRepository>();
builder.Services.AddScoped<IUserModifiedRepository, UserModifiedRepository>();
builder.Services.AddScoped<IRepository<ElectiveLessonModified>, ElectiveLessonModifiedRepository>();
builder.Services.AddScoped<IRepository<GroupLessonModified>, GroupLessonModifiedRepository>();
builder.Services.AddRepository<IGroupRepository, GroupRepository, Group>();
builder.Services.AddRepository<IUserRepository, UserRepository, User>();
builder.Services.AddRepository<IUserLessonRepository, UserLessonRepository, UserLesson>();
builder.Services.AddRepository<IUserLessonOccurenceRepository, UserLessonOccurenceRepository, UserLessonOccurrence>();
builder.Services.AddRepository<IElectedLessonRepository, ElectedLessonRepository, ElectedLesson>();
builder.Services.AddRepository<IElectiveLessonDayRepository, ElectiveLessonDayRepository, ElectiveLessonDay>();
builder.Services.AddKeyBasedRepository<IElectiveLessonRepository, ElectiveLessonRepository, ElectiveLesson>();
builder.Services.AddKeyBasedRepository<IGroupLessonRepository, GroupLessonRepository, GroupLesson>();

builder.Services.AddHttpClient();

// Schedule sources
builder.Services.AddScheduleSource<GroupLesson, GroupLessonModified, GroupScheduleParser, GroupScheduleReader, GroupLessonUpdaterService, GroupLessonUserUpdaterService>();
builder.Services.AddScheduleSource<ElectiveLesson, ElectiveLessonModified, ElectiveScheduleParser, ElectiveScheduleReader, ElectiveLessonUpdaterService, ElectiveUserUpdaterService>();

// Jobs
builder.Services.AddHostedService<OccurrencesUpdaterJob>();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ScheduleDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ScheduleDBConnection")));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("configs/appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var app = builder.Build();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
