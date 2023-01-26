# MinimalQueues


## **1<sup>st</sup> step: Configure a Provider**

### Example for **Aws Sqs**

```csharp
var queueApp = hostBuilder.AddQueueProcessorHostedService()
    .ConfigureAwsSqsListener(queueUrl: "...the queue url ...", maxConcurrency: 5)
```
### Example for **Azure Service Bus**
```csharp
var queueApp = hostBuilder.AddQueueProcessorHostedService()    
    .ConfigureAzureServiceBusListener(connectionString: "...ServiceBus connection string..", entityPath: "...the entity path...")
```
---
## **2<sup>nd</sup> step: Provide a Message Handler delegate**

### **Scenario 1:** Queue with Single Message Type

![](images/queue-single-message-type.PNG)

```csharp
queueApp.Use(async (Dto deserializedMessage) =>
{
    //Code to process the message
});

public class Dto//we want to deserialize to this type
{
    //...
}
```
The delegate passed to the `Use` method can receive as a parameter:
- Any dependency (for Scoped dependencies, a new scope is created for every message)
- A parameter with the `Prop` Attribute
- A `CancellationToken` that cancels when the Host is stopping.
- If the parameter is not any of the previous it is assumed that it is the message body parameter and the message will be deserialized to this parameter.

The following example receives the four types of parameters.

```csharp
queueApp.Use(async (Dto deserializedMessage, IInjectedService dependency, [Prop("someHeader")] string headerValue, CancellationToken cancellation) =>
{
    //Code to process the message.
});
```

### **Scenario 2:** Queue with Multiple Message Types
![](images/queue-multiple-message-types.PNG)

The delegate passed to the `When` method can receive any Header or Property, parameter name and type must match the message header name and type.
If the header name does not match the parameter name, use the `Prop` Attribute (as shown in the example below when configuring the blue Handler)

```csharp
queueApp.When((string type)=> type is "green").Use(async (GreenMessage message) =>
{
    //Code to process the message.
});

queueApp.When((string type) => type is "red").Use(async (RedMessage message) =>
{
    //Code to process the message.  
});

queueApp.When(([Prop("type")]string header) => header is "blue").Use(async (BlueMessage message) =>
{
    //Code to process the message.  
});
```
As in the 1<sup>st</sup> Scenario the `Use` method can receive dependencies, headers and the CancellationToken.

---

## **Configure Provider with IConfiguration or any dependency:**
### Example for Aws Sqs:
```csharp
var queueApp = hostBuilder.AddQueueProcessorHostedService()
    .ConfigureAwsSqsListener<IConfiguration>((sqsConfig, configuration) =>
    {//The generic parameter can be anything you want to be injected
        sqsConfig.QueueUrl = configuration["queueUrl"];
        sqsConfig.MaxConcurrentCalls = 5
    })
```

### Example for Azure Service Bus:
```csharp
var queueApp = hostBuilder.AddQueueProcessorHostedService()    
    .ConfigureAzureServiceBusListener<IConfiguration>((serviceBusConfig, configuration) =>
    {//The generic parameter can be anything you want to be injected
        serviceBusConfig.ConnectionString = configuration["sb-connectionString"];
        serviceBusConfig.EntityPath = configuration["entity-path"];
        serviceBusConfig.ServiceBusProcessorOptions = new ServiceBusProcessorOptions {MaxConcurrentCalls = 5};
    })
```