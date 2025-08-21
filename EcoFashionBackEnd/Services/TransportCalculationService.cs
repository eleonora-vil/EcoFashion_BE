using EcoFashionBackEnd.Common.Payloads.Requests;

namespace EcoFashionBackEnd.Services
{
    public class TransportCalculationService
    {
        // Supported 12 countries with focus on sustainability and major textile production
        private static readonly Dictionary<string, CountryTransportInfo> SupportedCountries = new()
        {
            { 
                "Vietnam", 
                new CountryTransportInfo 
                { 
                    Distance = 0, 
                    Flag = "🇻🇳",
                    SustainabilityRating = "Excellent",
                    Description = "Sản xuất trong nước - Không có carbon footprint vận chuyển",
                    AvailableTransportMethods = new List<TransportMethodInfo>
                    {
                        new("Land", 0, "LocalShipping", "Vận chuyển nội địa", true),
                        new("Rail", 0, "Train", "Đường sắt nội địa", true),
                        new("Sea", 0, "DirectionsBoat", "Vận chuyển ven biển", true)
                    }
                }
            },
            { 
                "China", 
                new CountryTransportInfo 
                { 
                    Distance = 1200, 
                    Flag = "🇨🇳",
                    SustainabilityRating = "Good",
                    Description = "Quốc gia sản xuất vải lớn nhất thế giới",
                    AvailableTransportMethods = new List<TransportMethodInfo>
                    {
                        new("Land", 1200, "LocalShipping", "Vận chuyển bằng xe tải qua biên giới", true),
                        new("Rail", 1150, "Train", "Đường sắt xuyên biên giới - Hiệu quả cao", true),
                        new("Sea", 1800, "DirectionsBoat", "Vận chuyển bằng tàu biển - Ít carbon nhất", true),
                        new("Air", 1200, "Flight", "Vận chuyển hàng không - Nhanh nhất", false)
                    }
                }
            },
            { 
                "India", 
                new CountryTransportInfo 
                { 
                    Distance = 3500, 
                    Flag = "🇮🇳",
                    SustainabilityRating = "Good",
                    Description = "Trung tâm sản xuất cotton hữu cơ và vải bền vững",
                    AvailableTransportMethods = new List<TransportMethodInfo>
                    {
                        new("Sea", 4200, "DirectionsBoat", "Vận chuyển bằng tàu biển - Phương thức tối ưu", true),
                        new("Air", 3500, "Flight", "Vận chuyển hàng không - Nhanh chóng", false),
                        new("Land", 4800, "LocalShipping", "Vận chuyển bằng xe tải - Thời gian dài", false)
                    }
                }
            },
            { 
                "Japan", 
                new CountryTransportInfo 
                { 
                    Distance = 3200, 
                    Flag = "🇯🇵",
                    SustainabilityRating = "Excellent",
                    Description = "Công nghệ vải tiên tiến và thân thiện môi trường",
                    AvailableTransportMethods = new List<TransportMethodInfo>
                    {
                        new("Sea", 3200, "DirectionsBoat", "Vận chuyển bằng tàu biển - Ít carbon nhất", true),
                        new("Air", 3200, "Flight", "Vận chuyển hàng không - Giao hàng nhanh", false)
                    }
                }
            },
            { 
                "Korea", 
                new CountryTransportInfo 
                { 
                    Distance = 2800, 
                    Flag = "🇰🇷",
                    SustainabilityRating = "Excellent", 
                    Description = "Công nghệ vải thông minh và bền vững",
                    AvailableTransportMethods = new List<TransportMethodInfo>
                    {
                        new("Sea", 2800, "DirectionsBoat", "Vận chuyển bằng tàu biển - Ít carbon nhất", true),
                        new("Air", 2800, "Flight", "Vận chuyển hàng không - Giao hàng nhanh", false)
                    }
                }
            },
            { 
                "United States", 
                new CountryTransportInfo 
                { 
                    Distance = 14000, 
                    Flag = "🇺🇸",
                    SustainabilityRating = "Fair",
                    Description = "Sản xuất cotton hữu cơ và công nghệ vải tái chế",
                    AvailableTransportMethods = new List<TransportMethodInfo>
                    {
                        new("Sea", 14000, "DirectionsBoat", "Vận chuyển bằng tàu biển - Ít carbon nhất", true),
                        new("Air", 14000, "Flight", "Vận chuyển hàng không - Nhanh chóng", false)
                    }
                }
            },
            { 
                "Turkey", 
                new CountryTransportInfo 
                { 
                    Distance = 8500, 
                    Flag = "🇹🇷",
                    SustainabilityRating = "Good",
                    Description = "Trung tâm sản xuất vải bền vững châu Âu",
                    AvailableTransportMethods = new List<TransportMethodInfo>
                    {
                        new("Sea", 9200, "DirectionsBoat", "Vận chuyển bằng tàu biển - Ít carbon nhất", true),
                        new("Air", 8500, "Flight", "Vận chuyển hàng không - Nhanh chóng", false),
                        new("Land", 12000, "LocalShipping", "Vận chuyển bằng xe tải - Thời gian dài", false)
                    }
                }
            },
            { 
                "Bangladesh", 
                new CountryTransportInfo 
                { 
                    Distance = 2500, 
                    Flag = "🇧🇩",
                    SustainabilityRating = "Fair",
                    Description = "Sản xuất vải với các tiêu chuẩn bền vững ngày càng cao",
                    AvailableTransportMethods = new List<TransportMethodInfo>
                    {
                        new("Sea", 3000, "DirectionsBoat", "Vận chuyển bằng tàu biển - Ít carbon nhất", true),
                        new("Air", 2500, "Flight", "Vận chuyển hàng không - Nhanh chóng", false),
                        new("Land", 3200, "LocalShipping", "Vận chuyển bằng xe tải", false)
                    }
                }
            },
            { 
                "Indonesia", 
                new CountryTransportInfo 
                { 
                    Distance = 1800, 
                    Flag = "🇮🇩",
                    SustainabilityRating = "Good",
                    Description = "Sản xuất vải từ sợi tự nhiên bền vững",
                    AvailableTransportMethods = new List<TransportMethodInfo>
                    {
                        new("Sea", 1800, "DirectionsBoat", "Vận chuyển bằng tàu biển - Ít carbon nhất", true),
                        new("Air", 1800, "Flight", "Vận chuyển hàng không - Nhanh chóng", false)
                    }
                }
            },
            { 
                "Thailand", 
                new CountryTransportInfo 
                { 
                    Distance = 800, 
                    Flag = "🇹🇭",
                    SustainabilityRating = "Good",
                    Description = "Sản xuất vải silk và cotton hữu cơ",
                    AvailableTransportMethods = new List<TransportMethodInfo>
                    {
                        new("Land", 800, "LocalShipping", "Vận chuyển bằng xe tải - Phù hợp cho khoảng cách ngắn", true),
                        new("Sea", 1200, "DirectionsBoat", "Vận chuyển bằng tàu biển", true),
                        new("Air", 800, "Flight", "Vận chuyển hàng không - Nhanh chóng", false)
                    }
                }
            },
            { 
                "Peru", 
                new CountryTransportInfo 
                { 
                    Distance = 19000, 
                    Flag = "🇵🇪",
                    SustainabilityRating = "Excellent",
                    Description = "Sản xuất cotton Pima hữu cơ cao cấp",
                    AvailableTransportMethods = new List<TransportMethodInfo>
                    {
                        new("Sea", 19000, "DirectionsBoat", "Vận chuyển bằng tàu biển - Ít carbon nhất", true),
                        new("Air", 19000, "Flight", "Vận chuyển hàng không - Nhanh chóng", false)
                    }
                }
            },
            { 
                "Egypt", 
                new CountryTransportInfo 
                { 
                    Distance = 8000, 
                    Flag = "🇪🇬",
                    SustainabilityRating = "Good",
                    Description = "Sản xuất cotton chất lượng cao với tiêu chuẩn bền vững",
                    AvailableTransportMethods = new List<TransportMethodInfo>
                    {
                        new("Sea", 8500, "DirectionsBoat", "Vận chuyển bằng tàu biển - Ít carbon nhất", true),
                        new("Air", 8000, "Flight", "Vận chuyển hàng không - Nhanh chóng", false)
                    }
                }
            }
        };

        // Models for enhanced transport data
        public class CountryTransportInfo
        {
            public decimal Distance { get; set; }
            public string Flag { get; set; } = "";
            public string SustainabilityRating { get; set; } = "";
            public string Description { get; set; } = "";
            public List<TransportMethodInfo> AvailableTransportMethods { get; set; } = new();
        }

        public class TransportMethodInfo
        {
            public string Method { get; set; }
            public decimal Distance { get; set; }
            public string Icon { get; set; }
            public string Description { get; set; }
            public bool IsRecommended { get; set; }

            public TransportMethodInfo(string method, decimal distance, string icon, string description, bool isRecommended)
            {
                Method = method;
                Distance = distance;
                Icon = icon;
                Description = description;
                IsRecommended = isRecommended;
            }
        }

        public class TransportOverrideOption
        {
            public string Method { get; set; } = "";
            public decimal EstimatedDistance { get; set; }
            public string Icon { get; set; } = "";
            public string Description { get; set; } = "";
            public bool IsRecommended { get; set; }
            public string SustainabilityImpact { get; set; } = "";
            public string Color { get; set; } = "";
        }

        /// <summary>
        /// Kiểm tra xem quốc gia có được hỗ trợ không
        /// </summary>
        public static bool IsCountrySupported(string? productionCountry)
        {
            if (string.IsNullOrEmpty(productionCountry))
                return false;

            var country = productionCountry.Trim();
            
            // Try exact match first
            if (SupportedCountries.ContainsKey(country))
                return true;

            // Try case-insensitive match
            var countryLower = country.ToLower();
            return SupportedCountries.Keys.Any(k => k.ToLower() == countryLower);
        }

        /// <summary>
        /// Lấy danh sách 12 quốc gia được hỗ trợ
        /// </summary>
        public static List<string> GetSupportedCountries()
        {
            return SupportedCountries.Keys.ToList();
        }

        /// <summary>
        /// Tự động tính khoảng cách vận chuyển từ nước sản xuất đến Vietnam (chỉ cho 12 nước hỗ trợ)
        /// </summary>
        public static decimal CalculateTransportDistance(string? productionCountry)
        {
            if (string.IsNullOrEmpty(productionCountry))
                return 0;

            var country = productionCountry.Trim();
            
            // Try exact match first
            if (SupportedCountries.TryGetValue(country, out var countryInfo))
                return countryInfo.Distance;

            // Try case-insensitive match
            var countryLower = country.ToLower();
            var match = SupportedCountries.FirstOrDefault(kvp => 
                kvp.Key.ToLower() == countryLower);
            
            if (!string.IsNullOrEmpty(match.Key))
                return match.Value.Distance;

            // Unsupported country - throw exception or return error
            throw new ArgumentException($"Quốc gia '{productionCountry}' không được hỗ trợ. Các quốc gia được hỗ trợ: {string.Join(", ", GetSupportedCountries())}");
        }

        /// <summary>
        /// Lấy tất cả phương thức vận chuyển có sẵn cho một quốc gia
        /// </summary>
        public static List<TransportOverrideOption> GetAvailableTransportMethods(string? productionCountry)
        {
            if (string.IsNullOrEmpty(productionCountry))
                return new List<TransportOverrideOption>();

            var country = productionCountry.Trim();
            
            // Try exact match first
            if (SupportedCountries.TryGetValue(country, out var countryInfo))
            {
                return countryInfo.AvailableTransportMethods.Select(tm => new TransportOverrideOption
                {
                    Method = tm.Method,
                    EstimatedDistance = tm.Distance,
                    Icon = tm.Icon,
                    Description = tm.Description,
                    IsRecommended = tm.IsRecommended,
                    SustainabilityImpact = GetSustainabilityImpact(tm.Distance, tm.Method),
                    Color = GetMethodColor(tm.Method)
                }).ToList();
            }

            // Try case-insensitive match
            var countryLower = country.ToLower();
            var match = SupportedCountries.FirstOrDefault(kvp => 
                kvp.Key.ToLower() == countryLower);
            
            if (!string.IsNullOrEmpty(match.Key))
            {
                return match.Value.AvailableTransportMethods.Select(tm => new TransportOverrideOption
                {
                    Method = tm.Method,
                    EstimatedDistance = tm.Distance,
                    Icon = tm.Icon,
                    Description = tm.Description,
                    IsRecommended = tm.IsRecommended,
                    SustainabilityImpact = GetSustainabilityImpact(tm.Distance, tm.Method),
                    Color = GetMethodColor(tm.Method)
                }).ToList();
            }

            return new List<TransportOverrideOption>();
        }

        /// <summary>
        /// Tự động đề xuất phương thức vận chuyển dựa trên quốc gia (lấy phương thức được đánh dấu recommended)
        /// </summary>
        public static string GetRecommendedTransportMethod(string? productionCountry)
        {
            if (string.IsNullOrEmpty(productionCountry))
                return "Land";

            var country = productionCountry.Trim();
            
            // Try exact match first
            if (SupportedCountries.TryGetValue(country, out var countryInfo))
            {
                var recommended = countryInfo.AvailableTransportMethods.FirstOrDefault(tm => tm.IsRecommended);
                return recommended?.Method ?? "Sea";
            }

            // Try case-insensitive match
            var countryLower = country.ToLower();
            var match = SupportedCountries.FirstOrDefault(kvp => 
                kvp.Key.ToLower() == countryLower);
            
            if (!string.IsNullOrEmpty(match.Key))
            {
                var recommended = match.Value.AvailableTransportMethods.FirstOrDefault(tm => tm.IsRecommended);
                return recommended?.Method ?? "Sea";
            }

            return "Sea"; // fallback
        }

        /// <summary>
        /// Tự động đề xuất phương thức vận chuyển dựa trên khoảng cách (legacy method để backward compatibility)
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
        /// Tính toán thông tin vận chuyển cho material creation request với hỗ trợ override
        /// </summary>
        public static void CalculateTransportInfo(MaterialCreationFormRequest request)
        {
            // Validate country is supported
            if (!IsCountrySupported(request.ProductionCountry))
            {
                throw new ArgumentException($"Quốc gia '{request.ProductionCountry}' không được hỗ trợ. Các quốc gia được hỗ trợ: {string.Join(", ", GetSupportedCountries())}");
            }

            // Nếu supplier đã chọn transport method, validate nó có hợp lệ không
            if (!string.IsNullOrEmpty(request.TransportMethod))
            {
                var availableMethods = GetAvailableTransportMethods(request.ProductionCountry);
                var selectedMethod = availableMethods.FirstOrDefault(m => 
                    m.Method.Equals(request.TransportMethod, StringComparison.OrdinalIgnoreCase));
                
                if (selectedMethod != null)
                {
                    // Supplier đã chọn method hợp lệ - sử dụng distance tương ứng
                    request.TransportDistance = selectedMethod.EstimatedDistance;
                }
                else
                {
                    // Method không hợp lệ - throw exception
                    var availableMethodNames = availableMethods.Select(m => m.Method);
                    throw new ArgumentException($"Phương thức vận chuyển '{request.TransportMethod}' không hợp lệ cho quốc gia '{request.ProductionCountry}'. Các phương thức có sẵn: {string.Join(", ", availableMethodNames)}");
                }
            }
            else
            {
                // Supplier chưa chọn method - tự động đề xuất
                request.TransportMethod = GetRecommendedTransportMethod(request.ProductionCountry);
                
                // Tính distance tương ứng với method được đề xuất
                var availableMethods = GetAvailableTransportMethods(request.ProductionCountry);
                var recommendedMethod = availableMethods.FirstOrDefault(m => 
                    m.Method.Equals(request.TransportMethod, StringComparison.OrdinalIgnoreCase));
                
                request.TransportDistance = recommendedMethod?.EstimatedDistance ?? CalculateTransportDistance(request.ProductionCountry);
            }
        }

        /// <summary>
        /// Lấy danh sách các nước sản xuất được hỗ trợ (thay thế GetCommonProductionCountries)
        /// </summary>
        public static List<string> GetCommonProductionCountries()
        {
            return GetSupportedCountries().Where(k => k != "Vietnam").ToList();
        }

        /// <summary>
        /// Lấy thông tin chi tiết về vận chuyển với method được đề xuất
        /// </summary>
        public static (decimal distance, string method, string description) GetTransportDetails(string? productionCountry)
        {
            if (!IsCountrySupported(productionCountry))
            {
                throw new ArgumentException($"Quốc gia '{productionCountry}' không được hỗ trợ. Các quốc gia được hỗ trợ: {string.Join(", ", GetSupportedCountries())}");
            }

            var distance = CalculateTransportDistance(productionCountry);
            var method = GetRecommendedTransportMethod(productionCountry);
            
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
        /// Lấy thông tin chi tiết về vận chuyển với method cụ thể
        /// </summary>
        public static (decimal distance, string method, string description) GetTransportDetails(string? productionCountry, string transportMethod)
        {
            if (!IsCountrySupported(productionCountry))
            {
                throw new ArgumentException($"Quốc gia '{productionCountry}' không được hỗ trợ. Các quốc gia được hỗ trợ: {string.Join(", ", GetSupportedCountries())}");
            }

            var availableMethods = GetAvailableTransportMethods(productionCountry);
            var selectedMethod = availableMethods.FirstOrDefault(m => 
                m.Method.Equals(transportMethod, StringComparison.OrdinalIgnoreCase));
            
            if (selectedMethod == null)
            {
                var availableMethodNames = availableMethods.Select(m => m.Method);
                throw new ArgumentException($"Phương thức vận chuyển '{transportMethod}' không hợp lệ cho quốc gia '{productionCountry}'. Các phương thức có sẵn: {string.Join(", ", availableMethodNames)}");
            }

            return (selectedMethod.EstimatedDistance, selectedMethod.Method, selectedMethod.Description);
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
        /// Lấy đánh giá chi tiết về sản xuất (cho 12 quốc gia được hỗ trợ)
        /// </summary>
        public static object GetProductionEvaluation(string? country)
        {
            if (!IsCountrySupported(country))
            {
                throw new ArgumentException($"Quốc gia '{country}' không được hỗ trợ. Các quốc gia được hỗ trợ: {string.Join(", ", GetSupportedCountries())}");
            }

            var isDomestic = country?.Equals("Vietnam", StringComparison.OrdinalIgnoreCase) ?? false;
            var countryInfo = GetCountryInfo(country);

            return new
            {
                country = country,
                flag = countryInfo?.Flag ?? "🌍",
                isDomestic = isDomestic,
                sustainabilityRating = countryInfo?.SustainabilityRating ?? "Unknown",
                description = countryInfo?.Description ?? "Không có thông tin",
                category = isDomestic ? "Sản xuất trong nước" : "Nhập khẩu",
                categoryColor = isDomestic ? "success" : "warning",
                availableTransportMethods = GetAvailableTransportMethods(country).Count
            };
        }

        /// <summary>
        /// Lấy thông tin quốc gia
        /// </summary>
        public static CountryTransportInfo? GetCountryInfo(string? country)
        {
            if (string.IsNullOrEmpty(country))
                return null;

            var countryName = country.Trim();
            
            // Try exact match first
            if (SupportedCountries.TryGetValue(countryName, out var countryInfo))
                return countryInfo;

            // Try case-insensitive match
            var countryLower = countryName.ToLower();
            var match = SupportedCountries.FirstOrDefault(kvp => 
                kvp.Key.ToLower() == countryLower);
            
            return !string.IsNullOrEmpty(match.Key) ? match.Value : null;
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
            var countryInfo = GetCountryInfo(country);
            return countryInfo?.Flag ?? "🌍";
        }

        private static string GetProductionSustainabilityImpact(string? country)
        {
            var countryInfo = GetCountryInfo(country);
            if (countryInfo == null)
                return "Không xác định";
                
            return country?.Equals("Vietnam", StringComparison.OrdinalIgnoreCase) == true
                ? "Tốt - Giảm thiểu carbon footprint"
                : $"{countryInfo.SustainabilityRating} - {countryInfo.Description}";
        }

        private static string GetProductionDescription(string? country)
        {
            var countryInfo = GetCountryInfo(country);
            if (countryInfo == null)
                return $"Sản xuất tại {country} - Không có thông tin";
                
            return country?.Equals("Vietnam", StringComparison.OrdinalIgnoreCase) == true
                ? "Sản xuất trong nước - Giảm thiểu carbon footprint"
                : $"Sản xuất tại {country} - {countryInfo.Description}";
        }
    }
} 