namespace MinimalQueues.AwsLambdaSqs;

public sealed class SQSEvent
{//This class is based on Amazon.Lambda.SQSEvents nuget classes but it only contains the properties I need
    public List<SQSEvent.SQSMessage> Records { get; set; }

    public sealed class MessageAttribute
    {
        public string StringValue { get; set; }
        public string DataType { get; set; }
    }
    public sealed class SQSMessage
    {
        public string MessageId { get; set; }
        public string Body { get; set; }

        public string EventSourceArn { get; set; }

        public Dictionary<string, SQSEvent.MessageAttribute> MessageAttributes { get; set; }
    }
}