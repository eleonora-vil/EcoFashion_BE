namespace EcoFashionBackEnd.Common.Payloads.Requests
{
    public class CreateDesignVariantRequest
    {
        public int DesignId { get; set; }
        public int SizeId { get; set; }
        public string  Color { get; set; }
        public int Quantity { get; set; }

    
    }

}
