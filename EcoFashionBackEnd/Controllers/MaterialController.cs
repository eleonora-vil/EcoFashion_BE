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
            // Use the new comprehensive method with default parameters (public only, sorted by sustainability)
            var result = await _materialService.GetAllMaterialsWithFiltersAsync(
                sortBySustainability: true, 
                publicOnly: true
            );
            return Ok(result);
        }
        
        /// <summary>
        /// Get all materials with comprehensive filtering and sorting by sustainability score
        /// </summary>
        [HttpGet("filtered")]
        public async Task<IActionResult> GetAllMaterialsWithFilters(
            [FromQuery] int? typeId = null,
            [FromQuery] string? supplierId = null,
            [FromQuery] string? supplierName = null,
            [FromQuery] string? materialName = null,
            [FromQuery] string? productionCountry = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] int? minQuantity = null,
            [FromQuery] bool? hasCertification = null,
            [FromQuery] string? transportMethod = null,
            [FromQuery] bool sortBySustainability = true,
            [FromQuery] bool publicOnly = true)
        {
            try
            {
                Guid? supplierGuid = null;
                if (!string.IsNullOrWhiteSpace(supplierId))
                {
                    if (Guid.TryParse(supplierId, out var parsedGuid))
                    {
                        supplierGuid = parsedGuid;
                    }
                    else
                    {
                        return BadRequest(ApiResult<object>.Fail("Invalid supplier ID format"));
                    }
                }

                var result = await _materialService.GetAllMaterialsWithFiltersAsync(
                    typeId: typeId,
                    supplierId: supplierGuid,
                    supplierName: supplierName,
                    materialName: materialName,
                    productionCountry: productionCountry,
                    minPrice: minPrice,
                    maxPrice: maxPrice,
                    minQuantity: minQuantity,
                    hasCertification: hasCertification,
                    transportMethod: transportMethod,
                    sortBySustainability: sortBySustainability,
                    publicOnly: publicOnly
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.Fail($"Error: {ex.Message}"));
            }
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
            var countries = TransportCalculationService.GetSupportedCountries();
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
            // Use the new comprehensive method with typeId filter (public only, sorted by sustainability)
            var result = await _materialService.GetAllMaterialsWithFiltersAsync(
                typeId: typeId,
                sortBySustainability: true,
                publicOnly: true
            );
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

        [HttpGet("GetAvailableTransportMethods")]
        public IActionResult GetAvailableTransportMethods([FromQuery] string country)
        {
            try
            {
                if (string.IsNullOrEmpty(country))
                {
                    return BadRequest(ApiResult<object>.Fail("Country parameter is required"));
                }

                var transportMethods = TransportCalculationService.GetAvailableTransportMethods(country);
                return Ok(ApiResult<object>.Succeed(transportMethods));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.Fail($"Error getting transport methods: {ex.Message}"));
            }
        }

        [HttpGet("GetSupportedCountries")]
        public IActionResult GetSupportedCountries()
        {
            try
            {
                var countries = TransportCalculationService.GetSupportedCountries();
                return Ok(ApiResult<object>.Succeed(countries));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.Fail($"Error getting supported countries: {ex.Message}"));
            }
        }


    }
}
