using System;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace IGrainWith3rdPartyLib
{
    public interface IUtilityGrain : Orleans.IGrainWithStringKey
    {
        Task<string> OutputJsonStr(InputDTO input);
        Task<BsonDocument> ToBson(InputDTO input);
    }
}