using EcoFashionBackEnd.Common;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Dtos;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _dbContext;

        public NotificationService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiResult<List<NotificationResponse>>> GetUserNotificationsAsync(int userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var notifications = await _dbContext.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(n => new NotificationResponse
                    {
                        NotificationId = n.NotificationId,
                        Title = n.Title,
                        Message = n.Message,
                        Type = n.Type,
                        IsRead = n.IsRead,
                        CreatedAt = n.CreatedAt,
                        RelatedId = n.RelatedId,
                        RelatedType = n.RelatedType
                    })
                    .ToListAsync();

                return ApiResult<List<NotificationResponse>>.Succeed(notifications);
            }
            catch (Exception ex)
            {
                return ApiResult<List<NotificationResponse>>.Fail($"Error getting notifications: {ex.Message}");
            }
        }

        public async Task<ApiResult<int>> GetUnreadCountAsync(int userId)
        {
            try
            {
                var count = await _dbContext.Notifications
                    .CountAsync(n => n.UserId == userId && !n.IsRead);

                return ApiResult<int>.Succeed(count);
            }
            catch (Exception ex)
            {
                return ApiResult<int>.Fail($"Error getting unread count: {ex.Message}");
            }
        }

        public async Task<ApiResult<bool>> MarkAsReadAsync(int notificationId, int userId)
        {
            try
            {
                var notification = await _dbContext.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

                if (notification == null)
                    return ApiResult<bool>.Fail("Notification not found");

                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();
                return ApiResult<bool>.Succeed(true);
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Fail($"Error marking notification as read: {ex.Message}");
            }
        }

        public async Task<ApiResult<bool>> MarkAllAsReadAsync(int userId)
        {
            try
            {
                var notifications = await _dbContext.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync();
                return ApiResult<bool>.Succeed(true);
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Fail($"Error marking all notifications as read: {ex.Message}");
            }
        }

        // Create notification for material approval
        public async Task<ApiResult<bool>> CreateMaterialApprovalNotificationAsync(int materialId, Guid supplierId, string status, string? adminNote = null)
        {
            try
            {
                var material = await _dbContext.Materials
                    .Include(m => m.Supplier)
                    .FirstOrDefaultAsync(m => m.MaterialId == materialId);

                if (material == null)
                    return ApiResult<bool>.Fail("Material not found");

                var notification = new Notification
                {
                    UserId = material.Supplier!.UserId,
                    Title = GetApprovalNotificationTitle(status),
                    Message = GetApprovalNotificationMessage(material.Name ?? "Unknown", status, adminNote),
                    Type = GetApprovalNotificationType(status),
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    RelatedId = materialId.ToString(),
                    RelatedType = "Material"
                };

                _dbContext.Notifications.Add(notification);
                await _dbContext.SaveChangesAsync();

                return ApiResult<bool>.Succeed(true);
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Fail($"Error creating notification: {ex.Message}");
            }
        }

        // Create notification for new material submission (for admin)
        public async Task<ApiResult<bool>> CreateNewMaterialNotificationAsync(int materialId)
        {
            try
            {
                var material = await _dbContext.Materials
                    .Include(m => m.Supplier)
                    .FirstOrDefaultAsync(m => m.MaterialId == materialId);

                if (material == null)
                    return ApiResult<bool>.Fail("Material not found");

                // Get all admin users
                var adminUsers = await _dbContext.Users
                    .Include(u => u.UserRole)
                    .Where(u => u.UserRole != null && u.UserRole.RoleName.ToLower() == "admin")
                    .ToListAsync();

                foreach (var admin in adminUsers)
                {
                    var notification = new Notification
                    {
                        UserId = admin.UserId,
                        Title = "Vật liệu mới cần phê duyệt",
                        Message = $"Vật liệu '{material.Name}' từ nhà cung cấp {material.Supplier?.SupplierName} đang chờ phê duyệt.",
                        Type = "info",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow,
                RelatedId = materialId.ToString(),
                        RelatedType = "Material"
                    };

                    _dbContext.Notifications.Add(notification);
                }

                await _dbContext.SaveChangesAsync();
                return ApiResult<bool>.Succeed(true);
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Fail($"Error creating admin notifications: {ex.Message}");
            }
        }

        private string GetApprovalNotificationTitle(string status)
        {
            return status switch
            {
                "Approved" => "Vật liệu được phê duyệt",
                "Rejected" => "Vật liệu bị từ chối",
                _ => "Cập nhật trạng thái vật liệu"
            };
        }

        private string GetApprovalNotificationMessage(string materialName, string status, string? adminNote)
        {
            var baseMessage = status switch
            {
                "Approved" => $"Vật liệu '{materialName}' đã được phê duyệt và có thể hiển thị công khai.",
                "Rejected" => $"Vật liệu '{materialName}' đã bị từ chối.",
                _ => $"Trạng thái vật liệu '{materialName}' đã được cập nhật."
            };

            if (!string.IsNullOrEmpty(adminNote))
            {
                baseMessage += $" Ghi chú: {adminNote}";
            }

            return baseMessage;
        }

        private string GetApprovalNotificationType(string status)
        {
            return status switch
            {
                "Approved" => "success",
                "Rejected" => "error",
                _ => "info"
            };
        }
    }
}
