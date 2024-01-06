using Microsoft.Extensions.Hosting;
using MinimalQueues.AwsSqs;
using MinimalQueues.Core;
using MinimalQueues.Core.Options;
using NUnit.Framework;
using NUnit.Framework.Internal;


[TestFixture]
[TestFixtureSource(nameof(GetListenerConfigurations))]
public class TestAwsSqs : BaseTest
{
    
    public TestAwsSqs(Func<IMessageSender> createMessageSenderDelegate, Func<IHost> createReceiverHostDelegate) 
        : base(createMessageSenderDelegate, createReceiverHostDelegate)
    {

    }

    private static IEnumerable<TestFixtureParameters> GetListenerConfigurations()
    {
        string queueUrl = TestSettings.Get("QUEUE_URL")!;//TestContext.Parameters["QueueUrl"]
        //It is possible to use TestParameters instead of Environment variables like this: TestContext.Parameters["ParameterName"]
        //Test parameters can be configured in the .runsettings file and in the dotnet test command (https://github.com/dotnet/dotnet/blob/3a29b786732fe6f22627567b62692855055189ea/src/vstest/docs/RunSettingsArguments.md)

        yield return GetFixtureParameters(testName: "With Prefetch (prefecth: 20, concurrency: 4, visibilityTimeout: 5, renewVisibilityTime: 4)"
                                        , queueUrl: queueUrl
                                        , maxConcurrency: 4
                                        , prefetchCount: 20
                                        , visibilityTimeout: 5
                                        , renewVisibilityTime: 4);

        yield return GetFixtureParameters(testName: "Without Prefetch (concurrency: 4, visibilityTimeout: 5, renewVisibilityTime: 4)"
                                        , queueUrl: queueUrl
                                        , maxConcurrency: 4
                                        , prefetchCount: 0
                                        , visibilityTimeout: 5
                                        , renewVisibilityTime: 4);
    }

    private static TestFixtureParameters GetFixtureParameters(string testName, string queueUrl, int maxConcurrency, int prefetchCount, int visibilityTimeout, int renewVisibilityTime)
    {
        var createMessageSenderDelegate = () => new SqsMessageSender(queueUrl);

        var createReceiverHostDelegate = () => CreateReceiverHost(queueUrl, maxConcurrency, prefetchCount, visibilityTimeout, renewVisibilityTime);

        return new TestFixtureParameters(createMessageSenderDelegate, createReceiverHostDelegate) { TestName = testName };
    }

    private static IHost CreateReceiverHost(string queueUrl, int maxConcurrency, int prefetchCount, int visibilityTimeout, int renewVisibilityTime)
    {
        return ReceiverHostFactory.Create(ConfigureHostToListenSqs);


        IOptionsBuilder<QueueProcessorOptions> ConfigureHostToListenSqs(IHostBuilder hostBuilder) => 
            hostBuilder.AddAwsSqsListener(queueUrl:            queueUrl
                                        , maxConcurrency:      maxConcurrency
                                        , prefetchCount:       prefetchCount
                                        , visibilityTimeout:   visibilityTimeout
                                        , renewVisibilityTime: renewVisibilityTime);
    }
}