using MongoDB.Driver;
using Interfracture.Interfaces;
using Infrastructure.Base;
using Microsoft.Extensions.Options;

namespace Repositories.Repositories {
    public class MongoDbUnitOfWork: IMongoDbUnitOfWork {
        private readonly IMongoDatabase _database;

        public MongoDbUnitOfWork(IMongoClient client, IOptions<MongoDBSettings> settings) {
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName) {
            return _database.GetCollection<T>(collectionName);
        }
    }
}