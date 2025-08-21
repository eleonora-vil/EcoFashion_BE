using EcoFashionBackEnd.Common.Payloads.Requests.Variant;
using EcoFashionBackEnd.Dtos.DesignDraft;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;
using EcoFashionBackEnd.Helpers;

namespace EcoFashionBackEnd.Services
{
    public class DesignsVariantService
    {
        private readonly IRepository<DesignsVariant, int> _designVariantRepository;
        private readonly IRepository<Design, int> _designRepository;
        private readonly IRepository<Size, int> _sizeRepository;


        public DesignsVariantService(
            IRepository<DesignsVariant, int> designVariantRepository,
            IRepository<Design, int> designRepository,
            IRepository<Size, int> sizeRepository)
        {
            _designVariantRepository = designVariantRepository;
            _designRepository = designRepository;
            _sizeRepository = sizeRepository;
        }

        public async Task<List<VariantDetailsDto>> GetVariantsByDesignIdAsync(int designId)
        {
            return await _designVariantRepository
                .GetAll()
                .AsNoTracking()
                .Where(v => v.DesignId == designId)
                .Include(v => v.Design)
                    .ThenInclude(d => d.ItemTypes)
                .Include(v => v.Size)
                .Select(v => new VariantDetailsDto
                {
                    VariantId = v.Id,
                    DesignName = v.Design.Name,
                    SizeName = v.Size.SizeName,
                    ColorCode = v.ColorCode,
                    Quantity = v.Quantity,
                    SizeId = v.SizeId,
                    Ratio = v.Design.ItemTypes.TypeSizeRatios
                        .FirstOrDefault(r => r.SizeId == v.SizeId)
                        .Ratio
                })
                .ToListAsync();
        }

        public async Task<VariantDetailsDto?> GetVariantByIdAsync(int variantId)
        {
            return await _designVariantRepository
                .GetAll()
                .AsNoTracking()
                .Where(v => v.Id == variantId)
                .Include(v => v.Design)
                .Include(v => v.Size)
                .Select(v => new VariantDetailsDto
                {
                    VariantId = v.Id,
                    DesignName = v.Design.Name,
                    SizeName = v.Size.SizeName,
                    ColorCode = v.ColorCode,
                    Quantity = v.Quantity,
                    // Find the correct ratio based on the variant's SizeId and the design's ItemTypeId.
                    Ratio = v.Design.ItemTypes.TypeSizeRatios
                        .FirstOrDefault(r => r.SizeId == v.SizeId)
                        .Ratio
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CreateVariantAsync(int designId, List<DesignsVariantCreateRequest> requests)
        {
            if (requests == null || requests.Count == 0)
                throw new ArgumentException("Variant list cannot be empty.");

            var design = await _designRepository.GetByIdAsync(designId);
            if (design == null)
                throw new KeyNotFoundException($"Design {designId} not found.");

            // Check từng SizeId tồn tại
            foreach (var r in requests)
            {
                var size = await _sizeRepository.GetByIdAsync(r.SizeId);
                if (size == null)
                    throw new Exception($"Size {r.SizeId} không tồn tại.");
            }

            // Lấy danh sách variants hiện có để kiểm tra trùng
            var existingVariants = await _designVariantRepository
                .GetAll()
                .Where(v => v.DesignId == designId)
                .ToListAsync();

            var newVariants = new List<DesignsVariant>();

            foreach (var r in requests)
            {
                var existing = existingVariants
                    .FirstOrDefault(v => v.ColorCode == r.ColorCode && v.SizeId == r.SizeId);

                if (existing != null)
                {
                    // Nếu trùng thì cộng quantity
                    existing.Quantity += r.Quantity;
                    _designVariantRepository.Update(existing);
                }
                else
                {
                    // Nếu không trùng thì tạo mới
                    newVariants.Add(new DesignsVariant
                    {
                        DesignId = designId,
                        ColorCode = r.ColorCode,
                        SizeId = r.SizeId,
                        Quantity = r.Quantity
                    });
                }
            }

            if (newVariants.Any())
                await _designVariantRepository.AddRangeAsync(newVariants);

            await _designVariantRepository.Commit();
            return true;
        }



        public async Task<bool> UpdateVariantAsync(int variantId, DesignsVariantUpdateRequest request)
        {
            var variant = await _designVariantRepository.GetByIdAsync(variantId);
            if (variant == null) return false;

            var size = await _sizeRepository.GetByIdAsync(request.SizeId);
            if (size == null) throw new Exception("Size không tồn tại.");

            variant.SizeId = request.SizeId;
            variant.ColorCode = request.ColorCode;
            variant.Quantity = request.Quantity;

            _designVariantRepository.Update(variant);
            await _designVariantRepository.Commit();
            return true;
        }

        public async Task<bool> UpdateVariantsAsync(int designId, List<DesignsVariantUpdateRequest> newVariants)
        {
            if (newVariants == null)
                throw new ArgumentException("Variant list cannot be null.");

            var design = await _designRepository.GetByIdAsync(designId);
            if (design == null)
                throw new KeyNotFoundException($"Design {designId} not found.");

            // Check size tồn tại
            foreach (var v in newVariants)
            {
                var size = await _sizeRepository.GetByIdAsync(v.SizeId);
                if (size == null)
                    throw new Exception($"Size {v.SizeId} không tồn tại.");
            }

            // Lấy hết variant cũ
            var existingVariants = await _designVariantRepository.GetAll()
                .Where(v => v.DesignId == designId)
                .ToListAsync();

            // Xóa hết variant cũ
            if (existingVariants.Any())
            {
                _designVariantRepository.RemoveRange(existingVariants);
            }

            // Thêm variant mới
            var variants = newVariants.Select(v => new DesignsVariant
            {
                DesignId = designId,
                SizeId = v.SizeId,
                ColorCode = v.ColorCode,
                Quantity = v.Quantity
            }).ToList();

            await _designVariantRepository.AddRangeAsync(variants);

            // Commit
            await _designVariantRepository.Commit();

            return true;
        }


        public async Task<bool> DeleteVariantAsync(int variantId)
        {
            var variant = await _designVariantRepository.GetByIdAsync(variantId);
            if (variant == null) return false;

            //_designVariantRepository.Remove();
            await _designVariantRepository.Commit();
            return true;
        }
    }

}
