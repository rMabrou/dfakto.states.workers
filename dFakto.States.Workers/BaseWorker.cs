using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using dFakto.States.Workers.Interfaces;

namespace dFakto.States.Workers
{
    // class JsonConverterJsonElement : JsonConverter<JsonElement>
    // {
    //     public override JsonElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //     {
    //         using (JsonDocument document = JsonDocument.ParseValue(ref reader))
    //         {
    //             return document.RootElement.Clone();
    //         }
    //     }
    //
    //     public override void Write(Utf8JsonWriter writer, JsonElement value, JsonSerializerOptions options)
    //     {
    //         if (value.ValueKind == JsonValueKind.Undefined)
    //         {
    //             writer.WriteNullValue();
    //         }
    //         else
    //         {
    //             value.WriteTo(writer);
    //         }
    //     }
    // }
    
    public abstract class BaseWorker<TI,TO> : IWorker
    {
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            Converters = {new JsonStringEnumConverter()},
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreNullValues = true,
            PropertyNameCaseInsensitive = true
        };
        
        public string ActivityName { get; }
        public TimeSpan HeartbeatDelay { get; }
        public int MaxConcurrency { get; }
        
        protected BaseWorker(string activityName)
            : this(activityName, TimeSpan.MaxValue)
        {

        }

        protected BaseWorker(string activityName, TimeSpan heartbeatDelay, int maxConcurrentExecutions = 5)
        {
            ActivityName = activityName;
            HeartbeatDelay = heartbeatDelay;
            MaxConcurrency = maxConcurrentExecutions;
        }
        
        public async Task<string> DoRawJsonWorkAsync(string input, CancellationToken token)
        {
            var result = await DoWorkAsync(JsonSerializer.Deserialize<TI>(input,_options),token);
            return JsonSerializer.Serialize(result,_options);
        }

        public abstract Task<TO> DoWorkAsync(TI input, CancellationToken token);
    }
}