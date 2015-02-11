using System;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using Newtonsoft.Json;

namespace MongoDBTempData
{
    /// <summary>
    /// Serializador personalizado para MongoDB
    /// </summary>
    /// <remarks>Basado en https://gist.github.com/lurumad/410e05154cc7e9f2f3eb</remarks>
    class MongoDBCustomSerializer : IBsonSerializer
    {
        #region IBsonSerializer members

        public object Deserialize(MongoDB.Bson.IO.BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
        {
            return Deserialize(bsonReader, nominalType, null, options);
        }

        public object Deserialize(
            MongoDB.Bson.IO.BsonReader bsonReader,
            Type nominalType,
            Type actualType,
            IBsonSerializationOptions options)
        {
            if (bsonReader.GetCurrentBsonType() != BsonType.Document)
            {
                throw new Exception("Not document");
            }

            var bsonDocument = BsonSerializer.Deserialize(bsonReader, typeof(BsonDocument), options) as BsonDocument;
            var json = Regex.Replace(bsonDocument.ToJson(), @"ObjectId\((.[a-f0-9]{24}.)\)", (m) => m.Groups[1].Value);
            json = GetFixedDeserializedJson(json);
            return JsonConvert.DeserializeObject<object>(json, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
        }

        public IBsonSerializationOptions GetDefaultSerializationOptions()
        {
            return new DocumentSerializationOptions();
        }

        public void Serialize(MongoDB.Bson.IO.BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
        {
            var json = (value == null) ? "{}" : JsonConvert.SerializeObject(value, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
            json = GetFixedSerializedJson(json);
            BsonDocument document = BsonDocument.Parse(json);
            BsonSerializer.Serialize(bsonWriter, typeof(BsonDocument), document, options);
        }

        #endregion

        private string GetFixedDeserializedJson(string json)
        {
            string pattern;
            if (Regex.IsMatch(json, @"""_type"""))
            {
                // No intentar deserializar a un tipo anónimo
                pattern = @"""_type""\s*:\s*""<>f__AnonymousType[\s\S]+?""\s*,{0,1}";
                json = Regex.Replace(json, pattern, "");
                // Restaurar $type para que la deserialización de Json.NET funcione
                json = json.Replace("_type", "$type");
            }
            // Si la deserialización es de un tipo simple, devolver sólo el valor
            pattern = @"^\{\s*""_value""\s*:\s*\""{0,1}"; // { "_value" : "
            var start = Regex.Match(json, pattern);
            if (start.Success)
            {
                pattern = @"""{0,1}\s*\}$"; // " }
                var end = Regex.Match(json, pattern);
                // No todos los tipos simples van entre comillas dobles
                var doubleQuote = end.Value.StartsWith("\"");
                var value = json.Substring(start.Length, end.Index - start.Length);
                json = string.Format("{0}{1}{2}", doubleQuote ? "\"" : "", value, doubleQuote ? "\"" : "");
            }
            return json;
        }

        private string GetFixedSerializedJson(string json)
        {
            // https://groups.google.com/forum/#!topic/mongodb-user/CDUTFoFF4FU
            // En MongoDB un nombre de propiedad no puede empezar por $ ni por .
            json = json.Replace("$type", "_type");
            if (!json.StartsWith("{"))
            {
                // Si la serialización es un tipo simple, añadir nombre de propiedad para que no falle BsonDocument.Parse
                json = string.Format("{{\"_value\":{0}}}", json);
            }
            return json;
        }
    }
}