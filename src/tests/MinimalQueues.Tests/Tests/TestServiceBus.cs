using Azure.Messaging.ServiceBus;
using MinimalQueues.AzureServiceBus;
using NUnit.Framework;
using NUnit.Framework.Internal;

[TestFixture]
[TestFixtureSource(nameof(GetListenerConfigurations))]
public class TestServiceBus : BaseTest
{
    public TestServiceBus(IMessageSender sender, MessageReceiver receiver) : base(sender, receiver)
    {
    }

    private static IEnumerable<TestFixtureParameters> GetListenerConfigurations()
    {
        var connectionString = TestSettings.Get("ServiceBusConnectionString");
        var topic = TestSettings.Get("ServiceBusTopic");
        var subscription = TestSettings.Get("ServiceBusSubscription");

        yield return new TestFixtureParameters(new ServiceBusMessageSender(connectionString, topic, subscription), new MessageReceiver(
            hostbuilder => hostbuilder.AddAzureServiceBusListener(connectionString: connectionString, entityPath: $"{topic}/Subscriptions/{subscription}"
                , serviceBusProcessorOptions: new ServiceBusProcessorOptions
                {
                    MaxConcurrentCalls = 4,
                    PrefetchCount = 20
                }
                , onError: args =>
                {
                    Console.WriteLine(args.Exception.Message);
                    return Task.CompletedTask;
                }
                , serviceBusClientOptions: new ServiceBusClientOptions
                {
                    TransportType = ServiceBusTransportType.AmqpTcp
                })))
        {
            TestName = "With Prefetch"
        };

        yield return new TestFixtureParameters(new ServiceBusMessageSender(connectionString, topic, subscription), new MessageReceiver(
            hostBuilder => hostBuilder.AddAzureServiceBusListener(connectionString: connectionString, entityPath: $"{topic}/Subscriptions/{subscription}"
                , serviceBusProcessorOptions: new ServiceBusProcessorOptions
                    {
                        MaxConcurrentCalls = 4,
                    }
                , onError: args =>
                {
                    Console.WriteLine(args);
                    Console.WriteLine(args.Exception.Message);
                    return Task.CompletedTask;
                }
                , serviceBusClientOptions: new ServiceBusClientOptions
                {
                    TransportType = ServiceBusTransportType.AmqpTcp
                })))
        {
            TestName = "Without Prefetch"
        };
    }
}