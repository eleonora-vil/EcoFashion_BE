namespace EcoFashionBackEnd.Helpers
{
    public class ColorExchange
    {
        public static string ClassifyColorAdvanced(string hex)
        {
            // Bỏ dấu #
            hex = hex.Replace("#", "");

            // Convert hex -> RGB
            int r = Convert.ToInt32(hex.Substring(0, 2), 16);
            int g = Convert.ToInt32(hex.Substring(2, 2), 16);
            int b = Convert.ToInt32(hex.Substring(4, 2), 16);

            // RGB -> HSL
            double rNorm = r / 255.0;
            double gNorm = g / 255.0;
            double bNorm = b / 255.0;

            double max = Math.Max(rNorm, Math.Max(gNorm, bNorm));
            double min = Math.Min(rNorm, Math.Min(gNorm, bNorm));
            double delta = max - min;

            double h = 0;
            if (delta != 0)
            {
                if (max == rNorm)
                    h = 60 * (((gNorm - bNorm) / delta) % 6);
                else if (max == gNorm)
                    h = 60 * (((bNorm - rNorm) / delta) + 2);
                else
                    h = 60 * (((rNorm - gNorm) / delta) + 4);
            }
            if (h < 0) h += 360;

            double l = (max + min) / 2;
            double s = delta == 0 ? 0 : delta / (1 - Math.Abs(2 * l - 1));

            // Convert sang %
            double H = h;
            double S = s * 100;
            double L = l * 100;

            // Phân loại 11 màu cơ bản
            if (S < 10 && L > 90) return "White";
            if (S < 10 && L < 10) return "Black";
            if (S < 10) return "Gray";
            if (H >= 0 && H < 15 || H >= 345 && H <= 360) return "Red";
            if (H >= 15 && H < 45) return "Orange";
            if (H >= 45 && H < 70) return "Yellow";
            if (H >= 70 && H < 170) return "Green";
            if (H >= 170 && H < 200) return "Cyan";
            if (H >= 200 && H < 250) return "Blue";
            if (H >= 250 && H < 290) return "Purple";
            if (H >= 290 && H < 345) return "Pink";

            return "Unknown";
        }
    }
}
