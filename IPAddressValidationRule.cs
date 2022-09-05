using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows.Controls;

namespace NetSpector
{
    /// <summary>
    /// 自定义规则：IP地址验证检查
    /// </summary>
    public class IPAddressValidationRule : ValidationRule
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public IPAddressValidationRule()
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
            string text = value?.ToString().Trim();
            if (text == null || text.Length == 0)
            {
                return ValidationResult.ValidResult;
            }
            else if (text.Split(',').Count(item => !IPAddress.TryParse(item, out IPAddress unused)) > 0)
            {
                return new ValidationResult(false, "字符输入不正确");
            }
            else
            {
                return ValidationResult.ValidResult;
            }
        }
    }
}
