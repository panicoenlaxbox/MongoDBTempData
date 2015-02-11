using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace MongoDBTempData
{
    public class MongoDBTempDataProvider : ITempDataProvider
    {
        private readonly string _collectionName;
        private readonly string _connectionString;
        private readonly string _databaseName;
        private readonly GetUniqueId _getUniqueId;

        public MongoDBTempDataProvider(string connectionString, string databaseName, string collectionName, GetUniqueId getUniqueId)
        {
            _connectionString = connectionString;
            _databaseName = databaseName;
            _collectionName = collectionName;
            _getUniqueId = getUniqueId;
        }

        #region ITempDataProvider members

        public IDictionary<string, object> LoadTempData(ControllerContext controllerContext)
        {
            var document = GetCollection().FindOne(GetQuery(controllerContext));
            if (document == null)
            {
                return null;
            }
            return document.Values.ToDictionary(value => value.Key, value => value.Value);
        }

        public void SaveTempData(ControllerContext controllerContext, IDictionary<string, object> values)
        {
            var mustPersist = values != null && values.Any();
            if (mustPersist)
            {
                var document = new MongoDBTempDataDocument
                {
                    UniqueId = _getUniqueId(controllerContext),
                    Values = values.Select(p => new MongoDBTempDataValue
                    {
                        Key = p.Key,
                        Value = p.Value
                    })
                };
                GetCollection().Update(GetQuery(controllerContext), Update.Replace(document), UpdateFlags.Upsert);
            }
            else
            {
                GetCollection().Remove(GetQuery(controllerContext));
            }
        }

        #endregion

        private MongoCollection<MongoDBTempDataDocument> GetCollection()
        {
            return GetDatabase().GetCollection<MongoDBTempDataDocument>(_collectionName);
        }

        private MongoDatabase GetDatabase()
        {
            var url = new MongoUrl(_connectionString);
            var client = new MongoClient(url);
            var server = client.GetServer();
            return server.GetDatabase(_databaseName);
        }

        private IMongoQuery GetQuery(ControllerContext controllerContext)
        {
            return Query<MongoDBTempDataDocument>.EQ(p => p.UniqueId, _getUniqueId(controllerContext));
        }
    }
}