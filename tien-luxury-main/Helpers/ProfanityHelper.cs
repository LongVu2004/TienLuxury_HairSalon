using System.Text.RegularExpressions;

namespace TienLuxury.Helpers
{
    public static class ProfanityHelper
    {
        
        private static readonly string[] BadWords = new string[]
        {
            "cc", "đéo", "đm", "vkl", "chó", "ngu", "lừa đảo",
            "như l", "cứt", "fuck", "shitty", "vcl", "địt",
            "đcmm", "clmm", "đmm", "dm", "lồn", "buồi", "lon", "vl"
        };

        public static string CensorText(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string output = input;

            foreach (var word in BadWords)
            {
                // Dùng Regex để thay thế không phân biệt hoa thường (Case Insensitive)
                string pattern = Regex.Escape(word);

                // Thay thế bằng số dấu * tương ứng độ dài từ (hoặc fix cứng là "***")
                string replacement = new string('*', word.Length);

                output = Regex.Replace(output, pattern, replacement, RegexOptions.IgnoreCase);
            }

            return output;
        }
    }
}
