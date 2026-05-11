using Microsoft.Extensions.DependencyInjection;

namespace KageNoTessen.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }

    private static IServiceCollection AddValidatorsFromAssembly(this IServiceCollection services, System.Reflection.Assembly assembly)
    {
        var validatorTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false }
                && t.BaseType is { IsGenericType: true }
                && t.BaseType.GetGenericTypeDefinition() == typeof(FluentValidation.AbstractValidator<>));

        foreach (var type in validatorTypes)
        {
            var iface = type.GetInterfaces().FirstOrDefault(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(FluentValidation.IValidator<>));
            if (iface is not null)
                services.AddScoped(iface, type);
        }
        return services;
    }
}
