using Amazon.Lambda.RuntimeSupport;
using Microsoft.Extensions.Hosting;

namespace MinimalQueues.AwsLambdaSqs;

internal sealed class LambdaBootstrapHostedService : IHostedService
{
    private readonly IHostApplicationLifetime                          _appLifetime;
    private readonly LambdaSqsEventProcessor                                  _lambdaSqsEventProcessor;
    private readonly IServiceProvider                                  _serviceProvider;
    private readonly Action<LambdaBootstrapBuilder, IServiceProvider>? _configureBootstraper;
    private Task?                                                      _bootstrapperTask;
    public LambdaBootstrapHostedService(IHostApplicationLifetime appLifetime
                                      , LambdaSqsEventProcessor lambdaSqsEventProcessor
                                      , IServiceProvider serviceProvider
                                      , Action<LambdaBootstrapBuilder, IServiceProvider>? configureBootstraper)
    {
        _appLifetime          = appLifetime;
        _lambdaSqsEventProcessor     = lambdaSqsEventProcessor;
        _serviceProvider      = serviceProvider;
        _configureBootstraper = configureBootstraper;
    }
    private void StartLambda()
    {
        var bootstrapBuilder = LambdaBootstrapBuilder.Create(_lambdaSqsEventProcessor.FunctionHandler);
        _configureBootstraper?.Invoke(bootstrapBuilder, _serviceProvider);
        _bootstrapperTask = bootstrapBuilder
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