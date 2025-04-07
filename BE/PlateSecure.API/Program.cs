using System.Text.Json;
using System.Text.Json.Serialization;
using PlateSecure.Application.DTOs;
using PlateSecure.Infrastructure.Configuration;
using PlateSecure.Infrastructure.Persistence;
using PlateSecure.Infrastructure.Settings;

using JsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

// 1. Swagger / OpenAPI cho Minimal API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PlateSecure API", Version = "v1" });
    c.OperationFilter<FileUploadOperationFilter>();
});

// 2. MongoDB + DI
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));
builder.Services.AddSingleton<MongoDbContext>();

builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// 3. Chỉ bật Swagger UI khi dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.MapControllers();

app.Run();