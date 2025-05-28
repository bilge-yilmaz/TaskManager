using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.DTOs;
using TaskManager.Services;

namespace TaskManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        /// <summary>
        /// Yeni görev oluştur
        /// </summary>
        /// <param name="createTaskDto">Görev bilgileri</param>
        /// <returns>Oluşturulan görev</returns>
        [HttpPost]
        public async Task<ActionResult<TaskResponseDto>> CreateTask([FromBody] CreateTaskDto createTaskDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Geçersiz token." });
                }

                var result = await _taskService.CreateTaskAsync(userId, createTaskDto);
                return CreatedAtAction(nameof(GetTask), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Görev oluşturulurken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Görev detayını getir
        /// </summary>
        /// <param name="id">Görev ID</param>
        /// <returns>Görev detayı</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponseDto>> GetTask(string id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Geçersiz token." });
                }

                var task = await _taskService.GetTaskByIdAsync(userId, id);
                if (task == null)
                {
                    return NotFound(new { message = "Görev bulunamadı." });
                }

                return Ok(task);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Görev getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Görevleri listele (filtreleme ve sayfalama ile)
        /// </summary>
        /// <param name="filter">Filtreleme parametreleri</param>
        /// <returns>Sayfalanmış görev listesi</returns>
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<TaskResponseDto>>> GetTasks([FromQuery] TaskFilterDto filter)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Geçersiz token." });
                }

                var result = await _taskService.GetTasksAsync(userId, filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Görevler getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Görevi güncelle
        /// </summary>
        /// <param name="id">Görev ID</param>
        /// <param name="updateTaskDto">Güncellenecek bilgiler</param>
        /// <returns>Güncellenmiş görev</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<TaskResponseDto>> UpdateTask(string id, [FromBody] UpdateTaskDto updateTaskDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Geçersiz token." });
                }

                var result = await _taskService.UpdateTaskAsync(userId, id, updateTaskDto);
                if (result == null)
                {
                    return NotFound(new { message = "Görev bulunamadı." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Görev güncellenirken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Görevi sil
        /// </summary>
        /// <param name="id">Görev ID</param>
        /// <returns>İşlem sonucu</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTask(string id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Geçersiz token." });
                }

                var result = await _taskService.DeleteTaskAsync(userId, id);
                if (!result)
                {
                    return NotFound(new { message = "Görev bulunamadı." });
                }

                return Ok(new { message = "Görev başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Görev silinirken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Görevi tamamla
        /// </summary>
        /// <param name="id">Görev ID</param>
        /// <returns>İşlem sonucu</returns>
        [HttpPost("{id}/complete")]
        public async Task<ActionResult> CompleteTask(string id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Geçersiz token." });
                }

                var result = await _taskService.CompleteTaskAsync(userId, id);
                if (!result)
                {
                    return NotFound(new { message = "Görev bulunamadı." });
                }

                return Ok(new { message = "Görev başarıyla tamamlandı." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Görev tamamlanırken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Görev tamamlanma durumunu geri al
        /// </summary>
        /// <param name="id">Görev ID</param>
        /// <returns>İşlem sonucu</returns>
        [HttpPost("{id}/uncomplete")]
        public async Task<ActionResult> UncompleteTask(string id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Geçersiz token." });
                }

                var result = await _taskService.UncompleteTaskAsync(userId, id);
                if (!result)
                {
                    return NotFound(new { message = "Görev bulunamadı." });
                }

                return Ok(new { message = "Görev tamamlanma durumu geri alındı." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Görev durumu değiştirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Görev istatistiklerini getir
        /// </summary>
        /// <returns>Görev istatistikleri</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<TaskStatsDto>> GetTaskStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Geçersiz token." });
                }

                var result = await _taskService.GetTaskStatsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "İstatistikler getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Bugün teslim edilecek görevleri getir
        /// </summary>
        /// <returns>Bugünkü görevler</returns>
        [HttpGet("due-today")]
        public async Task<ActionResult<List<TaskResponseDto>>> GetTasksDueToday()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Geçersiz token." });
                }

                var result = await _taskService.GetTasksDueTodayAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bugünkü görevler getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Süresi geçmiş görevleri getir
        /// </summary>
        /// <returns>Süresi geçmiş görevler</returns>
        [HttpGet("overdue")]
        public async Task<ActionResult<List<TaskResponseDto>>> GetOverdueTasks()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Geçersiz token." });
                }

                var result = await _taskService.GetOverdueTasksAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Süresi geçmiş görevler getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Belirli bir etikete sahip görevleri getir
        /// </summary>
        /// <param name="tag">Etiket</param>
        /// <returns>Etiketli görevler</returns>
        [HttpGet("by-tag/{tag}")]
        public async Task<ActionResult<List<TaskResponseDto>>> GetTasksByTag(string tag)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Geçersiz token." });
                }

                var result = await _taskService.GetTasksByTagAsync(userId, tag);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Etiketli görevler getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
} 