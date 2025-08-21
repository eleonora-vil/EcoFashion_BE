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
        /// Tính điểm vận chuyển sử dụng TransportCalculationService mới với hỗ trợ override
        /// </summary>
        private decimal CalculateTransportScore(Material material)
        {
            if (material.TransportDistance == null || material.TransportMethod == null || 
                string.IsNullOrEmpty(material.ProductionCountry))
                return 0;

            try
            {
                // Use the new TransportCalculationService to get evaluation
                var evaluation = TransportCalculationService.GetTransportEvaluation(
                    material.TransportDistance.Value, 
                    material.TransportMethod);
                
                // Extract sustainability impact and convert to score
                var evaluationObj = evaluation as dynamic;
                if (evaluationObj != null)
                {
                    // The new service provides more sophisticated scoring
                    var distance = material.TransportDistance.Value;
                    var method = material.TransportMethod.ToLower();
                    
                    // Enhanced scoring logic that considers country-specific data
                    decimal distanceScore = distance switch
                    {
                        <= 100 => 95,      // Domestic/very close: 95%
                        <= 500 => 90,      // Regional: 90%
                        <= 1000 => 80,     // Near: 80%
                        <= 2000 => 70,     // Medium: 70%
                        <= 5000 => 50,     // Far: 50%
                        <= 10000 => 30,    // Very far: 30%
                        _ => 10             // Intercontinental: 10%
                    };

                    // Enhanced method scoring aligned with sustainability goals
                    decimal methodScore = method switch
                    {
                        "sea" => 90,        // Sea transport: most sustainable for long distance
                        "rail" => 95,       // Rail transport: most sustainable overall
                        "land" => 75,       // Land transport: good for short/medium distance
                        "air" => 20,        // Air transport: least sustainable
                        _ => 60             // Default/unknown method
                    };

                    // Check if this is the recommended method for the country
                    if (!string.IsNullOrEmpty(material.ProductionCountry))
                    {
                        try
                        {
                            var availableMethods = TransportCalculationService.GetAvailableTransportMethods(material.ProductionCountry);
                            var currentMethod = availableMethods.FirstOrDefault(m => 
                                m.Method.Equals(material.TransportMethod, StringComparison.OrdinalIgnoreCase));
                            
                            if (currentMethod != null && currentMethod.IsRecommended)
                            {
                                methodScore += 5; // Bonus for using recommended method
                            }
                            else if (currentMethod == null)
                            {
                                methodScore -= 10; // Penalty for unsupported method
                            }
                        }
                        catch
                        {
                            // Country not supported in new system, use legacy scoring
                        }
                    }

                    // Weighted combination: distance (70%) + method (30%)
                    decimal finalScore = (distanceScore * 0.7m) + (methodScore * 0.3m);
                    return Math.Max(0, Math.Min(100, finalScore));
                }
            }
            catch
            {
                // Fallback to simplified scoring if TransportCalculationService fails
            }

            // Fallback: Legacy scoring for backward compatibility
            return CalculateTransportScoreLegacy(material);
        }

        /// <summary>
        /// Legacy transport scoring for backward compatibility
        /// </summary>
        private decimal CalculateTransportScoreLegacy(Material material)
        {
            var distance = material.TransportDistance.Value;
            var method = material.TransportMethod.ToLower();

            decimal distanceScore = distance switch
            {
                <= 500 => 90,
                <= 1000 => 80,
                <= 2000 => 70,
                <= 5000 => 50,
                _ => 20
            };

            decimal methodScore = method switch
            {
                "sea" => 80,
                "rail" => 90,
                "land" => 70,
                "air" => 30,
                _ => 60
            };

            return (distanceScore * 0.7m) + (methodScore * 0.3m);
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

            // Simplified business rule: Any recognized certification = Full 20% score
            // Use Contains() instead of exact matching to handle variations like "OEKO-TEX Standard 100"
            foreach (var cert in certifications)
            {
                // Check if the certification contains any recognized keywords
                bool isRecognizedCert = 
                    // Tier 1: Comprehensive sustainability standards
                    cert.Contains("GOTS") || 
                    cert.Contains("CRADLE TO CRADLE") || 
                    cert.Contains("USDA ORGANIC") || 
                    cert.Contains("BLUESIGN") ||
                    
                    // Tier 2: High-value specialized standards
                    cert.Contains("OCS") || 
                    cert.Contains("EU ECOLABEL") || 
                    cert.Contains("FAIRTRADE") || 
                    cert.Contains("BCI") || 
                    cert.Contains("BETTER COTTON") ||
                    cert.Contains("OEKO-TEX") ||  // Matches "OEKO-TEX Standard 100", etc.
                    cert.Contains("RWS") || 
                    cert.Contains("ECO PASSPORT") ||
                    
                    // Tier 3: Material-specific recycling standards
                    cert.Contains("GRS") || 
                    cert.Contains("RCS") || 
                    cert.Contains("RECYCLED CLAIM");

                if (isRecognizedCert)
                {
                    return 20; // Full certification score (20% of total sustainability)
                }
            }

            return 0; // No recognized certification found
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
            {
                actualValue = actualMs.Value;
            }
            // Note: Organic Certification fallback logic is handled in the switch case below

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
                    // Check if we have a MaterialSustainability record for this criterion
                    var organicMs = actualValues.FirstOrDefault(ms => ms.CriterionId == criterion.CriterionId);
                    
                    if (organicMs != null)
                    {
                        // Use the stored value from MaterialSustainability
                        score = organicMs.Value == 100 ? 100 : 0;
                        status = organicMs.Value == 100 ? "Certified" : "Not Certified";
                        explanation = organicMs.Value == 100 ? 
                            "Có chứng nhận bền vững được công nhận" : 
                            "Không có chứng nhận bền vững được công nhận";
                    }
                    else
                    {
                        // No MaterialSustainability record - fall back to certificationDetails string analysis
                        var hasCertification = HasOrganicCertification(material);
                        score = hasCertification ? 100 : 0;
                        status = hasCertification ? "Certified" : "Not Certified";
                        explanation = hasCertification ? 
                            "Có chứng nhận bền vững được công nhận (từ certificationDetails)" :
                            "Không có chứng nhận bền vững được công nhận";
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
            
            // Expanded recognition of sustainability certifications
            // Tier 1: Comprehensive sustainability standards
            if (details.Contains("GOTS") || 
                details.Contains("CRADLE TO CRADLE") || 
                details.Contains("USDA ORGANIC") || 
                details.Contains("BLUESIGN"))
                return true;

            // Tier 2: High-value specialized standards
            if (details.Contains("OCS") || 
                details.Contains("EU ECOLABEL") || 
                details.Contains("FAIRTRADE") || 
                details.Contains("BCI") || 
                details.Contains("BETTER COTTON") ||
                details.Contains("OEKO-TEX") ||  // Matches "OEKO-TEX Standard 100", "OEKO-TEX Standard 1000", etc.
                details.Contains("RWS") || 
                details.Contains("ECO PASSPORT"))
                return true;

            // Tier 3: Material-specific recycling standards
            if (details.Contains("GRS") || 
                details.Contains("RCS") || 
                details.Contains("RECYCLED CLAIM"))
                return true;

            return false;
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
