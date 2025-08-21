using AutoMapper;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Services
{
    public class OrderService
    {
        private readonly IRepository<Order, int> _orderRepository;
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        public OrderService(IRepository<Order, int> repository, AppDbContext dbContext, IMapper mapper)
        {
            _orderRepository = repository;
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public async Task<IEnumerable<OrderModel>> GetAllOrdersAsync()
        {
            var orders = await _dbContext.Orders
                .Include(o => o.User)
                .ToListAsync();
            return orders.Select(order =>
            {
                var supplier = order.SellerId.HasValue ? _dbContext.Suppliers
                    .Where(s => s.SupplierId == order.SellerId)
                    .Select(s => new { s.SupplierName, s.AvatarUrl })
                    .FirstOrDefault() : null;
                var designer = order.SellerId.HasValue ? _dbContext.Designers
                    .Where(d => d.DesignerId == order.SellerId)
                    .Select(d => new { d.DesignerName, d.AvatarUrl })
                    .FirstOrDefault() : null;

                if (supplier == null && designer == null)
                {
                    // Fallback: suy ra từ dòng hàng đầu tiên
                    var od = _dbContext.OrderDetails
                        .Where(od => od.OrderId == order.OrderId)
                        .Select(od => new { od.SupplierId, od.DesignerId })
                        .FirstOrDefault();
                    if (od != null)
                    {
                        if (od.SupplierId.HasValue)
                        {
                            supplier = _dbContext.Suppliers
                                .Where(s => s.SupplierId == od.SupplierId)
                                .Select(s => new { s.SupplierName, s.AvatarUrl })
                                .FirstOrDefault();
                        }
                        else if (od.DesignerId.HasValue)
                        {
                            designer = _dbContext.Designers
                                .Where(d => d.DesignerId == od.DesignerId)
                                .Select(d => new { d.DesignerName, d.AvatarUrl })
                                .FirstOrDefault();
                        }
                    }
                }

                // Auto-fix legacy orders: nếu đã paid nhưng fulfillment = None, tự động set thành Delivered
                var fulfillmentStatus = order.FulfillmentStatus;
                if (order.PaymentStatus == PaymentStatus.Paid && order.FulfillmentStatus == FulfillmentStatus.None)
                {
                    fulfillmentStatus = FulfillmentStatus.Delivered;
                    // Cập nhật luôn trong database
                    order.FulfillmentStatus = FulfillmentStatus.Delivered;
                    order.Status = OrderStatus.delivered;
                    _dbContext.Orders.Update(order);
                    _dbContext.SaveChanges();
                }

                return new OrderModel
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId,
                    UserName = order.User.FullName,
                    ShippingAddress = order.ShippingAddress,
                    TotalPrice = order.TotalPrice,
                    OrderDate = order.OrderDate,
                    Status = order.Status.ToString(),
                    PaymentStatus = order.PaymentStatus.ToString(),
                    FulfillmentStatus = fulfillmentStatus.ToString(),
                    SellerType = order.SellerType,
                    SellerName = supplier?.SupplierName ?? designer?.DesignerName,
                    SellerAvatarUrl = supplier?.AvatarUrl ?? designer?.AvatarUrl,
                };
            });
        }
        public async Task<IEnumerable<OrderModel>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _dbContext.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.User)
                .ToListAsync();
            return orders.Select(order =>
            {
                var supplier = order.SellerId.HasValue ? _dbContext.Suppliers
                    .Where(s => s.SupplierId == order.SellerId)
                    .Select(s => new { s.SupplierName, s.AvatarUrl })
                    .FirstOrDefault() : null;
                var designer = order.SellerId.HasValue ? _dbContext.Designers
                    .Where(d => d.DesignerId == order.SellerId)
                    .Select(d => new { d.DesignerName, d.AvatarUrl })
                    .FirstOrDefault() : null;

                if (supplier == null && designer == null)
                {
                    // Fallback: suy ra từ dòng hàng đầu tiên
                    var od = _dbContext.OrderDetails
                        .Where(od => od.OrderId == order.OrderId)
                        .Select(od => new { od.SupplierId, od.DesignerId })
                        .FirstOrDefault();
                    if (od != null)
                    {
                        if (od.SupplierId.HasValue)
                        {
                            supplier = _dbContext.Suppliers
                                .Where(s => s.SupplierId == od.SupplierId)
                                .Select(s => new { s.SupplierName, s.AvatarUrl })
                                .FirstOrDefault();
                        }
                        else if (od.DesignerId.HasValue)
                        {
                            designer = _dbContext.Designers
                                .Where(d => d.DesignerId == od.DesignerId)
                                .Select(d => new { d.DesignerName, d.AvatarUrl })
                                .FirstOrDefault();
                        }
                    }
                }

                // Auto-fix legacy orders: nếu đã paid nhưng fulfillment = None, tự động set thành Delivered
                var fulfillmentStatus = order.FulfillmentStatus;
                if (order.PaymentStatus == PaymentStatus.Paid && order.FulfillmentStatus == FulfillmentStatus.None)
                {
                    fulfillmentStatus = FulfillmentStatus.Delivered;
                    // Cập nhật luôn trong database
                    order.FulfillmentStatus = FulfillmentStatus.Delivered;
                    order.Status = OrderStatus.delivered;
                    _dbContext.Orders.Update(order);
                    _dbContext.SaveChanges();
                }

                return new OrderModel
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId,
                    UserName = order.User.FullName,
                    ShippingAddress = order.ShippingAddress,
                    TotalPrice = order.TotalPrice,
                    OrderDate = order.OrderDate,
                    Status = order.Status.ToString(),
                    PaymentStatus = order.PaymentStatus.ToString(),
                    FulfillmentStatus = fulfillmentStatus.ToString(),
                    SellerType = order.SellerType,
                    SellerName = supplier?.SupplierName ?? designer?.DesignerName,
                    SellerAvatarUrl = supplier?.AvatarUrl ?? designer?.AvatarUrl,
                };
            });
        }
        public async Task<OrderModel?> GetOrderByIdAsync(int id)
        {
            var order = await _dbContext.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null) return null;
            var supplier = order.SellerId.HasValue ? _dbContext.Suppliers
                .Where(s => s.SupplierId == order.SellerId)
                .Select(s => new { s.SupplierName, s.AvatarUrl })
                .FirstOrDefault() : null;
            var designer = order.SellerId.HasValue ? _dbContext.Designers
                .Where(d => d.DesignerId == order.SellerId)
                .Select(d => new { d.DesignerName, d.AvatarUrl })
                .FirstOrDefault() : null;

            if (supplier == null && designer == null)
            {
                var od = _dbContext.OrderDetails
                    .Where(od => od.OrderId == order.OrderId)
                    .Select(od => new { od.SupplierId, od.DesignerId })
                    .FirstOrDefault();
                if (od != null)
                {
                    if (od.SupplierId.HasValue)
                    {
                        supplier = _dbContext.Suppliers
                            .Where(s => s.SupplierId == od.SupplierId)
                            .Select(s => new { s.SupplierName, s.AvatarUrl })
                            .FirstOrDefault();
                    }
                    else if (od.DesignerId.HasValue)
                    {
                        designer = _dbContext.Designers
                            .Where(d => d.DesignerId == od.DesignerId)
                            .Select(d => new { d.DesignerName, d.AvatarUrl })
                            .FirstOrDefault();
                    }
                }
            }

            return new OrderModel
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                UserName = order.User.FullName,
                ShippingAddress = order.ShippingAddress,
                TotalPrice = order.TotalPrice,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                PaymentStatus = order.PaymentStatus.ToString(),
                SellerType = order.SellerType,
                SellerName = supplier?.SupplierName ?? designer?.DesignerName,
                SellerAvatarUrl = supplier?.AvatarUrl ?? designer?.AvatarUrl,
            };
        }
        public async Task<int> CreateOrderAsync(int userId, CreateOrderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ShippingAddress))
                throw new ArgumentException("Địa chỉ không được để trống.");
            if (request.TotalPrice < 0)
                throw new ArgumentException("Tổng giá không hợp lệ");
            var order = _mapper.Map<Order>(request);
            order.UserId = userId;
            order.Status = OrderStatus.pending;
            order.OrderDate = DateTime.UtcNow;
            await _orderRepository.AddAsync(order);
            await _dbContext.SaveChangesAsync();
            return order.OrderId;
        }
        public async Task<bool> UpdateOrderAsync(int orderId, UpdateOrderRequest request)
        {
            var order = await _dbContext.Orders.FindAsync(orderId);
            if (order == null) return false;

            if (!string.IsNullOrWhiteSpace(request.ShippingAddress))
                order.ShippingAddress = request.ShippingAddress;

            if (request.TotalPrice.HasValue)
                order.TotalPrice = request.TotalPrice.Value;

            if (request.Status.HasValue)
                order.Status = request.Status.Value;

            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteOrderAsync(int id)
        {
            var result = _orderRepository.Remove(id);
            await _dbContext.SaveChangesAsync();
            return result != null;
        }
        #region test 
        public async Task<OrderGroup> CreateSampleOrderGroupAsync()
        {
            var customer = await _dbContext.Users.FirstAsync(u => u.UserId == 4);
            var designer = await _dbContext.Designers.FirstAsync();
            var supplier = await _dbContext.Suppliers.FirstAsync();

            var designs = await _dbContext.Designs.Take(3).ToListAsync();
            var materials = await _dbContext.Materials.Take(3).ToListAsync();

            
            var orderGroup = new OrderGroup
            {
                UserId = customer.UserId,
                CreatedAt = DateTime.UtcNow,
            };
            await _dbContext.OrderGroups.AddAsync(orderGroup);
            await _dbContext.SaveChangesAsync(); 

            // --- Designer Order ---
            var designerOrder = new Order
            {
                OrderGroupId = orderGroup.OrderGroupId,
                UserId = customer.UserId,
                ShippingAddress = "Demo Address",
                SellerId = designer.DesignerId,
                SellerType = "Designer",
                FulfillmentStatus = FulfillmentStatus.Delivered,
                PaymentStatus = PaymentStatus.Paid,
                OrderDate = DateTime.UtcNow,
                TotalPrice = designs.Sum(d => (decimal)d.SalePrice)
            };
            await _dbContext.Orders.AddAsync(designerOrder);
            await _dbContext.SaveChangesAsync(); 

            foreach (var d in designs)
            {
                var detail = new OrderDetail
                {
                    OrderId = designerOrder.OrderId,
                    DesignId = d.DesignId,
                    Type = OrderDetailType.design,
                    Quantity = 5,
                    UnitPrice = (decimal)d.SalePrice
                };
                await _dbContext.OrderDetails.AddAsync(detail);
            }
            await _dbContext.SaveChangesAsync();

            
            var supplierOrder = new Order
            {
                OrderGroupId = orderGroup.OrderGroupId,
                UserId = customer.UserId,
                ShippingAddress = "Demo Address",
                SellerId = supplier.SupplierId,
                SellerType = "Supplier",
                FulfillmentStatus = FulfillmentStatus.Delivered,
                PaymentStatus = PaymentStatus.Paid,
                OrderDate = DateTime.UtcNow,
                TotalPrice = materials.Sum(m => m.PricePerUnit)
            };
            await _dbContext.Orders.AddAsync(supplierOrder);
            await _dbContext.SaveChangesAsync(); 

            foreach (var m in materials)
            {
                var detail = new OrderDetail
                {
                    OrderId = supplierOrder.OrderId,
                    MaterialId = m.MaterialId,
                    Type = OrderDetailType.material,
                    Quantity = 2,
                    UnitPrice = m.PricePerUnit
                };
                await _dbContext.OrderDetails.AddAsync(detail);
            }
            await _dbContext.SaveChangesAsync();

            return orderGroup;
        }

        #endregion


    }
}
