namespace EcoFashionBackEnd.Dtos.Design
{
    public class MaterialDto
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; }
        public decimal MeterUsed { get; set; }
        public string Certificates { get; set;}
        public string Description { get; set; }

        //public int MaterialId { get; set; }
        //public double PersentageUsed { get; set; }
        //public double MeterUsed { get; set; }
        //public string? MaterialDescription { get; set; }

        //public string? MaterialName { get; set; }
        //public string? MaterialTypeName { get; set; }
        //public decimal? CarbonFootprint { get; set; }
        //public string? CarbonFootprintUnit { get; set; } 
        //public decimal? WaterUsage { get; set; }
        //public string? WaterUsageUnit { get; set; } 
        //public decimal? WasteDiverted { get; set; }
        //public string? WasteDivertedUnit { get; set; }
        //public string? CertificationDetails { get; set; }
    }
}
