using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcoFashionBackEnd.Services;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Common;
using EcoFashionBackEnd.Common.Payloads.Responses;
using Microsoft.AspNetCore.Http;
using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialController : ControllerBase
    {
        private readonly MaterialService _materialService;
        private readonly SustainabilityService _sustainabilityService;
        public MaterialController(MaterialService materialService, SustainabilityService sustainabilityService)
        {
            _materialService = materialService;
            _sustainabilityService = sustainabilityService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMaterials()
        {
            var result = await _materialService.GetAllMaterialsAsync();
            return Ok(result);
        }

        // Admin: get all materials regardless of approval/availability
        [HttpGet("admin/all")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllMaterialsAdmin()
        {
            var result = await _materialService.GetAllMaterialsAdminAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMaterialById(int id)
        {
            var result = await _materialService.GetMaterialDetailByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("CreateWithSustainability")]
        [Authorize(Roles = "supplier")]
        public async Task<IActionResult> CreateMaterialFromForm([FromBody] MaterialCreationFormRequest request)
        {
            // Clean flow: trust supplierId from client (FE ensures correct), no fallback
            var result = await _materialService.CreateMaterialFromFormAsync(request);
            return Ok(result);
        }

        [HttpPost("{materialId}/images")]
        [Authorize(Roles = "supplier")]
        public async Task<IActionResult> UploadMaterialImages([FromRoute] int materialId, [FromForm] List<IFormFile> files)
        {
            var result = await _materialService.UploadMaterialImagesAsync(materialId, files);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var result = await _materialService.DeleteMaterialAsync(id);
            return Ok(result);
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ApproveMaterial(int id, [FromBody] string? adminNote)
        {
            var result = await _materialService.SetMaterialApprovalStatusAsync(id, true, adminNote);
            return Ok(result);
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RejectMaterial(int id, [FromBody] string? adminNote)
        {
            var result = await _materialService.SetMaterialApprovalStatusAsync(id, false, adminNote);
            return Ok(result);
        }

        [HttpGet("Sustainability/{materialId}")]
        public async Task<IActionResult> GetMaterialSustainability(int materialId)
        {
            var result = await _sustainabilityService.CalculateMaterialSustainabilityScore(materialId);
            if (result == null)
                return NotFound("Material not found");
            
            return Ok(result);
        }

        [HttpGet("GetProductionCountries")]
        public IActionResult GetProductionCountries()
        {
            var countries = TransportCalculationService.GetCommonProductionCountries();
            return Ok(new { countries });
        }

        [HttpGet("CalculateTransport/{country}")]
        public IActionResult CalculateTransport(string country)
        {
            var (distance, method, description) = TransportCalculationService.GetTransportDetails(country);
            return Ok(new { distance, method, description });
        }

        [HttpGet("GetTransportEvaluation/{distance}/{method}")]
        public IActionResult GetTransportEvaluation(decimal distance, string method)
        {
            var evaluation = TransportCalculationService.GetTransportEvaluation(distance, method);
            return Ok(evaluation);
        }

        [HttpGet("GetProductionEvaluation/{country}")]
        public IActionResult GetProductionEvaluation(string country)
        {
            var evaluation = TransportCalculationService.GetProductionEvaluation(country);
            return Ok(evaluation);
        }

        [HttpGet("GetSustainabilityEvaluation/{score}")]
        public IActionResult GetSustainabilityEvaluation(decimal score)
        {
            var evaluation = _sustainabilityService.GetSustainabilityEvaluation(score);
            return Ok(evaluation);
        }

        [HttpGet("GetAllMaterialByType/{typeId}")]
        public async Task<IActionResult> GetAllMaterialByType(int typeId)
        {
            var result = await _materialService.GetAllMaterialByTypeAsync(typeId);
            return Ok(result);
        }

        [HttpGet("GetAllMaterialTypes")]
        public async Task<IActionResult> GetAllMaterialTypes()
        {
            var result = await _materialService.GetAllMaterialTypesAsync();
            return Ok(result);
        }

        [HttpGet("GetSupplierMaterials")]
        [Authorize(Roles = "admin,supplier")]
        public async Task<IActionResult> GetSupplierMaterials([FromQuery] string supplierId, [FromQuery] string? approvalStatus)
        {
            try
            {
                var result = await _materialService.GetSupplierMaterialsAsync(supplierId, approvalStatus);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.Fail($"Error getting supplier materials: {ex.Message}"));
            }
        }


    }
}
