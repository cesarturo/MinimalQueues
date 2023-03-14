using System.Text.Json;

namespace MinimalQueues.AwsLambdaSqs;

internal static class ResponseStreamBuilder
{//this class builds a stream based on: https://docs.aws.amazon.com/lambda/latest/dg/with-sqs.html#services-sqs-batchfailurereporting
 //The same result can be achieved by serializing the SQSEvent class from Amazon.Lambda.SQSEvents

    public static Stream Build(string?[] messageIdOfErrors)
    {
        MemoryStream? stream = null;
        Utf8JsonWriter? writer = null;
        for (int i = 0; i < messageIdOfErrors.Length; i++)
        {
            var result = messageIdOfErrors[i];
            if (result is null) continue;
            if (stream is null)
            {
                stream = new MemoryStream();
                writer = new Utf8JsonWriter(stream, new JsonWriterOptions { SkipValidation = true });
                writer.WriteStartObject();
                writer.WritePropertyName("batchItemFailures");
                writer.WriteStartArray();
            }
            writer!.WriteStartObject();
            writer.WriteString("itemIdentifier", result);
            writer.WriteEndObject();
        }

        if (stream is null) return new MemoryStream();

        writer!.WriteEndArray();
        writer.WriteEndObject();
        writer.Dispose();
        stream.Position = 0;
        return stream;
    }
}