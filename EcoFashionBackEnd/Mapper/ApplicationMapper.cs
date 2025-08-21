using AutoMapper;
using EcoFashionBackEnd.Common.Payloads.Requests;
using EcoFashionBackEnd.Common.Payloads.Responses;
using EcoFashionBackEnd.Dtos;
using EcoFashionBackEnd.Dtos.Design;
using EcoFashionBackEnd.Entities;

namespace EcoFashionBackEnd.Mapper
{
    public class ApplicationMapper : Profile
    {
        public ApplicationMapper()
        {
            // ---------- User ----------
            CreateMap<User, UserModel>().ReverseMap();
            CreateMap<UserRole, UserRoleModel>().ReverseMap();

            // ---------- Supplier & Designer ----------
            CreateMap<Supplier, SupplierModel>().ReverseMap();
            CreateMap<Designer, DesignerModel>().ReverseMap();

            // ---------- Design ----------
            CreateMap<Design, DesignModel>().ReverseMap();
            CreateMap<DesignModel, Design>();
            CreateMap<UpdateDesignRequest, Design>();
            CreateMap<UpdateDesignVariantRequest, DesignsVariant>();
            CreateMap<CreateDesignVariantRequest, DesignVariantModel>();
            CreateMap<DesignVariantModel, DesignsVariant>();
            CreateMap<CreateDesignVariantRequest, DesignsVariant>();
            // CreateDesign mapping
            CreateMap<CreateDesignRequest, DesignModel>();
            CreateMap<CreateDesignRequest, DesignFeatureModel>()
                .ForMember(dest => dest.ReduceWaste, opt => opt.MapFrom(src => src.Feature.ReduceWaste))
                .ForMember(dest => dest.LowImpactDyes, opt => opt.MapFrom(src => src.Feature.LowImpactDyes))
                .ForMember(dest => dest.Durable, opt => opt.MapFrom(src => src.Feature.Durable))
                .ForMember(dest => dest.EthicallyManufactured, opt => opt.MapFrom(src => src.Feature.EthicallyManufactured));
            CreateMap<CreateDesignFeatureRequest, DesignFeatureModel>();


            // ---------- Material ----------
            CreateMap<MaterialDto, DesignMaterialModel>();
            CreateMap<DesignMaterialRequest, DesignMaterialModel>();    
            CreateMap<DesignMaterialModel, DesignsMaterial>();
            CreateMap<Material, MaterialModel>();
            CreateMap<MaterialRequest, Material>();
            CreateMap<MaterialType, MaterialTypeModel>();
            CreateMap<MaterialTypeRequest, MaterialType>();
            CreateMap<DesignerMaterialInventory, DesignerMaterialInventoryModel>();
            CreateMap<CreateDesignerMaterialInventoryRequest, DesignerMaterialInventory>();
            CreateMap<UpdateDesignerMaterialInventoryRequest, DesignerMaterialInventory>();

            // ---------- Design Detail Mapping (for GET) ----------
            CreateMap<DesignDetailDto, DesignDetailResponse>();
            CreateMap<DesignFeatureDto, DesignFeatureDto>(); // no change, passthrough
            CreateMap<VariantDto, VariantDto>();
            CreateMap<MaterialDto, MaterialDto>();
            CreateMap<SustainabilityCriterionDto, SustainabilityCriterionDto>();
            CreateMap<DesignerPublicDto, DesignerPublicDto>();
            CreateMap<ItemType, ItemTypeDto>(); 
            CreateMap<ItemTypeDto, ItemType>();
            // ---------- Application ----------
            CreateMap<Application, ApplicationModel>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // --------------- Order ----------------
            CreateMap<Order, OrderModel>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<CreateOrderRequest, Order>();
            CreateMap<CreateOrderDetailRequest, OrderDetail>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => OrderDetailStatus.pending));
        }
    }
}
