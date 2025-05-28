using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskManager.Models
{
    public class TaskItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("isCompleted")]
        public bool IsCompleted { get; set; } = false;

        [BsonElement("priority")]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        [BsonElement("category")]
        public TaskCategory Category { get; set; } = TaskCategory.Personal;

        [BsonElement("dueDate")]
        public DateTime? DueDate { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("completedAt")]
        public DateTime? CompletedAt { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        [BsonElement("estimatedHours")]
        public double? EstimatedHours { get; set; }

        [BsonElement("actualHours")]
        public double? ActualHours { get; set; }
    }

    public enum TaskPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public enum TaskCategory
    {
        Personal = 1,
        Work = 2,
        Health = 3,
        Education = 4,
        Finance = 5,
        Shopping = 6,
        Travel = 7,
        Other = 8
    }
} 