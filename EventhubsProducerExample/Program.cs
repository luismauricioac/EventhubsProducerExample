
using Azure.Messaging.EventHubs.Producer;
using System.Text.Json;


await StartEventGenerating();
async Task StartEventGenerating()
{
    var connectionString = "YOUR_EVENT_HUB_CONNECTION_STRING";
    var eventHubName = "YOUR_EVENT_HUB_NAME";

    await using (var producer = new EventHubProducerClient(connectionString, eventHubName))
    {
        while (true)
        {
            var eventBatch = await producer.CreateBatchAsync();
            var events = EventGenerator.GetSensorEvents(Sensors.DoorSensor, 1000);
            foreach (var sensorEvent in events)
            {
                var isSuccessfullyAdded = eventBatch.TryAdd(new Azure.Messaging.EventHubs.EventData(JsonSerializer.Serialize(sensorEvent)));
                if (!isSuccessfullyAdded)
                {
                    if (eventBatch.Count > 0)
                    {
                        await producer.SendAsync(eventBatch);
                        Console.WriteLine($"Batch data sent with total batch amount of {eventBatch.Count}");
                        eventBatch = await producer.CreateBatchAsync();
                    }
                }
                else
                {
                    // log event that failed
                }
            }

            if (eventBatch.Count > 0)
            {
                await producer.SendAsync(eventBatch);
            }
            eventBatch.Dispose();

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}