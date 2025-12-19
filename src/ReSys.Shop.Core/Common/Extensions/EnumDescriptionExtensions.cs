using System.ComponentModel;
using System.Reflection;

using Humanizer;

namespace ReSys.Shop.Core.Common.Extensions;

public static class EnumDescriptionExtensions
{
    private static string GetDescription<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        FieldInfo? field = typeof(TEnum).GetField(name: value.ToString());
        DescriptionAttribute? attr = field?.GetCustomAttribute<DescriptionAttribute>();
        if (attr != null)
            return attr.Description;

        return value.ToString().Humanize() + $"({(int)(object)value})";
    }

    private static List<string> GetEnumDescriptions<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>().Select(selector: GetDescription).ToList();
    }

    public static string GetEnumDescription<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        return GetDescription(value: value);
    }

    public static string GetEnumContextDescription<TEnum>() where TEnum : struct, Enum
    {
        return string.Join(separator: ", ",
            values: GetEnumDescriptions<TEnum>());
    }
}