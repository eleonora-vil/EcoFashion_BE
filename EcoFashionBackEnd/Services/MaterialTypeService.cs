using AutoMapper;
using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Dtos.Material;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using EcoFashionBackEnd.Common.Payloads.Requests;
using Microsoft.EntityFrameworkCore;
using EcoFashionBackEnd.Common;

namespace EcoFashionBackEnd.Services
{
    public class MaterialTypeService
    {
        private readonly IRepository<MaterialType, int> _materialTypeRepository;

        private readonly IMapper _mapper;
        private readonly AppDbContext _appDbContext;
        public MaterialTypeService(IRepository<MaterialType, int> materialTypeRepository,
            IMapper mapper,
            AppDbContext dbContext)
        {
            _materialTypeRepository = materialTypeRepository;
            _mapper = mapper;
            _appDbContext = dbContext;
        }
        // Get material type by id
        public async Task<MaterialTypeModel?> GetMaterialTypeByIdAsync(int id)
        {
            var materialType = await _materialTypeRepository.GetByIdAsync(id);
            if (materialType == null)
                return null;
            return _mapper.Map<MaterialTypeModel>(materialType);
        }
        // Get all material types
        public async Task<IEnumerable<MaterialTypeModel>> GetAllMaterialTypesAsync()
        {
            var materialTypes = await _materialTypeRepository.GetAll()
                                      .ToListAsync();
            return _mapper.Map<List<MaterialTypeModel>>(materialTypes);
        }

        // Create material type
        public async Task<MaterialTypeModel> CreateMaterialTypeAsync(MaterialTypeRequest request)
        {
            var materialType = new MaterialType
            {
                TypeName = request.TypeName,
                ImageUrl = "https://res.cloudinary.com/dguz8gz6s/image/upload/v1754826927/organic-coton_b9zo4y.webp"
            };
            await _materialTypeRepository.AddAsync(materialType);
            await _appDbContext.SaveChangesAsync();
            return _mapper.Map<MaterialTypeModel>(materialType);
        }
        // Update material type
        public async Task<MaterialTypeModel?> UpdateMaterialTypeAsync(int id, MaterialTypeRequest model)
        {
            var materialType = await _materialTypeRepository.GetByIdAsync(id);
            if (materialType == null)
                return null;

            _mapper.Map(model, materialType);
            _materialTypeRepository.Update(materialType);
            await _appDbContext.SaveChangesAsync();

            return _mapper.Map<MaterialTypeModel>(materialType);
        }
        // Delete material type
        public async Task<bool> DeleteMaterialTypeAsync(int id)
        {
            var materialType = await _materialTypeRepository.GetByIdAsync(id);
            if (materialType == null)
                return false;

            _materialTypeRepository.Remove(id);
            await _appDbContext.SaveChangesAsync();
            return true;
        }

        // Benchmarks for a specific material type admin only 
        public async Task<ApiResult<List<MaterialTypeBenchmarkModel>>> GetBenchmarksByTypeAsync(int typeId)
        {
            try
            {
                var benchmarks = await _appDbContext.MaterialTypesBenchmarks
                    .Include(b => b.MaterialType)
                    .Include(b => b.SustainabilityCriteria)
                    .Where(b => b.TypeId == typeId)
                    .Select(b => new MaterialTypeBenchmarkModel
                    {
                        BenchmarkId = b.BenchmarkId,
                        TypeId = b.TypeId,
                        CriteriaId = b.CriteriaId,
                        Value = (float)b.Value,
                        MaterialType = b.MaterialType,
                        SustainabilityCriteria = b.SustainabilityCriteria
                    })
                    .ToListAsync();

                return ApiResult<List<MaterialTypeBenchmarkModel>>.Succeed(benchmarks);
            }
            catch (Exception ex)
            {
                return ApiResult<List<MaterialTypeBenchmarkModel>>.Fail(ex.Message);
            }
        }
    }
}
