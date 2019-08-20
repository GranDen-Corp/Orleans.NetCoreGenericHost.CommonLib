using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace GrainWith3rdPartyLib
{
    public class UtilityGrain : Orleans.Grain, IGrainWith3rdPartyLib.IUtilityGrain
    {
        private readonly ILogger<UtilityGrain> _logger;

        public UtilityGrain(ILogger<UtilityGrain> logger)
        {
            _logger = logger;
        }

        public Task<string> OutputJsonStr(object input)
        {
            _logger.LogInformation("Invoke OutputJsonStr(), with input={@Input}", input);
            var output = JsonConvert.SerializeObject(input);
            
            return Task.FromResult(output);
        }

        public async Task<BsonDocument> ToBson(object input)
        {
            _logger.LogInformation("Invoke ToBson(), with input={@Input}", input);
            var jsonStr = await OutputJsonStr(input);
            var output = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(jsonStr);
            _logger.LogInformation("RPC result is {@Output}", output);
            return output;
        }
    }
}