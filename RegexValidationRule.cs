using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace NetSpector
{
    /// <summary>
    /// 自定义规则：正则表达式验证检查
    /// </summary>
    public class RegexValidationRule : ValidationRule
    {
        /// <summary>
        /// 正则表达式模板
        /// </summary>
        public string Pattern { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        public RegexValidationRule()
        {
        }
        /// <summary>
        /// 执行验证检查
        /// </summary>
        /// <param name="value">输入值</param>
        /// <param name="cultureInfo">区域信息</param>
        /// <returns>检查结果</returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            Regex regex = new Regex(Pattern);
            if (regex.IsMatch((string)value))
            {
                return ValidationResult.ValidResult;
            }
            else
            {
                return new ValidationResult(false, "字符输入不正确");
            }
        }
    }
}
