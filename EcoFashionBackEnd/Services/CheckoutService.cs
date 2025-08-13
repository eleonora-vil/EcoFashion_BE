using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Services
{
    public class CheckoutService
    {
        private readonly IRepository<Order, int> _orderRepository;
        private readonly IRepository<OrderDetail, int> _orderDetailRepository;
        private readonly IRepository<OrderGroup, Guid> _orderGroupRepository;

        public CheckoutService(
            IRepository<Order, int> orderRepository,
            IRepository<OrderDetail, int> orderDetailRepository,
            IRepository<OrderGroup, Guid> orderGroupRepository
        )
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _orderGroupRepository = orderGroupRepository;
        }

        // Tạo OrderGroup + nhiều Order theo Seller, kèm OrderDetail
        public async Task<CreateSessionResponse> CreateSessionAsync(int userId, CreateSessionRequest request)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(request.HoldMinutes <= 0 ? 30 : request.HoldMinutes);

            // Group items theo SellerId + SellerType
            var groups = request.Items
                .GroupBy(i => new { i.SellerId, i.SellerType })
                .ToList();

            var orderGroup = new OrderGroup
            {
                OrderGroupId = Guid.NewGuid(),
                UserId = userId,
                Status = OrderGroupStatus.InProgress,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
            };
            await _orderGroupRepository.AddAsync(orderGroup);
            await _orderGroupRepository.Commit();

            var response = new CreateSessionResponse
            {
                OrderGroupId = orderGroup.OrderGroupId,
                ExpiresAt = expiresAt
            };

            foreach (var group in groups)
            {
                var subtotal = group.Sum(i => i.UnitPrice * i.Quantity);
                var shipping = 0m;
                var discount = 0m;
                var total = subtotal + shipping - discount;

                var order = new Order
                {
                    UserId = userId,
                    OrderGroupId = orderGroup.OrderGroupId,
                    ShippingAddress = request.ShippingAddress,
                    Subtotal = subtotal,
                    ShippingFee = shipping,
                    Discount = discount,
                    TotalPrice = total,
                    Status = OrderStatus.pending,
                    PaymentStatus = PaymentStatus.Pending,
                    FulfillmentStatus = FulfillmentStatus.None,
                    SellerType = group.Key.SellerType,
                    SellerId = group.Key.SellerId,
                    ExpiresAt = expiresAt,
                    OrderDate = DateTime.UtcNow,
                    CreateAt = DateTime.UtcNow
                };

                await _orderRepository.AddAsync(order);
                await _orderRepository.Commit();

                // Add details
                foreach (var item in group)
                {
                    var detail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        MaterialId = item.ItemType.Equals("material", StringComparison.OrdinalIgnoreCase) ? item.MaterialId : null,
                        DesignId = item.ItemType.Equals("design", StringComparison.OrdinalIgnoreCase) ? item.DesignId : null,
                        SupplierId = item.SellerType.Equals("Supplier", StringComparison.OrdinalIgnoreCase) ? item.SellerId : null,
                        DesignerId = item.SellerType.Equals("Designer", StringComparison.OrdinalIgnoreCase) ? item.SellerId : null,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Type = item.ItemType.Equals("material", StringComparison.OrdinalIgnoreCase) ? OrderDetailType.material : OrderDetailType.design,
                        Status = OrderDetailStatus.pending
                    };
                    await _orderDetailRepository.AddAsync(detail);
                }
                await _orderDetailRepository.Commit();

                response.Orders.Add(new CheckoutOrderDto
                {
                    OrderId = order.OrderId,
                    SellerType = order.SellerType ?? string.Empty,
                    SellerId = order.SellerId,
                    Subtotal = order.Subtotal,
                    ShippingFee = order.ShippingFee,
                    Discount = order.Discount,
                    TotalAmount = order.TotalPrice,
                    PaymentStatus = order.PaymentStatus.ToString()
                });
            }

            // Update counts on group
            orderGroup.TotalOrders = response.Orders.Count;
            orderGroup.CompletedOrders = 0;
            _orderGroupRepository.Update(orderGroup);
            await _orderGroupRepository.Commit();

            return response;
        }
    }
}


