using System;
using VanillaStyleDiplomacy.Managers;

namespace VanillaStyleDiplomacy.Extensions
{
    public static class StringExtensions
    {
        public static bool TryCastOnRunTime(this string value, Type targetType, out object result)
        {
            if (targetType == typeof(string))
            {
                result = value;
                return true;
            }

            bool ret = false;
            result = null;

            if (targetType == typeof(int))
            {
                ret = int.TryParse(value, out int i);
                result = i;
            }
            else if (targetType == typeof(double))
            {
                ret = double.TryParse(value, out double d);
                result = d;
            }
            else if (targetType == typeof(bool))
            {
                result = false;
                value = value.ToLower().Trim();
                ret = value == "true" || value == "false";
                if (ret)
                    result = value == "true" ? true : false;
            }
            else
                LoggingManager.Instance.LogMessage($"Tried to cast to unsupported type {targetType.ToString()}!");

            return ret;
        }
    }
}