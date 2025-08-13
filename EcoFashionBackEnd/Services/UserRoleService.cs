using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Exceptions;
using EcoFashionBackEnd.Repositories;
using System.Diagnostics;

namespace EcoFashionBackEnd.Services
{
    public class UserRoleService
    {
        private readonly IRepository<UserRole, int> _userRoleRepository;
        private readonly IMapper _mapper;

        public UserRoleService(IRepository<UserRole, int> userRoleRepository, IMapper mapper)
        {
            _userRoleRepository = userRoleRepository;
            _mapper = mapper;
        }
        public async Task<UserRoleModel> GetByName(string Name) 
        {
            var userRoleEntity = _userRoleRepository.FindByCondition(x => x.RoleName.Equals(Name)).FirstOrDefault();
            if(userRoleEntity == null) 
            {
                throw new BadRequestException("Can not find this Role");
            }
            return  _mapper.Map<UserRoleModel>(userRoleEntity);
        }
        public List<UserRoleModel> GetAll(int currentId)
        {
            var roles = _userRoleRepository.FindByCondition(x => x.RoleId > currentId).ToList();
            if (roles == null)
            {
                throw new BadRequestException("Can not find any Role");
            }
            else
            {
                return _mapper.Map<List<UserRoleModel>>(roles);
            }
        }
    }
}
