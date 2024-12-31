using DotNetEnv;
using FluentValidation;
using MassTransit;
using Microsoft.OpenApi.Models;
using OrdersMS.Core.Application.GoogleApiService;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Logger;
using OrdersMS.Core.Infrastructure.Data;
using OrdersMS.Core.Infrastructure.GoogleMaps;
using OrdersMS.Core.Infrastructure.Logger;
using OrdersMS.Core.Infrastructure.UUID;
using OrdersMS.src.Contracts.Application.Commands.CreateContract.Types;
using OrdersMS.src.Contracts.Application.Commands.CreateInsurancePolicy.Types;
using OrdersMS.src.Contracts.Application.Commands.CreateInsuredVehicle.Types;
using OrdersMS.src.Contracts.Application.Commands.UpdateContract.Types;
using OrdersMS.src.Contracts.Application.Commands.UpdateInsuredPolicy.Types;
using OrdersMS.src.Contracts.Application.Commands.UpdateInsuredVehicle.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Infrastructure.Repositories;
using OrdersMS.src.Contracts.Infrastructure.Validators;
using OrdersMS.src.Orders.Application.Commands.AddExtraCost.Types;
using OrdersMS.src.Orders.Application.Commands.CreateOrder.Types;
using OrdersMS.src.Orders.Application.Commands.UpdateDriverAssigned.Types;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatus.Types;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Infrastructure.Repositories;
using OrdersMS.src.Orders.Infrastructure.Validators;
using RestSharp;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddTransient<IValidator<CreateVehicleCommand>, CreateVehicleValidator>();
builder.Services.AddTransient<IValidator<UpdateVehicleCommand>, UpdateVehicleValidator>();
builder.Services.AddTransient<IValidator<CreatePolicyCommand>, CreatePolicyValidator>();
builder.Services.AddTransient<IValidator<UpdatePolicyCommand>, UpdatePolicyValidator>();
builder.Services.AddTransient<IValidator<CreateContractCommand>, CreateContractValidator>();
builder.Services.AddTransient<IValidator<UpdateContractCommand>, UpdateContractValidator>();
builder.Services.AddTransient<IValidator<CreateOrderCommand>, CreateOrderValidator>();
builder.Services.AddTransient<IValidator<AddExtraCostCommand>, AddExtraCostValidator>();
builder.Services.AddTransient<IValidator<UpdateDriverAssignedCommand>, UpdateDriverAssignedValidator>();
builder.Services.AddTransient<IValidator<UpdateOrderStatusCommand>, UpdateOrderStatusValidator>();
builder.Services.AddScoped<IInsuredVehicleRepository, MongoInsuredVehicleRepository>();
builder.Services.AddScoped<IPolicyRepository, MongoInsurancePolicyRepository>();
builder.Services.AddScoped<IContractRepository, MongoContractRepository>();
builder.Services.AddScoped<IOrderRepository, MongoOrderRepository>();
builder.Services.AddScoped<IdGenerator<string>, GuidGenerator>();
builder.Services.AddScoped<ILoggerContract, Logger>();
builder.Services.AddScoped<IGoogleApiService, GoogleApiService>();
builder.Services.AddSingleton<IRestClient>(sp => new RestClient(new RestClientOptions
{
    BaseUrl = new Uri("https://localhost:4052")
}));
builder.Services.AddMassTransit(busConfiguration =>
{
    busConfiguration.SetKebabCaseEndpointNameFormatter();

    busConfiguration.AddConsumers(typeof(Program).Assembly);

    //busConfiguration.AddSagaStateMachine<SagaData, SagaDataImplementation>();

    busConfiguration.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqUri = Environment.GetEnvironmentVariable("RABBITMQ");
        var rabbitMqUsername = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest";
        var rabbitMqPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest";

        cfg.Host(new Uri(rabbitMqUri!), hst =>
        {
            hst.Username(rabbitMqUsername);
            hst.Password(rabbitMqPassword);
        });

        cfg.UseInMemoryOutbox(context);

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API OrdersMicroservice",
        Version = "v1",
        Description = "Endpoints OrdersMicroservice",
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowApiGateway",
        builder => builder.WithOrigins("https://localhost:4050")
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty;
    });

    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/swagger");
            return;
        }
        await next();
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowApiGateway");
app.UseAuthorization();
app.MapControllers();

app.Run();
