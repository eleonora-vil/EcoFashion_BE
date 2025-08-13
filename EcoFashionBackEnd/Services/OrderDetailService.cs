using AutoMapper;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Services;

public class OrderDetailService
{
    private readonly IRepository<OrderDetail, int> _orderDetailRepository;
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    public OrderDetailService(IRepository<OrderDetail, int> repository, IMapper mapper, AppDbContext context)
    {
        _orderDetailRepository = repository;
        _dbContext = context;
        _mapper = mapper;
    }
    public async Task<IEnumerable<OrderDetailModel>> GetAllOrderDetailAsync()
    {
        var orderDetails = await _dbContext.OrderDetails
            .Include(od => od.Order)
            .Include(od => od.Designer)
            .Include(od => od.Material).ThenInclude(m => m.MaterialImages)
                .ThenInclude(mi => mi.Image)
            .Include(od => od.Supplier)
            .ToListAsync();
        var result = orderDetails.Select(od =>
        {
            string itemName;
            string providerName;
            string? imageUrl = null;

            if (od.Type == OrderDetailType.design && od.Design != null)
            {
                itemName = od.Design.Name ?? "Không có tên thiết kế";
                providerName = od.Designer?.DesignerName ?? "Nhà thiết kế không tồn tại";
            }
            else if (od.Type == OrderDetailType.material && od.Material != null)
            {
                itemName = od.Material.Name ?? "Không có tên chất liệu";
                providerName = od.Supplier?.SupplierName ?? "Nhà cung cấp không tồn tại";
                imageUrl = od.Material.MaterialImages.FirstOrDefault()?.Image?.ImageUrl;
            }
            else
            {
                // Fallback: unknown type or corrupted data
                itemName = "Không xác định";
                providerName = "Không xác định";
            }

            return new OrderDetailModel
            {
                OrderDetailId = od.OrderDetailId,
                OrderId = od.OrderId,
                Quantity = od.Quantity,
                UnitPrice = od.UnitPrice,
                Type = od.Type.ToString(),
                Status = od.Status.ToString(),

                DesignId = od.Type == OrderDetailType.design ? od.DesignId : null,
                DesignerId = od.Type == OrderDetailType.design ? od.DesignerId : null,
                MaterialId = od.Type == OrderDetailType.material ? od.MaterialId : 0,
                SupplierId = od.Type == OrderDetailType.material ? od.SupplierId : null,

                ItemName = itemName,
                ProviderName = providerName,
                ImageUrl = imageUrl
            };
        });

        return result;
    }
    public async Task<OrderDetailModel?> GetOrderDetailByIdAsync(int id)
    {
        var od = await _dbContext.OrderDetails
            .Include(o => o.Order)
            .Include(o => o.Designer)
            .Include(o => o.Material).ThenInclude(m => m.MaterialImages).ThenInclude(mi => mi.Image)
            .Include(o => o.Supplier)
            .FirstOrDefaultAsync(o => o.OrderDetailId == id);

        if (od == null)
            return null;

        string itemName = "Không xác định";
        string providerName = "Không xác định";
        string? imageUrl = null;

        if (od.Type == OrderDetailType.design && od.Design != null)
        {
            itemName = od.Design?.Name ?? "Không có tên thiết kế";
            providerName = od.Designer?.DesignerName ?? "Nhà thiết kế không tồn tại";
        }
        else if (od.Type == OrderDetailType.material && od.Material != null)
        {
            itemName = od.Material?.Name ?? "Không có tên chất liệu";
            providerName = od.Supplier?.SupplierName ?? "Nhà cung cấp không tồn tại";
            imageUrl = od.Material?.MaterialImages.FirstOrDefault()?.Image?.ImageUrl;
        }

        return new OrderDetailModel
        {
            OrderDetailId = od.OrderDetailId,
            OrderId = od.OrderId,
            Quantity = od.Quantity,
            UnitPrice = od.UnitPrice,
            Type = od.Type.ToString(),
            Status = od.Status.ToString(),

            DesignId = od.Type == OrderDetailType.design ? od.DesignId : null,
            DesignerId = od.Type == OrderDetailType.design ? od.DesignerId : null,
            MaterialId = od.Type == OrderDetailType.material ? od.MaterialId : null,
            SupplierId = od.Type == OrderDetailType.material ? od.SupplierId : null,

            ItemName = itemName,
            ProviderName = providerName,
            ImageUrl = imageUrl
        };
    }
    public async Task<int> CreateOrderDetailAsync(int userId, CreateOrderDetailRequest request)
    {
        // Kiểm tra chỉ được chọn một trong hai: Design hoặc Material
        if (request.Type == OrderDetailType.design && request.DesignId == null)
            throw new ArgumentException("Thiết kế không được để trống khi loại là 'design'.");
        if (request.Type == OrderDetailType.material && request.MaterialId == null)
            throw new ArgumentException("Chất liệu không được để trống khi loại là 'material'.");
        if (request.Type == OrderDetailType.design && request.MaterialId != null)
            throw new ArgumentException("Không được chọn chất liệu khi đơn hàng là 'design'.");
        if (request.Type == OrderDetailType.material && request.DesignId != null)
            throw new ArgumentException("Không được chọn thiết kế khi đơn hàng là 'material'.");
        var orderDetail = _mapper.Map<OrderDetail>(request);
        _dbContext.OrderDetails.Add(orderDetail);
        await _dbContext.SaveChangesAsync();
        return orderDetail.OrderDetailId;
    }
    public async Task<bool> UpdateOrderDetailAsync(int orderDetailId, UpdateOrderDetailRequest request)
    {
        var orderDetail = await _orderDetailRepository.GetByIdAsync(orderDetailId);
        if (orderDetail == null) return false;
        if (request.Quantity.HasValue)
            orderDetail.Quantity = request.Quantity.Value;
        if (request.UnitPrice.HasValue)
            orderDetail.UnitPrice = request.UnitPrice.Value;
        if (!string.IsNullOrWhiteSpace(request.Status))
            if (Enum.TryParse<OrderDetailStatus>(request.Status, true, out var parsedStatus))
                orderDetail.Status = parsedStatus;
            else
                throw new ArgumentException("Trạng thái chi tiết đơn hàng không hợp lệ");
        _orderDetailRepository.Update(orderDetail);
        var result = await _dbContext.SaveChangesAsync();
        return result > 0;
    }
    public async Task<bool> DeleteOrderDetailAsync(int orderDetailId)
    {
        var orderDetail = await _orderDetailRepository.GetByIdAsync(orderDetailId);
        if (orderDetail == null) return false;
        _orderDetailRepository.Remove(orderDetailId);
        var result = await _dbContext.SaveChangesAsync();
        return result > 0;
    }
}
