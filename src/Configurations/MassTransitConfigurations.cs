using System.Reflection;
using MassTransit;
using MassTransit.Configuration;

namespace ChatService.Configurations;

internal static class MassTransitConfigurations {
    public static void RegisterConsumersFromAssembly(Assembly assembly, IBusRegistrationConfigurator config) {
        var method = typeof(DependencyInjectionConsumerRegistrationExtensions)
            .GetMethods()
            .First(m => m.Name == nameof(DependencyInjectionConsumerRegistrationExtensions.RegisterConsumer));

        foreach (var type in assembly.GetTypes()) {
            if (type.IsAssignableTo(typeof(IConsumer)) && !type.IsInterface && !type.IsAbstract) {
                method.MakeGenericMethod(type).Invoke(null, [config]);
            }
        }
    }
}