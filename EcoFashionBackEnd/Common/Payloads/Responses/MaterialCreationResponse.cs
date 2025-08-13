namespace EcoFashionBackEnd.Common.Payloads.Responses
{
    public class MaterialCreationResponse
    {
        public int MaterialId { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string MaterialTypeName { get; set; } = "";
        public decimal RecycledPercentage { get; set; }
        public int QuantityAvailable { get; set; }
        public decimal PricePerUnit { get; set; }
        public string? DocumentationUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }

        // Sustainability fields
        public decimal CarbonFootprint { get; set; }
        public decimal WaterUsage { get; set; }
        public decimal WasteDiverted { get; set; }
        public bool HasOrganicCertification { get; set; }
        public string? OrganicCertificationType { get; set; }
        public string? ProductionCountry { get; set; }
        public string? ProductionRegion { get; set; }
        public string? ManufacturingProcess { get; set; }
        public bool IsCertified { get; set; }
        public string? CertificationDetails { get; set; }
        public string? QualityStandards { get; set; }

        // Sustainability score
        public int SustainabilityScore { get; set; }
        public string SustainabilityLevel { get; set; } = "";
        public string SustainabilityColor { get; set; } = "";

        // Market analysis
        public string MarketPosition { get; set; } = "";
        public string CompetitiveAdvantage { get; set; } = "";

        // Criterion scores
        public List<CriterionScoreDetail> CriterionScores { get; set; } = new();

        // Summary
        public SustainabilitySummary Summary { get; set; } = new();

        public class CriterionScoreDetail
        {
            public string CriterionName { get; set; } = "";
            public decimal ActualValue { get; set; }
            public decimal BenchmarkValue { get; set; }
            public string Unit { get; set; } = "";
            public decimal Score { get; set; }
            public string Status { get; set; } = "";
        }

        public class SustainabilitySummary
        {
            public int TotalCriteria { get; set; }
            public int ExcellentCriteria { get; set; }
            public int GoodCriteria { get; set; }
            public int AverageCriteria { get; set; }
            public int NeedsImprovementCriteria { get; set; }
            public string Recommendation { get; set; } = "";
        }
    }
} 