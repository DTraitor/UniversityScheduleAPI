using BusinessLogic.Jobs;
using BusinessLogic.Services;
using BusinessLogic.Services.Interfaces;
using DataAccess.Domain;
using DataAccess.Repositories;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using BusinessLogic.Services.Readers;
using BusinessLogic.Services.Readers.Interfaces;
using DataAccess.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IScheduleReader, GroupScheduleReader>();
builder.Services.AddSingleton<IScheduleParser<GroupLesson>, GroupScheduleParser>();
builder.Services.AddSingleton<IScheduleParser<ElectiveLesson>, ElectiveScheduleParser>();

builder.Services.AddScoped<IScheduleService, ScheduleService> ();
builder.Services.AddScoped<IUserService, UserService> ();
builder.Services.AddScoped<IGroupService, GroupService> ();

builder.Services.AddScoped<IPersistentDataRepository, PersistentDataRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IElectiveLessonRepository, ElectiveLessonRepository>();
builder.Services.AddScoped<IGroupLessonRepository, GroupLessonRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserLessonRepository, UserLessonRepository>();
builder.Services.AddScoped<IUserLessonOccurenceRepository, UserLessonOccurenceRepository>();
builder.Services.AddScoped<IElectedLessonRepository, ElectedLessonRepository>();
builder.Services.AddScoped<IElectiveLessonDayRepository, ElectiveLessonDayRepository>();

builder.Services.AddHttpClient();

builder.Services.AddHostedService<ScheduleParserJob123>();
builder.Services.AddHostedService<ScheduleParserJob123>();
builder.Services.AddHostedService<OccurrencesUpdaterJob>();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ScheduleDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ScheduleDBConnection")));
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("UserDBConnection")));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

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