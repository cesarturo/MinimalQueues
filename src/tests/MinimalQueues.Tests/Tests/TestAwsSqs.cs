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
    private static string QueueUrl => Environment.GetEnvironmentVariable("QUEUE_URL")!;//TestContext.Parameters["QueueUrl"]

    public TestAwsSqs(Func<IHostBuilder, IOptionsBuilder<QueueProcessorOptions>> configureListener)
        :base(configureListener, new SqsMessageSender(QueueUrl), new SqsMessageReceiver())
    {

    }

    private static IEnumerable<TestFixtureParameters> GetListenerConfigurations()
    {
        yield return new TestFixtureParameters(new Func<IHostBuilder, IOptionsBuilder<QueueProcessorOptions>>(
            hostBuilder => hostBuilder.AddAwsSqsListener(
                                              queueUrl: QueueUrl
                                            , prefetchCount: 20
                                            , visibilityTimeout: 5
                                            , renewVisibilityTime: 4
                                            , maxConcurrency: 4)))
        {
            TestName = "With Prefetch (prefecth: 20, concurrency: 4)"
        };

        yield return new TestFixtureParameters(new Func<IHostBuilder, IOptionsBuilder<QueueProcessorOptions>>(
            hostBuilder => hostBuilder.AddAwsSqsListener(
                queueUrl: QueueUrl
                , visibilityTimeout: 5
                , renewVisibilityTime: 4
                , maxConcurrency: 4)))
        {
            TestName = "Without Prefetch (concurrency: 4)"
        };
    }
}