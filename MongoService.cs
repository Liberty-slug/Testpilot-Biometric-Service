using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiometricService
{
    public class MongoService
    {
        private readonly IMongoDatabase _database;

        public MongoService(string connectionString, string dbName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(dbName);
        }

        public async Task<List<Student>> GetStudentsWithFingerprintsAsync()
        {
            var collection = _database.GetCollection<Student>("students");

            // Only students who have fingerprints.f1/f6 (or legacy f$1/f$6)
            var filter = Builders<Student>.Filter.Or(
                Builders<Student>.Filter.Exists("fingerprints.f1", true),
                Builders<Student>.Filter.Exists("fingerprints.f6", true),
                Builders<Student>.Filter.Exists("fingerprints.f$1", true),
                Builders<Student>.Filter.Exists("fingerprints.f$6", true)
            );

            var projection = Builders<Student>.Projection
                .Include("_id")
                .Include("fingerprints.f1")
                .Include("fingerprints.f6")
                .Include("fingerprints.f$1")
                .Include("fingerprints.f$6");

            return await collection.Find(filter).Project<Student>(projection).ToListAsync();
        }
    }
}
