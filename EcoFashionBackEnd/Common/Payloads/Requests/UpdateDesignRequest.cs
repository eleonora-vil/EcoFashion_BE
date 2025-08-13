namespace EcoFashionBackEnd.Common.Payloads.Requests
{
    public class UpdateDesignRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        //public float RecycledPercentage { get; set; }
        //public string? CareInstructions { get; set; }
        //public decimal Price { get; set; }
        //public bool Gender { get; set; }
        //public int ProductScore { get; set; }
        public string? Status { get; set; }

        public List<UpdateDesignVariantRequest> Variants { get; set; } = new();
    }
}
