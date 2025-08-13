namespace EcoFashionBackEnd.Dtos
{
    public class MaterialTypeModel
    {
        public int TypeId { get; set; }
        public string? TypeName { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsOrganic { get; set; }
        public bool IsRecycled { get; set; }
        public string? SustainabilityNotes { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
