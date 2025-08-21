namespace EcoFashionBackEnd.Dtos.DesignDraft
{
    public class VariantDetailsDto
    {
        public int VariantId { get; set; }
        public string DesignName { get; set; }
        public int SizeId { get; set; }
        public string SizeName { get; set; }
        public string ColorCode { get; set; }
        public int Quantity { get; set; }
        public float Ratio { get; set; }
    }
}
