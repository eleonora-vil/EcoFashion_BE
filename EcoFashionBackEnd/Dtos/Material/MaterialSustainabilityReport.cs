namespace EcoFashionBackEnd.Dtos.Material
{
    public class MaterialSustainabilityReport
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public string? MaterialTypeName { get; set; }
        public decimal RecycledPercentage { get; set; }
        public int OverallSustainabilityScore { get; set; }
        public string SustainabilityLevel { get; set; } = string.Empty; // Xuất sắc, Tốt, Trung bình, Cần cải thiện
        public string LevelColor { get; set; } = string.Empty; // green, yellow, orange, red
        public List<CriterionCalculationDetail> CriterionDetails { get; set; } = new();
        public SustainabilitySummary Summary { get; set; } = new();
    }

    public class CriterionCalculationDetail
    {
        public string CriterionName { get; set; } = string.Empty;
        public decimal ActualValue { get; set; }
        public decimal BenchmarkValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal Score { get; set; } // Điểm thành phần cho tiêu chí này (0-100)
        public string Status { get; set; } = string.Empty; // Excellent, Good, Average, Needs Improvement
        public string Explanation { get; set; } = string.Empty;
    }

    public class SustainabilitySummary
    {
        public int TotalCriteria { get; set; }
        public int ExcellentCriteria { get; set; }
        public int GoodCriteria { get; set; }
        public int AverageCriteria { get; set; }
        public int NeedsImprovementCriteria { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }
}
