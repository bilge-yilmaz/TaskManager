using TaskManager.DTOs;
using TaskManager.Models;

namespace TaskManager.Services
{
    public interface ITaskService
    {
        Task<TaskResponseDto> CreateTaskAsync(string userId, CreateTaskDto createTaskDto);
        Task<TaskResponseDto?> GetTaskByIdAsync(string userId, string taskId);
        Task<PagedResultDto<TaskResponseDto>> GetTasksAsync(string userId, TaskFilterDto filter);
        Task<TaskResponseDto?> UpdateTaskAsync(string userId, string taskId, UpdateTaskDto updateTaskDto);
        Task<bool> DeleteTaskAsync(string userId, string taskId);
        Task<bool> CompleteTaskAsync(string userId, string taskId);
        Task<bool> UncompleteTaskAsync(string userId, string taskId);
        Task<TaskStatsDto> GetTaskStatsAsync(string userId);
        Task<List<TaskResponseDto>> GetTasksDueTodayAsync(string userId);
        Task<List<TaskResponseDto>> GetOverdueTasksAsync(string userId);
        Task<List<TaskResponseDto>> GetTasksByTagAsync(string userId, string tag);
    }
} 