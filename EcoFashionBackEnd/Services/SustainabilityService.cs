using EcoFashionBackEnd.Entities;
using Microsoft.EntityFrameworkCore;
using EcoFashionBackEnd.Dtos.Material;

namespace EcoFashionBackEnd.Services
{
    public class SustainabilityService
    {
        private readonly AppDbContext _dbContext;

        public SustainabilityService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Tính điểm bền vững cho một material
        /// </summary>
        public async Task<MaterialSustainabilityReport?> CalculateMaterialSustainabilityScore(int materialId)
        {
            var material = await _dbContext.Materials
                .Include(m => m.MaterialType)
                .FirstOrDefaultAsync(m => m.MaterialId == materialId);

            if (material == null)
                return null;

            var actualValues = await _dbContext.MaterialSustainabilities
                .Where(ms => ms.MaterialId == materialId)
                .Include(ms => ms.SustainabilityCriterion)
                .ToListAsync();

            var benchmarks = await _dbContext.MaterialTypesBenchmarks
                .Where(mtb => mtb.TypeId == material.TypeId)
                .ToListAsync();

            var criteria = await _dbContext.SustainabilityCriterias.ToListAsync();

            var criterionDetails = new List<CriterionCalculationDetail>();
            decimal weightedScoreSum = 0;
            decimal totalWeight = 0;

            // Define the 5 current sustainability criteria with equal weights (20% each)
            var currentCriteria = new[] { "Carbon Footprint", "Water Usage", "Waste Diverted", "Organic Certification", "Transport" };

            foreach (var criterion in criteria)
            {
                // Only process the 5 current criteria
                if (!currentCriteria.Contains(criterion.Name))
                    continue;

                var (score, status, explanation, actualValue, benchmarkValue) = CalculateCriterionScore(
                    material, actualValues, benchmarks, criterion);

                var criterionDetail = new CriterionCalculationDetail
                {
                    CriterionName = criterion.Name,
                    ActualValue = actualValue,
                    BenchmarkValue = benchmarkValue,
                    Unit = criterion.Unit,
                    Score = score,
                    Status = status,
                    Explanation = explanation
                };

                criterionDetails.Add(criterionDetail);

                // Set equal weight (20%) for all 5 criteria
                decimal equalWeight = 20.0m;
                weightedScoreSum += score * equalWeight;
                totalWeight += equalWeight;
            }

            var overallScore = totalWeight > 0 ? weightedScoreSum / totalWeight : 0;
            var sustainabilityLevel = GetSustainabilityLevel(overallScore);
            var levelColor = GetLevelColor(sustainabilityLevel);

            var excellentCount = criterionDetails.Count(c => c.Status == "Excellent");
            var goodCount = criterionDetails.Count(c => c.Status == "Good");
            var needsImprovementCount = criterionDetails.Count(c => c.Status == "Needs Improvement");

            return new MaterialSustainabilityReport
            {
                MaterialId = materialId,
                OverallSustainabilityScore = (int)overallScore,
                SustainabilityLevel = sustainabilityLevel,
                LevelColor = levelColor,
                CriterionDetails = criterionDetails,
                Summary = new SustainabilitySummary
                {
                    TotalCriteria = criterionDetails.Count,
                    ExcellentCriteria = excellentCount,
                    GoodCriteria = goodCount,
                    AverageCriteria = criterionDetails.Count(c => c.Status == "Average"),
                    NeedsImprovementCriteria = needsImprovementCount,
                    Recommendation = GetRecommendation(overallScore)
                }
            };
        }

        /// <summary>
        /// Tính điểm vận chuyển dựa trên khoảng cách (15%) và phương thức (5%)
        /// </summary>
        private decimal CalculateTransportScore(Material material)
        {
            if (material.TransportDistance == null || material.TransportMethod == null)
                return 0;

            var distance = material.TransportDistance.Value;
            var method = material.TransportMethod.ToLower();

            // Calculate distance score (15% of total sustainability)
            decimal distanceScore = 100;
            if (distance > 5000) distanceScore = 20;      // >5000km: 20%
            else if (distance > 2000) distanceScore = 40;  // 2000-5000km: 40%
            else if (distance > 1000) distanceScore = 60;  // 1000-2000km: 60%
            else if (distance > 500) distanceScore = 80;   // 500-1000km: 80%
            else if (distance > 100) distanceScore = 90;   // 100-500km: 90%

            // Calculate method score (5% of total sustainability)
            decimal methodScore = method switch
            {
                "sea" => 80,    // Sea transport: 80%
                "rail" => 90,    // Rail transport: 90%
                "land" => 70,    // Land transport: 70%
                "air" => 30,     // Air transport: 30% (worst)
                _ => 100         // Default: 100%
            };

            // Calculate weighted scores: (distance * 15%) + (method * 5%)
            decimal distanceContribution = (distanceScore * 15) / 100;  // 15% weight
            decimal methodContribution = (methodScore * 5) / 100;       // 5% weight

            return distanceContribution + methodContribution;
        }

        /// <summary>
        /// Tính điểm chứng nhận dựa trên loại chứng nhận
        /// </summary>
        private decimal CalculateCertificationScore(Material material)
        {
            if (string.IsNullOrEmpty(material.CertificationDetails))
                return 0;

            var certifications = material.CertificationDetails.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim().ToUpper())
                .ToList();

            decimal totalScore = 0;

            foreach (var cert in certifications)
            {
                totalScore += cert switch
                {
                    "GOTS" => 20,      // Global Organic Textile Standard
                    "OCS" => 15,        // Organic Content Standard
                    "GRS" => 10,        // Global Recycled Standard
                    "OEKO-TEX" => 12,   // OEKO-TEX Standard 100
                    "USDA ORGANIC" => 18, // USDA Organic
                    "EU ECOLABEL" => 15,  // EU Ecolabel
                    "RWS" => 12,        // Responsible Wool Standard
                    _ => 5              // Other certifications
                };
            }

            return Math.Min(totalScore, 100); // Cap at 100%
        }

        /// <summary>
        /// Tính điểm cho từng tiêu chí
        /// </summary>
        private (decimal score, string status, string explanation, decimal actualValue, decimal benchmarkValue) 
            CalculateCriterionScore(Material material, List<MaterialSustainability> actualValues, 
                List<MaterialTypeBenchmark> benchmarks, SustainabilityCriteria criterion)
        {
            decimal actualValue = 0;
            decimal benchmarkValue = 0;
            decimal score = 0;
            string status = "N/A";
            string explanation = "";

            // Lấy giá trị thực tế
            var actualMs = actualValues.FirstOrDefault(ms => ms.CriterionId == criterion.CriterionId);
            if (actualMs != null)
                actualValue = actualMs.Value;
            else if (criterion.Name == "Organic Certification")
            {
                // Fallback: nếu chưa có bản ghi MaterialSustainability cho tiêu chí Organic,
                // xác định từ CertificationDetails của material
                actualValue = HasOrganicCertification(material) ? 1 : 0;
            }

            // Lấy giá trị benchmark
            benchmarkValue = benchmarks.FirstOrDefault(b => b.CriteriaId == criterion.CriterionId)?.Value ?? 0;

            // Tính điểm dựa trên loại tiêu chí
            switch (criterion.Name)
            {
                case "Carbon Footprint":
                case "Water Usage":
                    if (benchmarkValue > 0)
                    {
                        score = Math.Max(0, (1 - (actualValue / benchmarkValue)) * 100);
                        if (actualValue <= benchmarkValue * 0.8m)
                            status = "Excellent";
                        else if (actualValue <= benchmarkValue)
                            status = "Good";
                        else if (actualValue <= benchmarkValue * 1.2m)
                            status = "Average";
                        else
                            status = "Needs Improvement";
                    }
                    else
                    {
                        score = actualValue == 0 ? 100 : 50;
                        status = "No Benchmark";
                    }
                    explanation = $"Thực tế: {actualValue} {criterion.Unit} (chuẩn: {benchmarkValue} {criterion.Unit})";
                    break;

                case "Waste Diverted":
                    if (benchmarkValue > 0)
                    {
                        score = (actualValue / benchmarkValue) * 100;
                        if (score > 100) score = 100;
                        if (actualValue >= benchmarkValue * 1.2m)
                            status = "Excellent";
                        else if (actualValue >= benchmarkValue)
                            status = "Good";
                        else if (actualValue >= benchmarkValue * 0.8m)
                            status = "Average";
                        else
                            status = "Needs Improvement";
                    }
                    else
                    {
                        score = actualValue > 0 ? 100 : 0;
                        status = "No Benchmark";
                    }
                    explanation = $"Thực tế: {actualValue} {criterion.Unit} (chuẩn: {benchmarkValue} {criterion.Unit})";
                    break;

                case "Organic Certification":
                    // Logic mới: Xem xét cả benchmark và actual value
                    if (benchmarkValue > 0)
                    {
                        // Loại vải yêu cầu chứng chỉ hữu cơ
                        if (actualValue > 0)
                        {
                            score = 100; // Có chứng chỉ khi yêu cầu
                            status = "Certified";
                            explanation = "Có chứng nhận hữu cơ (yêu cầu)";
                        }
                        else
                        {
                            score = 0; // Không có chứng chỉ khi yêu cầu
                            status = "Not Certified";
                            explanation = "Không có chứng nhận hữu cơ (yêu cầu)";
                        }
                    }
                    else
                    {
                        // Loại vải không yêu cầu chứng chỉ hữu cơ
                        if (actualValue > 0)
                        {
                            score = 100; // Có chứng chỉ khi không yêu cầu = bonus
                            status = "Certified (Bonus)";
                            explanation = "Có chứng nhận hữu cơ (không yêu cầu - bonus)";
                        }
                        else
                        {
                            score = 100; // Không có chứng chỉ khi không yêu cầu = đạt chuẩn
                            status = "Not Required";
                            explanation = "Không yêu cầu chứng nhận hữu cơ";
                        }
                    }
                    break;

                case "Transport":
                    score = CalculateTransportScore(material);
                    if (score >= 80)
                        status = "Excellent";
                    else if (score >= 60)
                        status = "Good";
                    else if (score >= 40)
                        status = "Average";
                    else
                        status = "Needs Improvement";
                    explanation = $"Điểm vận chuyển: {score:F1}% (khoảng cách: {material.TransportDistance}km, phương thức: {material.TransportMethod})";
                    break;

                default:
                    score = 0;
                    status = "Unknown";
                    explanation = $"Tiêu chí không xác định: {criterion.Name}";
                    break;
            }

            return (score, status, explanation, actualValue, benchmarkValue);
        }

        private bool HasOrganicCertification(Material material)
        {
            if (material == null || string.IsNullOrWhiteSpace(material.CertificationDetails))
                return false;

            var details = material.CertificationDetails.ToUpperInvariant();
            // Recognized: GOTS, OEKO-TEX Standard 100, GRS, OCS
            return details.Contains("GOTS") || details.Contains("OEKO-TEX") || details.Contains("GRS") || details.Contains("OCS");
        }

        /// <summary>
        /// Tính điểm cho nhiều materials cùng lúc (batch processing)
        /// </summary>
        public async Task<Dictionary<int, MaterialSustainabilityReport>> CalculateMaterialsSustainabilityScores(List<int> materialIds)
        {
            var result = new Dictionary<int, MaterialSustainabilityReport>();
            
            if (materialIds == null || materialIds.Count == 0)
                return result;

            foreach (var materialId in materialIds)
            {
                var report = await CalculateMaterialSustainabilityScore(materialId);
                if (report != null)
                    result[materialId] = report;
            }

            return result;
        }

        private string GetSustainabilityLevel(decimal score)
        {
            return score switch
            {
                >= 80 => "Xuất sắc",
                >= 60 => "Tốt",
                >= 40 => "Trung bình",
                _ => "Cần cải thiện"
            };
        }

        private string GetLevelColor(string level)
        {
            return level switch
            {
                "Xuất sắc" => "green",
                "Tốt" => "#FFD700", // Golden yellow - dễ đọc hơn
                "Trung bình" => "orange",
                _ => "red"
            };
        }

        private string GetRecommendation(decimal overallScore)
        {
            return overallScore switch
            {
                >= 80 => "Vật liệu rất bền vững, phù hợp cho thị trường cao cấp",
                >= 60 => "Vật liệu khá bền vững, phù hợp cho thị trường phổ thông",
                >= 40 => "Vật liệu bình thường, cần cải thiện để tăng tính bền vững",
                _ => "Vật liệu kém bền vững, cần cải thiện đáng kể"
            };
        }

        /// <summary>
        /// Lấy đánh giá chi tiết về sustainability score
        /// </summary>
        public object GetSustainabilityEvaluation(decimal score)
        {
            var level = GetSustainabilityLevel(score);
            var color = GetLevelColor(level);
            var description = GetScoreDescription(score);
            var recommendation = GetRecommendation(score);
            var category = GetScoreCategory(score);

            return new
            {
                score = score,
                level = level,
                color = color,
                description = description,
                recommendation = recommendation,
                category = category,
                isExcellent = score >= 80,
                isGood = score >= 60 && score < 80,
                isFair = score >= 40 && score < 60,
                isPoor = score < 40
            };
        }

        private string GetScoreDescription(decimal score)
        {
            return score switch
            {
                >= 80 => "Đạt chuẩn bền vững cao nhất",
                >= 60 => "Đạt chuẩn bền vững tốt",
                >= 40 => "Cần cải thiện thêm",
                _ => "Cần cải thiện đáng kể"
            };
        }

        private string GetScoreCategory(decimal score)
        {
            return score switch
            {
                >= 80 => "Xuất sắc",
                >= 60 => "Tốt",
                >= 40 => "Trung bình",
                _ => "Cần cải thiện"
            };
        }
    }
}
