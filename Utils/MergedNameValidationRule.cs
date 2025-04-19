
using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace BatchTextProcessor.Utils
{
    public class MergedNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string text && !string.IsNullOrWhiteSpace(text))
            {
                // 简单验证：不允许包含特殊字符
                if (text.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    return new ValidationResult(false, "合并名称包含非法字符");
                }
                return ValidationResult.ValidResult;
            }
            return new ValidationResult(false, "合并名称不能为空");
        }
    }
}
