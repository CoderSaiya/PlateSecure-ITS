using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlateSecure.Application.Interfaces;
using PlateSecure.Application.Services;
using PlateSecure.Domain.Interfaces;
using PlateSecure.Infrastructure.Repositories;

namespace PlateSecure.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDetectionLogRepository, DetectionLogRepository>();
        services.AddScoped<IParkingEventRepository, ParkingEventRepository>();

        services.AddScoped<IDetectionService, DetectionService>();
        
        return services;
    }
}