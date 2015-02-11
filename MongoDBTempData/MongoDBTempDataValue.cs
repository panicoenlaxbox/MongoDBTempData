using MongoDB.Bson.Serialization.Attributes;

namespace MongoDBTempData
{
    class MongoDBTempDataValue
    {
        [BsonSerializer(typeof(MongoDBCustomSerializer))]
        public object Value { get; set; }
        public string Key { get; set; }
    }
}