using DotNetEnv;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using OrdersMS.Core.Application.Firebase;
using OrdersMS.Core.Application.GoogleApiService;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Logger;
using OrdersMS.Core.Infrastructure.Data;
using OrdersMS.Core.Infrastructure.Firebase;
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
using OrdersMS.src.Orders.Application.Commands.AddExtraCost;
using OrdersMS.src.Orders.Application.Commands.CreateExtraCost.Types;
using OrdersMS.src.Orders.Application.Commands.CreateOrder.Types;
using OrdersMS.src.Orders.Application.Commands.UpdateDriverAssigned.Types;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatus.Types;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatusToCompleted.Types;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatusToPaid.Types;
using OrdersMS.src.Orders.Application.Commands.UpdateTotalAmountOrder.Types;
using OrdersMS.src.Orders.Application.Commands.ValidateLocationDriverToIncidecident.Types;
using OrdersMS.src.Orders.Application.Commands.ValidatePricesOfExtrasCost.Types;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.SagaData;
using OrdersMS.src.Orders.Domain.Services;
using OrdersMS.src.Orders.Infrastructure.Repositories;
using OrdersMS.src.Orders.Infrastructure.StateMachine;
using OrdersMS.src.Orders.Infrastructure.Validators;
using RestSharp;
using System.Text;

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
builder.Services.AddTransient<IValidator<ValidatePricesOfExtrasCostCommand>, ValidatePricesOfExtrasCostValidator>();
builder.Services.AddTransient<IValidator<UpdateDriverAssignedCommand>, UpdateDriverAssignedValidator>();
builder.Services.AddTransient<IValidator<UpdateOrderStatusCommand>, UpdateOrderStatusValidator>();
builder.Services.AddTransient<IValidator<ValidateLocationCommand>, ValidateLocationDriverValidator>();
builder.Services.AddTransient<IValidator<UpdateTotalAmountOrderCommand>, UpdateTotalAmountOrderValidator>();
builder.Services.AddTransient<IValidator<UpdateOrderStatusToCompletedCommand>, UpdateOrderStatusToCompletedValidator>();
builder.Services.AddTransient<IValidator<UpdateOrderStatusToPaidCommand>, UpdateOrderStatusToPaidValidator>();
builder.Services.AddTransient<IValidator<CreateExtraCostCommand>, CreateExtraCostValidator>();
builder.Services.AddScoped<IInsuredVehicleRepository, MongoInsuredVehicleRepository>();
builder.Services.AddScoped<IPolicyRepository, MongoInsurancePolicyRepository>();
builder.Services.AddScoped<IContractRepository, MongoContractRepository>();
builder.Services.AddScoped<IOrderRepository, MongoOrderRepository>();
builder.Services.AddScoped<IExtraCostRepository, MongoExtraCostRepository>();
builder.Services.AddScoped<CalculateOrderTotalAmount>();
builder.Services.AddScoped<AddExtraCostCommandHandler>();
builder.Services.AddScoped<IdGenerator<string>, GuidGenerator>();
builder.Services.AddScoped<ILoggerContract, Logger>();
builder.Services.AddScoped<IGoogleApiService, GoogleApiService>();
builder.Services.AddSingleton<IRestClient>(sp => new RestClient());
//builder.Services.Configure<FirebaseMessagingSettings>(options =>
//{
//    options.ApiKey = Environment.GetEnvironmentVariable("FIREBASE_API_KEY");
//    options.SenderId = Environment.GetEnvironmentVariable("FIREBASE_SENDER_ID");
//    options.ProjectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");
//});
//builder.Services.AddSingleton<IFirebaseMessagingService, FirebaseMessagingService>();
//builder.Services.AddSingleton<IFirebaseMessagingClient, FirebaseMessagingClient>();
//builder.Services.AddSingleton<IFirebaseAppClient, FirebaseAppClient>();
builder.Services.AddMassTransit(busConfiguration =>
{
    busConfiguration.SetKebabCaseEndpointNameFormatter();

    busConfiguration.AddConsumers(typeof(Program).Assembly);

    busConfiguration.AddSagaStateMachine<OrderStatusStateMachine, OrderStatusSagaData>()
        .MongoDbRepository(r =>
        {
            r.Connection = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
            r.DatabaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME_SAGA");
            r.CollectionName = "events";
        });

    BsonClassMap.RegisterClassMap<OrderStatusSagaData>(cm =>
    {
        cm.AutoMap();
        cm.MapIdProperty(x => x.CorrelationId)
            .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
    });

    busConfiguration.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqUri = Environment.GetEnvironmentVariable("RABBITMQ");
        var rabbitMqUsername = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME");
        var rabbitMqPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");

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
        Description = "Endpoints de OrdersMicroservice",
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT en el formato: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
        ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY")!))
    };
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
