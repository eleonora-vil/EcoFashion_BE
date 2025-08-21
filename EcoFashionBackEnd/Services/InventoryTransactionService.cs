using AutoMapper;
using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Dtos.TransactionInventory;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Cms;

namespace EcoFashionBackEnd.Services
{
    public class InventoryTransactionService
    {
        #region injection
        private readonly IRepository<ProductInventoryTransaction, int> _productTransactionRepository;
        private readonly IRepository<MaterialInventoryTransaction, int> _materialTransactionRepository;
        private readonly IRepository<Design, int> _designRepository;
        private readonly IRepository<Warehouse, int> _warehouseRepository;
        public InventoryTransactionService(
            IRepository<ProductInventoryTransaction, int> productTransactionRepository,
            IRepository<MaterialInventoryTransaction, int> materialTransactionRepository,
            IRepository<Design, int> designRepository,
            IRepository<Warehouse, int> warehouseRepository
            )
           
        {
            _productTransactionRepository = productTransactionRepository;
            _materialTransactionRepository = materialTransactionRepository;
            _designRepository = designRepository;
            _warehouseRepository = warehouseRepository;
            
        }
        #endregion

        //get all ProductInventoryTransaction   
        public async Task<List<ProductInventoryTransactionDto>> GetAllProductTransactionsAsync()
        {
            var transactions = await _productTransactionRepository
                .GetAll().AsNoTracking()
                .Include(t => t.ProductInventory)
                .Include(t => t.User)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            return transactions.Select(t => new ProductInventoryTransactionDto
            {
                TransactionId = t.TransactionId,
                InventoryId = t.InventoryId,
                ProductName = t.ProductInventory?.Product.SKU ?? string.Empty,
                PerformedByUserId = t.PerformedByUserId,
                PerformedByUserName = t.User?.FullName,
                QuantityChanged = t.QuantityChanged,
                BeforeQty = t.BeforeQty,
                AfterQty = t.AfterQty,
                TransactionDate = t.TransactionDate,
                TransactionType = t.TransactionType,
                Notes = t.Notes
            }).ToList();
        }


        public async Task<List<MaterialInventoryTransactionDto>> GetAllMaterialTransactionsAsync()
        {
            var transactions = await _materialTransactionRepository
                .GetAll().AsNoTracking()
                .Include(t => t.MaterialInventory)
                .Include(t => t.User)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            return transactions.Select(t => new MaterialInventoryTransactionDto
            {
                TransactionId = t.TransactionId,
                InventoryId = t.InventoryId,
                MaterialName = t.MaterialInventory?.Material.Name ?? string.Empty,
                PerformedByUserId = t.PerformedByUserId,
                PerformedByUserName = t.User?.FullName,
                QuantityChanged = t.QuantityChanged,
                BeforeQty = t.BeforeQty,
                AfterQty = t.AfterQty,
                TransactionType = t.TransactionType,
                Notes = t.Notes,
                TransactionDate = t.TransactionDate
            }).ToList();
        }


        public async Task<List<InventoryTransactionDto>> GetAllTransactionsAsync()
        {
            var productTx = await _productTransactionRepository.GetAll().AsNoTracking()
                .Include(t => t.ProductInventory)
                    .ThenInclude(p => p.Product)
                .Include(t => t.User)
                .Select(t => new InventoryTransactionDto
                {
                    TransactionId = t.TransactionId,
                    InventoryType = "Product",
                    InventoryId = t.InventoryId,
                    ItemName = t.ProductInventory.Product.SKU ?? string.Empty,
                    PerformedByUserId = t.PerformedByUserId,
                    PerformedByUserName = t.User.FullName,
                    QuantityChanged = t.QuantityChanged,
                    BeforeQty = t.BeforeQty,
                    AfterQty = t.AfterQty,
                    TransactionType = t.TransactionType,
                    Notes = t.Notes,
                    TransactionDate = t.TransactionDate
                })
                .ToListAsync();

            var materialTx = await _materialTransactionRepository.GetAll().AsNoTracking()
                .Include(t => t.MaterialInventory)
                    .ThenInclude(p => p.Material)
                .Include(t => t.User)
                .Select(t => new InventoryTransactionDto
                {
                    TransactionId = t.TransactionId,
                    InventoryType = "Material",
                    InventoryId = t.InventoryId,
                    ItemName = t.MaterialInventory.Material.Name,
                    PerformedByUserId = t.PerformedByUserId,
                    PerformedByUserName = t.User.FullName,
                    QuantityChanged = t.QuantityChanged,
                    BeforeQty = t.BeforeQty,
                    AfterQty = t.AfterQty,
                    TransactionType = t.TransactionType,
                    Notes = t.Notes,
                    TransactionDate = t.TransactionDate
                })
                .ToListAsync();

            return productTx.Concat(materialTx)
                            .OrderByDescending(t => t.TransactionDate)
                            .ToList();
        }

        public async Task<List<InventoryTransactionDto>> GetTransactionsByDesignerAsync(Guid designerId)
        {
            var productTx = await _productTransactionRepository.GetAll().AsNoTracking()
                .Where(t => t.ProductInventory.Warehouse.DesignerId == designerId)
                .Include(t => t.ProductInventory)
                    .ThenInclude(p => p.Product)
                .Include(t => t.User)
                .Select(t => new InventoryTransactionDto
                {
                    TransactionId = t.TransactionId,
                    InventoryType = "Product",
                    InventoryId = t.InventoryId,
                    ItemName = t.ProductInventory.Product.SKU ?? string.Empty,
                    PerformedByUserId = t.PerformedByUserId,
                    PerformedByUserName = t.User.FullName,
                    QuantityChanged = t.QuantityChanged,
                    BeforeQty = t.BeforeQty,
                    AfterQty = t.AfterQty,
                    TransactionType = t.TransactionType,
                    Notes = t.Notes,
                    TransactionDate = t.TransactionDate
                })
                .ToListAsync();

            var materialTx = await _materialTransactionRepository.GetAll().AsNoTracking()
                .Where(t => t.MaterialInventory.Warehouse.DesignerId == designerId)
                .Include(t => t.MaterialInventory)
                    .ThenInclude(m => m.Material)
                .Include(t => t.User)
                .Select(t => new InventoryTransactionDto
                {
                    TransactionId = t.TransactionId,
                    InventoryType = "Material",
                    InventoryId = t.InventoryId,
                    ItemName = t.MaterialInventory.Material.Name ,
                    PerformedByUserId = t.PerformedByUserId,
                    PerformedByUserName = t.User.FullName,
                    QuantityChanged = t.QuantityChanged,
                    BeforeQty = t.BeforeQty,
                    AfterQty = t.AfterQty,
                    TransactionType = t.TransactionType,
                    Notes = t.Notes,
                    TransactionDate = t.TransactionDate
                })
                .ToListAsync();

            return productTx.Concat(materialTx)
                            .OrderByDescending(t => t.TransactionDate)
                            .ToList();
        }




    }
}