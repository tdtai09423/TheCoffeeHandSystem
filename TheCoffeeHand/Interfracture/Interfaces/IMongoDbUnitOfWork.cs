using MongoDB.Driver;

namespace Interfracture.Interfaces {
    public interface IMongoDbUnitOfWork {
        IMongoCollection<T> GetCollection<T>(string collectionName);
    }
}
