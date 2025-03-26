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
using RabbitMQ.Client;
using Domain.Constants;
using BusinessLogic.Services.ConsumerHandlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskConditions, TaskConditions>();
builder.Services.AddScoped<IConsumerHandler, CreateTaskHandler>();
builder.Services.AddScoped<IConsumerHandler, SearchTaskHandler>();
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory
    {
        HostName = "localhost",
        UserName = "guest",
        Password = "guest"
    };

    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

builder.Services.AddScoped<IChannel>(sp =>
{
    var connection = sp.GetRequiredService<IConnection>();
    return connection.CreateChannelAsync().GetAwaiter().GetResult();
});

builder.Services.AddScoped<IMessageProduser>((sp)  =>
{
    var logger = sp.GetService<ILogger<MessageProduser>>()!;
    var channel = sp.GetService<IChannel>()!;
    return MessageProduser.Create(logger, channel).GetAwaiter().GetResult();
});

builder.Services.AddSingleton(sp => new Dictionary<string, IConsumerHandler>
{
    { BrokerConfigurations.QueueNames.CreateTaskQueue, sp.GetRequiredService<CreateTaskHandler>() },
    { BrokerConfigurations.QueueNames.SearchTaskQueue, sp.GetRequiredService<SearchTaskHandler>() }
});

builder.Services.AddHostedService<MessageConsumer>();

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
