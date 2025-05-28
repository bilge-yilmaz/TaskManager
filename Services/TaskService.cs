using MongoDB.Driver;
using TaskManager.Data;
using TaskManager.DTOs;
using TaskManager.Models;

namespace TaskManager.Services
{
    public class TaskService : ITaskService
    {
        private readonly MongoDbContext _context;

        public TaskService(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<TaskResponseDto> CreateTaskAsync(string userId, CreateTaskDto createTaskDto)
        {
            var task = new TaskItem
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                Priority = createTaskDto.Priority,
                Category = createTaskDto.Category,
                DueDate = createTaskDto.DueDate,
                Tags = createTaskDto.Tags,
                EstimatedHours = createTaskDto.EstimatedHours,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Tasks.InsertOneAsync(task);
            return MapToResponseDto(task);
        }

        public async Task<TaskResponseDto?> GetTaskByIdAsync(string userId, string taskId)
        {
            var task = await _context.Tasks
                .Find(t => t.Id == taskId && t.UserId == userId)
                .FirstOrDefaultAsync();

            return task != null ? MapToResponseDto(task) : null;
        }

        public async Task<PagedResultDto<TaskResponseDto>> GetTasksAsync(string userId, TaskFilterDto filter)
        {
            var filterBuilder = Builders<TaskItem>.Filter;
            var filters = new List<FilterDefinition<TaskItem>>
            {
                filterBuilder.Eq(t => t.UserId, userId)
            };

            // Filtreleri uygula
            if (filter.IsCompleted.HasValue)
                filters.Add(filterBuilder.Eq(t => t.IsCompleted, filter.IsCompleted.Value));

            if (filter.Priority.HasValue)
                filters.Add(filterBuilder.Eq(t => t.Priority, filter.Priority.Value));

            if (filter.Category.HasValue)
                filters.Add(filterBuilder.Eq(t => t.Category, filter.Category.Value));

            if (filter.DueDateFrom.HasValue)
                filters.Add(filterBuilder.Gte(t => t.DueDate, filter.DueDateFrom.Value));

            if (filter.DueDateTo.HasValue)
                filters.Add(filterBuilder.Lte(t => t.DueDate, filter.DueDateTo.Value));

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchFilter = filterBuilder.Or(
                    filterBuilder.Regex(t => t.Title, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i")),
                    filterBuilder.Regex(t => t.Description, new MongoDB.Bson.BsonRegularExpression(filter.SearchTerm, "i"))
                );
                filters.Add(searchFilter);
            }

            if (filter.Tags != null && filter.Tags.Any())
                filters.Add(filterBuilder.AnyIn(t => t.Tags, filter.Tags));

            var combinedFilter = filterBuilder.And(filters);

            // Sıralama
            var sortDefinition = GetSortDefinition(filter.SortBy, filter.SortDescending);

            // Toplam sayı
            var totalCount = await _context.Tasks.CountDocumentsAsync(combinedFilter);

            // Sayfalama
            var skip = (filter.Page - 1) * filter.PageSize;
            var tasks = await _context.Tasks
                .Find(combinedFilter)
                .Sort(sortDefinition)
                .Skip(skip)
                .Limit(filter.PageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

            return new PagedResultDto<TaskResponseDto>
            {
                Items = tasks.Select(MapToResponseDto).ToList(),
                TotalCount = (int)totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasNextPage = filter.Page < totalPages,
                HasPreviousPage = filter.Page > 1
            };
        }

        public async Task<TaskResponseDto?> UpdateTaskAsync(string userId, string taskId, UpdateTaskDto updateTaskDto)
        {
            var updateBuilder = Builders<TaskItem>.Update;
            var updates = new List<UpdateDefinition<TaskItem>>
            {
                updateBuilder.Set(t => t.UpdatedAt, DateTime.UtcNow)
            };

            if (!string.IsNullOrEmpty(updateTaskDto.Title))
                updates.Add(updateBuilder.Set(t => t.Title, updateTaskDto.Title));

            if (updateTaskDto.Description != null)
                updates.Add(updateBuilder.Set(t => t.Description, updateTaskDto.Description));

            if (updateTaskDto.IsCompleted.HasValue)
            {
                updates.Add(updateBuilder.Set(t => t.IsCompleted, updateTaskDto.IsCompleted.Value));
                if (updateTaskDto.IsCompleted.Value)
                    updates.Add(updateBuilder.Set(t => t.CompletedAt, DateTime.UtcNow));
                else
                    updates.Add(updateBuilder.Unset(t => t.CompletedAt));
            }

            if (updateTaskDto.Priority.HasValue)
                updates.Add(updateBuilder.Set(t => t.Priority, updateTaskDto.Priority.Value));

            if (updateTaskDto.Category.HasValue)
                updates.Add(updateBuilder.Set(t => t.Category, updateTaskDto.Category.Value));

            if (updateTaskDto.DueDate.HasValue)
                updates.Add(updateBuilder.Set(t => t.DueDate, updateTaskDto.DueDate.Value));

            if (updateTaskDto.Tags != null)
                updates.Add(updateBuilder.Set(t => t.Tags, updateTaskDto.Tags));

            if (updateTaskDto.EstimatedHours.HasValue)
                updates.Add(updateBuilder.Set(t => t.EstimatedHours, updateTaskDto.EstimatedHours.Value));

            if (updateTaskDto.ActualHours.HasValue)
                updates.Add(updateBuilder.Set(t => t.ActualHours, updateTaskDto.ActualHours.Value));

            var combinedUpdate = updateBuilder.Combine(updates);
            var result = await _context.Tasks.FindOneAndUpdateAsync(
                t => t.Id == taskId && t.UserId == userId,
                combinedUpdate,
                new FindOneAndUpdateOptions<TaskItem> { ReturnDocument = ReturnDocument.After }
            );

            return result != null ? MapToResponseDto(result) : null;
        }

        public async Task<bool> DeleteTaskAsync(string userId, string taskId)
        {
            var result = await _context.Tasks.DeleteOneAsync(t => t.Id == taskId && t.UserId == userId);
            return result.DeletedCount > 0;
        }

        public async Task<bool> CompleteTaskAsync(string userId, string taskId)
        {
            var update = Builders<TaskItem>.Update
                .Set(t => t.IsCompleted, true)
                .Set(t => t.CompletedAt, DateTime.UtcNow)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Tasks.UpdateOneAsync(
                t => t.Id == taskId && t.UserId == userId,
                update
            );

            return result.ModifiedCount > 0;
        }

        public async Task<bool> UncompleteTaskAsync(string userId, string taskId)
        {
            var update = Builders<TaskItem>.Update
                .Set(t => t.IsCompleted, false)
                .Unset(t => t.CompletedAt)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Tasks.UpdateOneAsync(
                t => t.Id == taskId && t.UserId == userId,
                update
            );

            return result.ModifiedCount > 0;
        }

        public async Task<TaskStatsDto> GetTaskStatsAsync(string userId)
        {
            var filter = Builders<TaskItem>.Filter.Eq(t => t.UserId, userId);
            var tasks = await _context.Tasks.Find(filter).ToListAsync();

            var totalTasks = tasks.Count;
            var completedTasks = tasks.Count(t => t.IsCompleted);
            var pendingTasks = totalTasks - completedTasks;
            var overdueTasks = tasks.Count(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow);
            var completionRate = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0;

            var tasksByCategory = tasks.GroupBy(t => t.Category)
                .ToDictionary(g => g.Key, g => g.Count());

            var tasksByPriority = tasks.GroupBy(t => t.Priority)
                .ToDictionary(g => g.Key, g => g.Count());

            return new TaskStatsDto
            {
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                PendingTasks = pendingTasks,
                OverdueTasks = overdueTasks,
                CompletionRate = Math.Round(completionRate, 2),
                TasksByCategory = tasksByCategory,
                TasksByPriority = tasksByPriority
            };
        }

        public async Task<List<TaskResponseDto>> GetTasksDueTodayAsync(string userId)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var filter = Builders<TaskItem>.Filter.And(
                Builders<TaskItem>.Filter.Eq(t => t.UserId, userId),
                Builders<TaskItem>.Filter.Eq(t => t.IsCompleted, false),
                Builders<TaskItem>.Filter.Gte(t => t.DueDate, today),
                Builders<TaskItem>.Filter.Lt(t => t.DueDate, tomorrow)
            );

            var tasks = await _context.Tasks.Find(filter).ToListAsync();
            return tasks.Select(MapToResponseDto).ToList();
        }

        public async Task<List<TaskResponseDto>> GetOverdueTasksAsync(string userId)
        {
            var now = DateTime.UtcNow;

            var filter = Builders<TaskItem>.Filter.And(
                Builders<TaskItem>.Filter.Eq(t => t.UserId, userId),
                Builders<TaskItem>.Filter.Eq(t => t.IsCompleted, false),
                Builders<TaskItem>.Filter.Lt(t => t.DueDate, now)
            );

            var tasks = await _context.Tasks.Find(filter).ToListAsync();
            return tasks.Select(MapToResponseDto).ToList();
        }

        public async Task<List<TaskResponseDto>> GetTasksByTagAsync(string userId, string tag)
        {
            var filter = Builders<TaskItem>.Filter.And(
                Builders<TaskItem>.Filter.Eq(t => t.UserId, userId),
                Builders<TaskItem>.Filter.AnyEq(t => t.Tags, tag)
            );

            var tasks = await _context.Tasks.Find(filter).ToListAsync();
            return tasks.Select(MapToResponseDto).ToList();
        }

        private static TaskResponseDto MapToResponseDto(TaskItem task)
        {
            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                Priority = task.Priority,
                Category = task.Category,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                CompletedAt = task.CompletedAt,
                Tags = task.Tags,
                EstimatedHours = task.EstimatedHours,
                ActualHours = task.ActualHours
            };
        }

        private static SortDefinition<TaskItem> GetSortDefinition(string sortBy, bool descending)
        {
            var sortBuilder = Builders<TaskItem>.Sort;
            
            return sortBy.ToLower() switch
            {
                "title" => descending ? sortBuilder.Descending(t => t.Title) : sortBuilder.Ascending(t => t.Title),
                "priority" => descending ? sortBuilder.Descending(t => t.Priority) : sortBuilder.Ascending(t => t.Priority),
                "category" => descending ? sortBuilder.Descending(t => t.Category) : sortBuilder.Ascending(t => t.Category),
                "duedate" => descending ? sortBuilder.Descending(t => t.DueDate) : sortBuilder.Ascending(t => t.DueDate),
                "updatedat" => descending ? sortBuilder.Descending(t => t.UpdatedAt) : sortBuilder.Ascending(t => t.UpdatedAt),
                _ => descending ? sortBuilder.Descending(t => t.CreatedAt) : sortBuilder.Ascending(t => t.CreatedAt)
            };
        }
    }
} 