using DotNetEnv;
using FluentValidation;
using Microsoft.OpenApi.Models;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Logger;
using OrdersMS.Core.Infrastructure.Data;
using OrdersMS.Core.Infrastructure.Logger;
using OrdersMS.Core.Infrastructure.UUID;
using OrdersMS.src.InsuredVehicles.Application.Commands.CreateInsuredVehicle.Types;
using OrdersMS.src.InsuredVehicles.Application.Commands.UpdateInsuredVehicle.Types;
using OrdersMS.src.InsuredVehicles.Application.Repositories;
using OrdersMS.src.InsuredVehicles.Infrastructure.Repositories;
using OrdersMS.src.InsuredVehicles.Infrastructure.Validators;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddTransient<IValidator<CreateVehicleCommand>, CreateVehicleValidator>();
builder.Services.AddTransient<IValidator<UpdateVehicleCommand>, UpdateVehicleValidator>();
builder.Services.AddScoped<IInsuredVehicleRepository, MongoInsuredVehicleRepository>();
builder.Services.AddScoped<IdGenerator<string>, GuidGenerator>();
builder.Services.AddScoped<ILoggerContract, Logger>();
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
