using BusinessLogic.Jobs;
using BusinessLogic.Services;
using BusinessLogic.Services.Interfaces;
using DataAccess.Domain;
using DataAccess.Repositories;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using API.Extensions;
using BusinessLogic.Configuration;
using BusinessLogic.Parsing;
using BusinessLogic.Parsing.Interfaces;
using DataAccess.Models;
using DataAccess.Models.Internal;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IElectiveService, ElectiveService>();
builder.Services.AddScoped<IUserLessonUpdaterService, UserLessonUpdaterService>();
builder.Services.AddScoped<ILessonUpdaterService, LessonUpdaterService>();
builder.Services.AddSingleton<IUserAlertService, UserAlertService>();
builder.Services.AddSingleton<IUsageMetricService, UsageMetricService>();
builder.Services.AddSingleton<IScheduleReader, ScheduleReader>();
builder.Services.AddSingleton<IScheduleParser, ScheduleParser>();
builder.Services.AddSingleton<IChangeHandler, ChangeHandler>();

// Repositories
builder.Services.AddScoped<IUserModifiedRepository, UserModifiedRepository>();
builder.Services.AddScoped<IPersistentDataRepository, PersistentDataRepository>();
builder.Services.AddRepository<IUserRepository, UserRepository, User>();
builder.Services.AddRepository<IUserAlertRepository, UserAlertRepository, UserAlert>();
builder.Services.AddRepository<IUserLessonRepository, UserLessonRepository, UserLesson>();
builder.Services.AddRepository<IUserLessonOccurenceRepository, UserLessonOccurenceRepository, UserLessonOccurrence>();
builder.Services.AddRepository<IUsageMetricRepository, UsageMetricRepository, UsageMetric>();
builder.Services.AddRepository<ILessonSourceRepository, LessonSourceRepository, LessonSource>();
builder.Services.AddRepository<ILessonEntryRepository, LessonEntryRepository, LessonEntry>();
builder.Services.AddRepository<ILessonSourceModifiedRepository, LessonSourceModifiedRepositoryRepository, LessonSourceModified>();
builder.Services.AddRepository<ISelectedLessonSourceRepository, SelectedLessonSourceRepository, SelectedLessonSource>();
builder.Services.AddRepository<ISelectedElectiveLesson, SelectedElectiveLessonRepository, SelectedElectiveLesson>();

builder.Services.AddHttpClient();

// Jobs
builder.Services.AddHostedService<OccurrencesUpdaterJob>();
builder.Services.AddHostedService<UserMetricJob>();
builder.Services.AddHostedService<UserAlertJob>();
builder.Services.AddHostedService<ScheduleParserJob>();
builder.Services.AddHostedService<LessonUpdaterJob>();
builder.Services.AddHostedService<UserLessonUpdaterJob>();

// Configurations
builder.Services.Configure<ElectiveScheduleParsingOptions>(
    builder.Configuration.GetSection("ElectiveScheduleParsing"));
builder.Services.Configure<ScheduleParsingOptions>(
    builder.Configuration.GetSection("GroupScheduleParsing"));

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ScheduleDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ScheduleDBConnection")));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

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
