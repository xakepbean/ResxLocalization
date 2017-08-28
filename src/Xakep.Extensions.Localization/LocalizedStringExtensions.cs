using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Xakep.Extensions.Localization
{
    public static class LocalizedStringExtensions
    {
        /// <summary>
        /// 格式化资源文件的参数
        /// </summary>
        /// <param name="Localizer"></param>
        /// <param name="param"></param>
        /// <example>
        ///  --Hello="你好 {UserName}"
        ///  Localizer["Hello"].Format(new { UserName = "小明" });
        /// </example>
        /// <returns></returns>
        public static string Format(this LocalizedString Localizer, object classarg)
        {
            if (string.IsNullOrWhiteSpace(Localizer.Value))
                return Localizer.Value;
            Dictionary<string, object> lstParam = classarg.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(classarg));
            if (lstParam.Count == 0)
                return Localizer.Value;

            var LocalString = Localizer.Value;
            foreach (var item in lstParam)
            {
                LocalString = LocalString.Replace($"{{{item.Key}}}", item.Value.ToString());
            }
            return LocalString;

        }
    }
}
