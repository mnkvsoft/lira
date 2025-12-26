using System.Reflection;
using Lira.Common.Extensions;
using Lira.Common.PrettyParsers;

namespace Lira.Common;

[AttributeUsage(AttributeTargets.Property)]
public class ParameterNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

public static class LinesDeserializer
{
    public static T Deserialize<T>(string[] lines, char splitter = '=') where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(lines);

        var result = new T();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var propertyMap = properties
            .Select(p => new
            {
                Property = p,
                Name = p.GetCustomAttribute<ParameterNameAttribute>()?.Name ?? p.Name
            })
            .ToDictionary(x => x.Name, x => x.Property, StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var (name, value) = line.SplitToTwoParts(splitter).Trim();

            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Parameter name cannot be empty. Line: " + line);

            if (propertyMap.TryGetValue(name, out var property))
            {
                try
                {
                    SetPropertyValue(result, property, value);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error setting property '{property.Name}' with value '{value}': {ex.Message}");
                }
            }
            else
            {
                throw new InvalidOperationException("Unknown parameter: " + name);
            }
        }

        return result;
    }

    private static void SetPropertyValue(object obj, PropertyInfo property, string? value)
    {
        if (property.PropertyType == typeof(string))
        {
            property.SetValue(obj, value);
            return;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            if (property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) == null)
                throw new InvalidOperationException($"Cannot assign null to non-nullable type {property.PropertyType.Name}");

            property.SetValue(obj, null);
            return;
        }

        if (property.PropertyType == typeof(TimeSpan))
        {
            if (!PrettyTimespanParser.TryParse(value, out var convertedValue))
                throw new InvalidOperationException($"Cannot convert value '{value}' to TimeSpan");

            property.SetValue(obj, convertedValue);
            return;
        }

        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        // Используем TypeConverter для более гибкого преобразования
        var converter = System.ComponentModel.TypeDescriptor.GetConverter(targetType);
        if (converter.CanConvertFrom(typeof(string)))
        {
            var convertedValue = converter.ConvertFromInvariantString(value);
            property.SetValue(obj, convertedValue);
        }
        else
        {
            // Стандартное преобразование для примитивных типов
            var convertedValue = Convert.ChangeType(value, targetType);
            property.SetValue(obj, convertedValue);
        }
    }
}