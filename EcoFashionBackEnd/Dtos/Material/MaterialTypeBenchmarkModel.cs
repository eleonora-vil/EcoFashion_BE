using EcoFashionBackEnd.Entities;

namespace EcoFashionBackEnd.Dtos.Material
{
    public class MaterialTypeBenchmarkModel
    {
        public int BenchmarkId { get; set; }
        public int TypeId { get; set; }
        public int CriteriaId { get; set; }
        public float Value { get; set; } // Giá trị chuẩn (benchmark)
        public MaterialType? MaterialType { get; set; }
        public SustainabilityCriteria? SustainabilityCriteria { get; set; }
        
        // Thêm các trường so sánh
        public float? ActualValue { get; set; } // Giá trị thực tế của material
        public float? ImprovementPercentage { get; set; } // Phần trăm cải thiện
        public string? ImprovementStatus { get; set; } // Trạng thái: "Tốt hơn", "Kém hơn", "Bằng"
        public string? ImprovementColor { get; set; } // Màu sắc: "success", "error", "warning"
    }
}
