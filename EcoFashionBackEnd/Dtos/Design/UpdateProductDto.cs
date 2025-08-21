namespace EcoFashionBackEnd.Dtos.Design
{
    public class UpdateProductDto
    {
        public int DesignId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CareInstruction { get; set; }
        public DesignFeatureModel DesignFeatures { get; set; } // Sử dụng DTO
        public List<IFormFile> DesignImages { get; set; }

    }
}
