using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.Hosting;

namespace MinimalQueues.AwsLambdaSqs;

internal sealed class LambdaEventListenerHostedService : IHostedService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly MessageProcessor _messageProcessor;
    private Task? _bootstrapperTask;
    public LambdaEventListenerHostedService(IHostApplicationLifetime appLifetime, MessageProcessor messageProcessor)
    {
        _appLifetime = appLifetime;
        _messageProcessor = messageProcessor;
    }
    private void StartLambda()
    {
        _bootstrapperTask = LambdaBootstrapBuilder
            .Create<SQSEvent>(_messageProcessor.FunctionHandler, new DefaultLambdaJsonSerializer())
            .Build()
            .RunAsync(_appLifetime.ApplicationStopping);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _appLifetime.ApplicationStarted.Register(StartLambda);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _bootstrapperTask!;
    }
}