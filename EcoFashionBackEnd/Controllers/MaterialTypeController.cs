using EcoFashionBackEnd.Common;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Dtos.Material;
using EcoFashionBackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EcoFashionBackEnd.Controllers
{
    [Route("api/MaterialTypes")]
    [ApiController]
    public class MaterialTypeController : ControllerBase
    {
        private readonly MaterialTypeService _materialTypeService;
        public MaterialTypeController(MaterialTypeService materialTypeService)
        {
            _materialTypeService = materialTypeService;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMaterialTypeById(int id)
        {
            var materialType = await _materialTypeService.GetMaterialTypeByIdAsync(id);
            if (materialType == null)
                return NotFound(ApiResult<MaterialTypeModel>.Fail("Không tìm thấy loại vật liệu"));
            return Ok(ApiResult<MaterialTypeModel>.Succeed(materialType));
        }
        [HttpGet]
        public async Task<IActionResult> GetAllMaterialTypes()
        {
            try
            {
                var materialTypes = await _materialTypeService.GetAllMaterialTypesAsync();
                return Ok(ApiResult<List<MaterialTypeModel>>.Succeed(materialTypes.ToList()));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.Fail($"Lỗi khi lấy danh sách loại vật liệu: {ex.Message}"));
            }
        }

        // Benchmarks for a specific material type admin only 
        [HttpGet("{id}/benchmarks")]
        [Authorize(Roles = "admin")] 
        public async Task<IActionResult> GetBenchmarksByType(int id)
        {
            var result = await _materialTypeService.GetBenchmarksByTypeAsync(id);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> CreateMaterialType([FromBody] MaterialTypeRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResult<object>.Fail("Dữ liệu không hợp lệ"));

            try
            {
                var materialType = await _materialTypeService.CreateMaterialTypeAsync(model);
                return Ok(ApiResult<MaterialTypeModel>.Succeed(materialType));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.Fail($"Lỗi khi tạo loại vật liệu: {ex.Message}"));
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMaterialType(int id, [FromBody] MaterialTypeRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResult<object>.Fail("Dữ liệu không hợp lệ"));

            try
            {
                var materialType = await _materialTypeService.UpdateMaterialTypeAsync(id, model);
                if (materialType == null)
                    return NotFound(ApiResult<object>.Fail("Không tìm thấy loại vật liệu cần cập nhật"));

                return Ok(ApiResult<MaterialTypeModel>.Succeed(materialType));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.Fail($"Lỗi khi cập nhật loại vật liệu: {ex.Message}"));
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaterialType(int id)
        {
            try
            {
                var deleted = await _materialTypeService.DeleteMaterialTypeAsync(id);
                if (!deleted)
                    return NotFound(ApiResult<object>.Fail("Không tìm thấy loại vật liệu cần xóa"));

                return Ok(ApiResult<object>.Succeed("Xóa loại vật liệu thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult<object>.Fail($"Lỗi khi xóa loại vật liệu: {ex.Message}"));
            }
        }
    }
}