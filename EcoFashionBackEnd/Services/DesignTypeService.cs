using AutoMapper;
using EcoFashionBackEnd.Dtos.Design;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoFashionBackEnd.Services
{
    public class DesignTypeService
    {
        //private readonly IRepository<DesignsType, int> _designTypeRepository;
        //private readonly IMapper _mapper;

        //public DesignTypeService(IRepository<DesignsType, int> designTypeRepository, IMapper mapper)
        //{
        //    _designTypeRepository = designTypeRepository;
        //    _mapper = mapper;
        //}

        //public async Task<List<DesignTypeDto>> GetAllDesignTypesAsync()
        //{
        //    var types = await _designTypeRepository.GetAll().ToListAsync();
        //    return _mapper.Map<List<DesignTypeDto>>(types);
        //}
    }
}
