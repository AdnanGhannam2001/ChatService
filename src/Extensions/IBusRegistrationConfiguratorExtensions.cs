using System.Reflection;
using MassTransit;
using MassTransit.Configuration;

namespace ChatService.Extensions;

internal static class IBusRegistrationConfiguratorExtensions {
    public static void RegisterConsumersFromAssembly(this IBusRegistrationConfigurator config, Assembly assembly) {
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