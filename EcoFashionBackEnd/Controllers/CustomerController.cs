using EcoFashionBackEnd.Common.Payloads.Responses;
using EcoFashionBackEnd.Common;
using EcoFashionBackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using EcoFashionBackEnd.Common.Payloads.Requests;

namespace EcoFashionBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _customerService;
        public CustomerController(CustomerService customerService)
        {
            _customerService = customerService;
        }
        [HttpGet("GetAll")] // Định nghĩa route cho action này, ví dụ: /api/Supplier/GetAll
        public async Task<IActionResult> GetAllCustomers()
        {
            var result = await _customerService.GetAllCustomers();
            return Ok(ApiResult<GetCustomerResponse>.Succeed(new GetCustomerResponse
            {
                Customers = result
            }));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _customerService.GetCustomerById(id);
            if (customer == null)
            {
                return NotFound(ApiResult<CustomerDetailResponse>.Fail("Không tìm thấy khách hàng."));
            }
            return Ok(ApiResult<CustomerDetailResponse>.Succeed(new CustomerDetailResponse
            {
                Customer = customer
            }));
        }
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
        {
            var customerId = await _customerService.CreateCustomer(request);
            return CreatedAtAction(nameof(GetCustomerById), new { id = customerId }, ApiResult<int>.Succeed(customerId));
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerRequest request)
        {
            var isUpdated = await _customerService.UpdateCustomer(id, request);
            if (!isUpdated)
            {
                return NotFound(ApiResult<object>.Fail("Không tìm thấy khách hàng để cập nhật."));
            }
            return Ok(ApiResult<object>.Succeed("Cập nhật thông tin khách hàng thành công."));
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var isDeleted = await _customerService.DeactiveCustomer(id);
            if (!isDeleted)
            {
                return NotFound(ApiResult<object>.Fail("Không tìm thấy khách hàng để xóa."));
            }
            return Ok(ApiResult<object>.Succeed("Vô hiệu hóa khách hàng thành công."));
        }
    }
}
