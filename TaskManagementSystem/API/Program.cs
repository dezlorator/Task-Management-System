using BusinessLogic.Services;
using FluentValidation.AspNetCore;
using Infrastructure.Database.Repository;
using Microsoft.OpenApi.Models;
using BusinessLogic.Interfaces.Services;
using BusinessLogic.Interfaces.Repositories;
using Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Database.Condition;
using Infrastructure.MessageBroker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskConditions, TaskConditions>();
builder.Services.AddScoped<IMessageProduser, MessageProduser>();

builder.Services.AddHostedService<CreateTaskMessageConsumer>();
builder.Services.AddHostedService<SearchTaskMessageConsumer>();

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task API",
        Version = "v1",
        Description = "API for managing tasks",
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
