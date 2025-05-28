using MongoDB.Driver;
using TaskManager.Models;

namespace TaskManager.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoDB");
            var mongoClient = new MongoClient(connectionString);
            _database = mongoClient.GetDatabase("task-manager");
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("user");
        public IMongoCollection<TaskItem> Tasks => _database.GetCollection<TaskItem>("taskitem");
    }
} 