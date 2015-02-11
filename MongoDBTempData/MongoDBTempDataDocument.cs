using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDBTempData
{
    class MongoDBTempDataDocument
    {
        [BsonId]
        public Guid UniqueId { get; set; }
        public IEnumerable<MongoDBTempDataValue> Values { get; set; }
    }
}