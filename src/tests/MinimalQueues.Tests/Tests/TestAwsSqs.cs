using System.Collections.Concurrent;
using System.Text.Json;
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
    
    public TestAwsSqs(IMessageSender sender, MessageReceiver receiver) 
        : base(sender, receiver)
    {

    }

    private static IEnumerable<TestFixtureParameters> GetListenerConfigurations()
    {
        string queueUrl = TestSettings.Get("QUEUE_URL")!;//TestContext.Parameters["QueueUrl"]
        //It is possible to use TestParameters instead of Environment variables like this: TestContext.Parameters["ParameterName"]
        //Test parameters can be configured in the .runsettings file and in the dotnet test command (https://github.com/dotnet/dotnet/blob/3a29b786732fe6f22627567b62692855055189ea/src/vstest/docs/RunSettingsArguments.md)

        yield return GetFixtureParameters("With Prefetch (prefecth: 20, concurrency: 4, visibilityTimeout: 5, renewVisibilityTime: 4)"
            , queueUrl
            , hostBuilder => hostBuilder.AddAwsSqsListener(queueUrl: queueUrl
                                                         , prefetchCount: 20
                                                         , visibilityTimeout: 5
                                                         , renewVisibilityTime: 4
                                                         , maxConcurrency: 4));

        yield return GetFixtureParameters("Without Prefetch (concurrency: 4, visibilityTimeout: 5, renewVisibilityTime: 4)"
            , queueUrl
            , hostBuilder => hostBuilder.AddAwsSqsListener(queueUrl: queueUrl
                                                         , visibilityTimeout: 5
                                                         , renewVisibilityTime: 4
                                                         , maxConcurrency: 4));
    }

    private static TestFixtureParameters GetFixtureParameters(string testName, string queueUrl, Func<IHostBuilder, IOptionsBuilder<QueueProcessorOptions>> configureListener)
    {
        return new TestFixtureParameters(new SqsMessageSender(queueUrl), new MessageReceiver(configureListener))
        {
            TestName = testName
        };
    }
}