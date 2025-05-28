using System.ComponentModel.DataAnnotations;
using TaskManager.Models;

namespace TaskManager.DTOs
{
    public class CreateTaskDto
    {
        [Required(ErrorMessage = "Başlık gereklidir")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string Description { get; set; } = string.Empty;

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public TaskCategory Category { get; set; } = TaskCategory.Personal;

        public DateTime? DueDate { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        [Range(0.1, 1000, ErrorMessage = "Tahmini süre 0.1 ile 1000 saat arasında olmalıdır")]
        public double? EstimatedHours { get; set; }
    }

    public class UpdateTaskDto
    {
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
        public string? Title { get; set; }

        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string? Description { get; set; }

        public bool? IsCompleted { get; set; }

        public TaskPriority? Priority { get; set; }

        public TaskCategory? Category { get; set; }

        public DateTime? DueDate { get; set; }

        public List<string>? Tags { get; set; }

        [Range(0.1, 1000, ErrorMessage = "Tahmini süre 0.1 ile 1000 saat arasında olmalıdır")]
        public double? EstimatedHours { get; set; }

        [Range(0.1, 1000, ErrorMessage = "Gerçek süre 0.1 ile 1000 saat arasında olmalıdır")]
        public double? ActualHours { get; set; }
    }

    public class TaskResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public TaskPriority Priority { get; set; }
        public TaskCategory Category { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public double? EstimatedHours { get; set; }
        public double? ActualHours { get; set; }
    }

    public class TaskFilterDto
    {
        public bool? IsCompleted { get; set; }
        public TaskPriority? Priority { get; set; }
        public TaskCategory? Category { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public string? SearchTerm { get; set; }
        public List<string>? Tags { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }

    public class TaskStatsDto
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int OverdueTasks { get; set; }
        public double CompletionRate { get; set; }
        public Dictionary<TaskCategory, int> TasksByCategory { get; set; } = new Dictionary<TaskCategory, int>();
        public Dictionary<TaskPriority, int> TasksByPriority { get; set; } = new Dictionary<TaskPriority, int>();
    }

    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
} 