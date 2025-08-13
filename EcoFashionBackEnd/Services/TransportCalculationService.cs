using EcoFashionBackEnd.Common.Payloads.Requests;

namespace EcoFashionBackEnd.Services
{
    public class TransportCalculationService
    {
        // Dictionary of distances from Vietnam to major production countries
        private static readonly Dictionary<string, decimal> CountryDistances = new()
        {
            { "Vietnam", 0 },
            { "China", 1200 },
            { "India", 3500 },
            { "Turkey", 2800 },
            { "Bangladesh", 2500 },
            { "Pakistan", 3200 },
            { "Indonesia", 1800 },
            { "Thailand", 800 },
            { "Malaysia", 1500 },
            { "Philippines", 1200 },
            { "Myanmar", 600 },
            { "Cambodia", 400 },
            { "Laos", 300 },
            { "Singapore", 1400 },
            { "Japan", 3200 },
            { "South Korea", 2800 },
            { "Taiwan", 1800 },
            { "United States", 14000 },
            { "Brazil", 18000 },
            { "Mexico", 16000 },
            { "Egypt", 8000 },
            { "Morocco", 12000 },
            { "Tunisia", 11000 },
            { "Ethiopia", 7000 },
            { "Kenya", 7500 },
            { "Uganda", 7000 },
            { "Tanzania", 7500 },
            { "Madagascar", 6500 },
            { "Mauritius", 6000 },
            { "South Africa", 10000 },
            { "Australia", 6000 },
            { "New Zealand", 8000 }
        };

        // Dictionary of recommended transport methods based on distance
        private static readonly Dictionary<string, string> DistanceTransportMethods = new()
        {
            { "0-500", "Land" },
            { "500-1000", "Land" },
            { "1000-2000", "Land" },
            { "2000-5000", "Sea" },
            { "5000+", "Sea" }
        };

        /// <summary>
        /// Tự động tính khoảng cách vận chuyển từ nước sản xuất đến Vietnam
        /// </summary>
        public static decimal CalculateTransportDistance(string? productionCountry)
        {
            if (string.IsNullOrEmpty(productionCountry))
                return 0;

            var country = productionCountry.Trim();
            
            // Try exact match first
            if (CountryDistances.TryGetValue(country, out var distance))
                return distance;

            // Try case-insensitive match
            var countryLower = country.ToLower();
            var match = CountryDistances.FirstOrDefault(kvp => 
                kvp.Key.ToLower() == countryLower);
            
            if (!string.IsNullOrEmpty(match.Key))
                return match.Value;

            // Default distance for unknown countries
            return 5000;
        }

        /// <summary>
        /// Tự động đề xuất phương thức vận chuyển dựa trên khoảng cách
        /// </summary>
        public static string GetRecommendedTransportMethod(decimal distance)
        {
            if (distance <= 500) return "Land";
            if (distance <= 1000) return "Land";
            if (distance <= 2000) return "Land";
            if (distance <= 5000) return "Sea";
            return "Sea";
        }

        /// <summary>
        /// Tính toán thông tin vận chuyển cho material creation request
        /// </summary>
        public static void CalculateTransportInfo(MaterialCreationFormRequest request)
        {
            // Nếu chưa có transport distance, tự động tính
            if (request.TransportDistance == null || request.TransportDistance == 0)
            {
                request.TransportDistance = CalculateTransportDistance(request.ProductionCountry);
            }

            // Nếu chưa có transport method, tự động đề xuất
            if (string.IsNullOrEmpty(request.TransportMethod))
            {
                request.TransportMethod = GetRecommendedTransportMethod(request.TransportDistance ?? 0);
            }
        }

        /// <summary>
        /// Lấy danh sách các nước sản xuất phổ biến
        /// </summary>
        public static List<string> GetCommonProductionCountries()
        {
            return CountryDistances.Keys.Where(k => k != "Vietnam").ToList();
        }

        /// <summary>
        /// Lấy thông tin chi tiết về vận chuyển
        /// </summary>
        public static (decimal distance, string method, string description) GetTransportDetails(string? productionCountry)
        {
            var distance = CalculateTransportDistance(productionCountry);
            var method = GetRecommendedTransportMethod(distance);
            
            var description = method switch
            {
                "Sea" => $"Vận chuyển bằng tàu biển từ {productionCountry} ({distance}km) - Ít carbon nhất",
                "Land" => $"Vận chuyển bằng xe tải từ {productionCountry} ({distance}km) - Phù hợp cho khoảng cách ngắn",
                "Rail" => $"Vận chuyển bằng tàu hỏa từ {productionCountry} ({distance}km) - Hiệu quả cao",
                "Air" => $"Vận chuyển bằng máy bay từ {productionCountry} ({distance}km) - Nhanh nhất nhưng nhiều carbon",
                _ => $"Vận chuyển từ {productionCountry} ({distance}km)"
            };

            return (distance, method, description);
        }

        /// <summary>
        /// Lấy đánh giá chi tiết về vận chuyển
        /// </summary>
        public static object GetTransportEvaluation(decimal distance, string method)
        {
            var distanceCategory = GetDistanceCategory(distance);
            var methodColor = GetMethodColor(method);
            var methodDescription = GetMethodDescription(method);
            var sustainabilityImpact = GetSustainabilityImpact(distance, method);

            return new
            {
                distance = distance,
                method = method,
                distanceCategory = distanceCategory,
                methodColor = methodColor,
                methodDescription = methodDescription,
                sustainabilityImpact = sustainabilityImpact,
                icon = GetMethodIcon(method),
                isRecommended = IsRecommendedMethod(distance, method)
            };
        }

        /// <summary>
        /// Lấy đánh giá chi tiết về sản xuất
        /// </summary>
        public static object GetProductionEvaluation(string country)
        {
            var isDomestic = country?.Equals("Vietnam", StringComparison.OrdinalIgnoreCase) ?? false;
            var flag = GetCountryFlag(country);
            var sustainabilityImpact = GetProductionSustainabilityImpact(country);
            var description = GetProductionDescription(country);

            return new
            {
                country = country,
                flag = flag,
                isDomestic = isDomestic,
                sustainabilityImpact = sustainabilityImpact,
                description = description,
                category = isDomestic ? "Sản xuất trong nước" : "Nhập khẩu",
                categoryColor = isDomestic ? "success" : "warning"
            };
        }

        private static string GetDistanceCategory(decimal distance)
        {
            if (distance <= 500) return "Gần";
            if (distance <= 2000) return "Trung bình";
            if (distance <= 5000) return "Xa";
            return "Rất xa";
        }

        private static string GetMethodColor(string method)
        {
            return method?.ToLower() switch
            {
                "sea" => "primary",
                "land" => "success",
                "rail" => "info",
                "air" => "warning",
                _ => "default"
            };
        }

        private static string GetMethodDescription(string method)
        {
            return method?.ToLower() switch
            {
                "sea" => "Vận chuyển bằng tàu biển - Ít carbon nhất",
                "land" => "Vận chuyển bằng xe tải - Phù hợp cho khoảng cách ngắn",
                "rail" => "Vận chuyển bằng tàu hỏa - Hiệu quả cao",
                "air" => "Vận chuyển bằng máy bay - Nhanh nhất nhưng nhiều carbon",
                _ => "Phương thức vận chuyển không xác định"
            };
        }

        private static string GetMethodIcon(string method)
        {
            return method?.ToLower() switch
            {
                "sea" => "DirectionsBoat",
                "land" => "LocalShipping",
                "rail" => "Train",
                "air" => "Flight",
                _ => "LocalShipping"
            };
        }

        private static bool IsRecommendedMethod(decimal distance, string method)
        {
            var recommended = GetRecommendedTransportMethod(distance);
            return method?.Equals(recommended, StringComparison.OrdinalIgnoreCase) ?? false;
        }

        private static string GetSustainabilityImpact(decimal distance, string method)
        {
            var distanceScore = distance switch
            {
                <= 500 => "Tốt",
                <= 2000 => "Trung bình",
                <= 5000 => "Cao",
                _ => "Rất cao"
            };

            var methodScore = method?.ToLower() switch
            {
                "sea" => "Tốt nhất",
                "rail" => "Tốt",
                "land" => "Trung bình",
                "air" => "Cao nhất",
                _ => "Không xác định"
            };

            return $"{distanceScore} (khoảng cách) + {methodScore} (phương thức)";
        }

        private static string GetCountryFlag(string? country)
        {
            var flags = new Dictionary<string, string>
            {
                { "Vietnam", "🇻🇳" },
                { "China", "🇨🇳" },
                { "India", "🇮🇳" },
                { "Turkey", "🇹🇷" },
                { "Bangladesh", "🇧🇩" },
                { "Pakistan", "🇵🇰" },
                { "Indonesia", "🇮🇩" },
                { "Thailand", "🇹🇭" },
                { "Malaysia", "🇲🇾" },
                { "Philippines", "🇵🇭" },
                { "Myanmar", "🇲🇲" },
                { "Cambodia", "🇰🇭" },
                { "Laos", "🇱🇦" },
                { "Singapore", "🇸🇬" },
                { "Japan", "🇯🇵" },
                { "South Korea", "🇰🇷" },
                { "Taiwan", "🇹🇼" },
                { "United States", "🇺🇸" },
                { "Brazil", "🇧🇷" },
                { "Mexico", "🇲🇽" },
                { "Egypt", "🇪🇬" },
                { "Morocco", "🇲🇦" },
                { "Tunisia", "🇹🇳" },
                { "Ethiopia", "🇪🇹" },
                { "Kenya", "🇰🇪" },
                { "Uganda", "🇺🇬" },
                { "Tanzania", "🇹🇿" },
                { "Madagascar", "🇲🇬" },
                { "Mauritius", "🇲🇺" },
                { "South Africa", "🇿🇦" },
                { "Australia", "🇦🇺" },
                { "New Zealand", "🇳🇿" }
            };

            return flags.TryGetValue(country ?? "", out var flag) ? flag : "🌍";
        }

        private static string GetProductionSustainabilityImpact(string? country)
        {
            return country?.Equals("Vietnam", StringComparison.OrdinalIgnoreCase) == true
                ? "Tốt - Giảm thiểu carbon footprint"
                : "Có thể ảnh hưởng đến sustainability score";
        }

        private static string GetProductionDescription(string? country)
        {
            return country?.Equals("Vietnam", StringComparison.OrdinalIgnoreCase) == true
                ? "Sản xuất trong nước - Giảm thiểu carbon footprint"
                : $"Sản xuất tại {country} - Có thể ảnh hưởng đến sustainability score";
        }
    }
} 