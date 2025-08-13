using EcoFashionBackEnd.Common;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Common.Payloads.Responses;
using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcoFashionBackEnd.Controllers
{
    [Route("api/Applications")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly ApplicationService _applicationService;

        public ApplicationController(ApplicationService applicationService)
        {
            _applicationService = applicationService;
        }


        // Apply as Supplier or Designer
        [Authorize]
        [HttpPost("ApplySupplier")]
        public async Task<IActionResult> ApplyAsSupplier([FromForm] ApplySupplierRequest request)
        {
            // Lấy UserId từ claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(ApiResult<ApplicationModel>.Fail("Không thể xác định người dùng."));
            }

            var application = await _applicationService.ApplyAsSupplier(userId, request);
            if (application != null)
            {
                return CreatedAtAction("GetApplicationById", new { id = application.ApplicationId }, ApiResult<ApplicationModel>.Succeed(application));
            }
            return BadRequest(ApiResult<object>.Fail("Không thể gửi đơn đăng ký trở thành nhà cung cấp."));
        }

        [Authorize]
        [HttpPost("ApplyDesigner")]
        public async Task<IActionResult> ApplyAsDesigner([FromForm] ApplyDesignerRequest request)
        {
            // Lấy UserId từ claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(ApiResult<ApplicationModel>.Fail("Không thể xác định người dùng."));
            }

            var application = await _applicationService.ApplyAsDesigner(userId, request);
            if (application != null)
            {
                return Ok(ApiResult<ApplicationModel>.Succeed(application));
            }
            return BadRequest(ApiResult<object>.Fail("Không thể gửi đơn đăng ký trở thành nhà thiết kế."));
        }
        // -----------------------------------------------
        // Get Application by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetApplicationById(int id)
        {
            var application = await _applicationService.GetApplicationById(id);
            if (application == null)
            {
                return NotFound(ApiResult<ApplicationModel>.Fail("Không tìm thấy đơn đăng ký."));
            }
            return Ok(ApiResult<ApplicationModel>.Succeed(application));
        }


        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllApplications()
        {
            try
            {
                var applications = await _applicationService.GetAllApplications();
                var response = new GetApplicationsResponse
                {
                    Applications = applications
                };
                return Ok(ApiResult<GetApplicationsResponse>.Succeed(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.Fail($"Lỗi khi lấy danh sách đơn đăng ký: {ex.Message}"));
            }
        }


        [Authorize(Roles = "admin")]
        [HttpPut("{applicationId}/ApproveSupplier")]
        public async Task<IActionResult> ApproveSupplierApplication(int applicationId)
        {
            try
            {
                // Lấy AdminId từ claims
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !int.TryParse(adminIdClaim.Value, out int adminId))
                {
                    adminId = 1; // Default admin ID
                }

                var isApproved = await _applicationService.ApproveSupplierApplication(applicationId, adminId);
                if (isApproved)
                {
                    return Ok(ApiResult<object>.Succeed("Đơn đăng ký nhà cung cấp đã được phê duyệt và hồ sơ nhà cung cấp đã được tạo."));
                }
                return BadRequest(ApiResult<object>.Fail("Không thể phê duyệt đơn đăng ký nhà cung cấp hoặc đơn đăng ký không tồn tại."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.Fail($"Lỗi khi phê duyệt: {ex.Message}"));
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{applicationId}/ApproveDesigner")]
        public async Task<IActionResult> ApproveDesignerApplication(int applicationId)
        {
            try
            {
                // Lấy AdminId từ claims
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (adminIdClaim == null || !int.TryParse(adminIdClaim.Value, out int adminId))
                {
                    adminId = 1; // Default admin ID
                }

                var isApproved = await _applicationService.ApproveDesignerApplication(applicationId, adminId);
                if (isApproved)
                {
                    return Ok(ApiResult<object>.Succeed("Đơn đăng ký nhà thiết kế đã được phê duyệt và hồ sơ nhà thiết kế đã được tạo."));
                }
                return BadRequest(ApiResult<object>.Fail("Không thể phê duyệt đơn đăng ký nhà thiết kế hoặc đơn đăng ký không tồn tại."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.Fail($"Lỗi khi phê duyệt: {ex.Message}"));
            }
        }


        [Authorize(Roles = "admin")]
        [HttpPut("{applicationId}/Reject")]
        public async Task<IActionResult> RejectApplication(int applicationId, [FromBody] RejectApplicationRequest request)
        {
            // Lấy AdminId từ claims
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (adminIdClaim == null || !int.TryParse(adminIdClaim.Value, out int adminId))
            {
                return Unauthorized(ApiResult<object>.Fail("Không thể xác định admin."));
            }

            var isRejected = await _applicationService.RejectApplication(applicationId, adminId, request.RejectionReason);
            if (isRejected)
            {
                return Ok(ApiResult<object>.Succeed("Đơn đăng ký đã bị từ chối."));
            }
            return BadRequest(ApiResult<object>.Fail("Không thể từ chối đơn đăng ký hoặc đơn đăng ký không tồn tại."));
        }

        [HttpGet("Filter")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> FilterApplications(
        [FromQuery] string? status, // Thay ApplicationStatus? bằng string?
        [FromQuery] int? targetRoleId,
        [FromQuery] DateTime? createdFrom,
        [FromQuery] DateTime? createdTo)
        {
            ApplicationStatus? parsedStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ApplicationStatus>(status, true, out var parsed))
            {
                parsedStatus = parsed;
            }
            else if (!string.IsNullOrEmpty(status))
            {
                ModelState.AddModelError("status", "Giá trị trạng thái không hợp lệ.");
                return BadRequest(ModelState);
            }

            var applications = await _applicationService.FilterApplications(parsedStatus, targetRoleId, createdFrom, createdTo);
            return Ok(ApiResult<GetApplicationsResponse>.Succeed(new GetApplicationsResponse
            {
                Applications = applications
            }));
        }

        [HttpGet("Search")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> SearchApplications([FromQuery] string? keyword)
        {
            var applications = await _applicationService.SearchApplications(keyword);
            return Ok(ApiResult<GetApplicationsResponse>.Succeed(new GetApplicationsResponse
            {
                Applications = applications
            }));
        }

        [HttpGet("MyApplications")]
        [Authorize]
        public async Task<IActionResult> GetMyApplications()
        {
            try
            {
                // Lấy UserId từ claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(ApiResult<object>.Fail("Không thể xác định người dùng."));
                }

                var applications = await _applicationService.GetApplicationsByUserId(userId);
                return Ok(ApiResult<List<ApplicationModel>>.Succeed(applications.ToList()));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.Fail($"Lỗi khi lấy danh sách đơn đăng ký: {ex.Message}"));
            }
        }
    }
}