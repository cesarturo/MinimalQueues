using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using MinimalQueues.AzureServiceBus;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;
using NUnit.Framework;
using NUnit.Framework.Internal;

[TestFixture]
[TestFixtureSource(nameof(GetListenerConfigurations))]
public class TestServiceBus : BaseTest
{
    public TestServiceBus(Func<IMessageSender> createMessageSenderDelegate, Func<IHost> createReceiverHostDelegate)
        : base(createMessageSenderDelegate, createReceiverHostDelegate)
    {

    }

    private static IEnumerable<TestFixtureParameters> GetListenerConfigurations()
    {
        yield return GetFixtureParameters(testName: "With Prefetch"
                                        , maxConcurrentCalls: 4
                                        , prefetchCount: 20
                                        , transportType: ServiceBusTransportType.AmqpTcp);

        yield return GetFixtureParameters(testName: "Without Prefetch"
                                        , maxConcurrentCalls: 4
                                        , prefetchCount: 0
                                        , transportType: ServiceBusTransportType.AmqpTcp);
    }

    private static TestFixtureParameters GetFixtureParameters(string testName, int maxConcurrentCalls, int prefetchCount, ServiceBusTransportType transportType)
    {
        var serviceBusNamespace = TestSettings.Get("ServiceBusNamespace");
        var topic               = TestSettings.Get("ServiceBusTopic");
        var subscription        = TestSettings.Get("ServiceBusSubscription");

        var createMessageSenderDelegate = () => new ServiceBusMessageSender(serviceBusNamespace, topic, subscription);

        var createReceiverHostDelegate = () => CreateReceiverHost(maxConcurrentCalls, prefetchCount, topic, subscription, serviceBusNamespace, transportType);

        return new TestFixtureParameters(createMessageSenderDelegate, createReceiverHostDelegate) { TestName = testName };
    }

    private static IHost CreateReceiverHost(int maxConcurrentCalls, int prefetchCount, string topic, string subscription, string serviceBusNamespace, ServiceBusTransportType transportType)
    {
        return ReceiverHostFactory.Create(ConfigureHostToListenServiceBus);


        IOptionsBuilder<QueueProcessorOptions> ConfigureHostToListenServiceBus(IHostBuilder hostBuilder) =>
            hostBuilder.AddAzureServiceBusListener(
                  @namespace:                 serviceBusNamespace
                , entityPath:                 $"{topic}/Subscriptions/{subscription}"
                , serviceBusProcessorOptions: GetServiceBusProcessorOptions(maxConcurrentCalls, prefetchCount)
                , onError:                    OnError
                , serviceBusClientOptions:    new ServiceBusClientOptions { TransportType = transportType });

        static ServiceBusProcessorOptions GetServiceBusProcessorOptions(int maxConcurrentCalls, int prefetchCount)
        {
            return  new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = maxConcurrentCalls,
                PrefetchCount = prefetchCount
            };
        }

        static Task OnError(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.Message);
            return Task.CompletedTask;
        };
    }
}