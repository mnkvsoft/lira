using System.Linq.Expressions;
using System.Reflection;

namespace Lira.Common;

public static class AttributeExtractor
{
    public static TAttribute Extract<TClass, TAttribute>(Expression<Func<TClass, object?>> propertyExpression)
        where TAttribute : Attribute
    {
        if (propertyExpression == null)
            throw new ArgumentNullException(nameof(propertyExpression));

        // Получаем информацию о свойстве из выражения
        var memberExpression = GetMemberExpression(propertyExpression);
        var propertyInfo = memberExpression.Member as PropertyInfo;

        if (propertyInfo == null)
            throw new ArgumentException("Выражение должно ссылаться на свойство", nameof(propertyExpression));

        // Ищем атрибут на свойстве
        var attribute = propertyInfo.GetCustomAttribute<TAttribute>();

        if (attribute == null)
            throw new InvalidOperationException($"Атрибут {typeof(TAttribute).Name} не найден на свойстве {propertyInfo.Name}");

        return attribute;
    }

    private static MemberExpression GetMemberExpression<TClass>(Expression<Func<TClass, object?>> expression)
    {
        var body = expression.Body;

        // Обработка преобразования типов (например, value type к object)
        if (body is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
        {
            body = unaryExpression.Operand;
        }

        if (body is not MemberExpression memberExpression)
            throw new ArgumentException("Выражение должно быть ссылкой на свойство или поле", nameof(expression));

        return memberExpression;
    }
}