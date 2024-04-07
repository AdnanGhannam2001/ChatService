using System.Reflection;
using ChatService.Attributes;
using ChatService.Extensions;
using MassTransit;

namespace ChatService.Configurations;

internal static class RabbitMQConfigurations {
    public static void ConfigurationReceiveEndpointsFromAssembly(Assembly assembly,
        IBusRegistrationContext context,
        IRabbitMqBusFactoryConfigurator config)
    {
        foreach (var klass in GetAssemblyTypesWithAttribute<QueueConsumerAttribute>(assembly)) {
            var queueAttribute = klass.GetCustomAttribute<QueueConsumerAttribute>()!;
            var queueName = queueAttribute.Name ?? klass.Name.ToKebabCase().Replace("-consumer", "");

            config.ReceiveEndpoint(queueName, e => e.ConfigureConsumer(context, klass));
        }
    }

    private static IEnumerable<Type> GetAssemblyTypesWithAttribute<T>(Assembly assembly)
        where T : Attribute
    {
        foreach (var type in assembly.GetTypes()) {
            if (type.GetCustomAttributes<T>().Any()) {
                yield return type;
            }
        }
    }
}