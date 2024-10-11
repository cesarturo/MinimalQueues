using Amazon.Lambda.Core;

namespace MinimalQueues.AwsLambdaSqs;

public delegate Task<Stream> LambdaSqsEventHandler(Stream stream, ILambdaContext context);