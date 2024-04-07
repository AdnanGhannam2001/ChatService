using System.Reflection;
using ChatService.Attributes;
using MassTransit;

namespace ChatService.Extensions;

internal static class IRabbitMqBusFactoryConfiguratorExtensions {
    public static void ConfigurationReceiveEndpointsFromAssembly(this IRabbitMqBusFactoryConfigurator config,
        Assembly assembly,
        IBusRegistrationContext context)
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