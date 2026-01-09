using System.Text.Json.Nodes;

namespace Lira.Common;

public static class ObjectPathElemsExtension
{
    public static bool TryGetStringValue(
        this ObjectPath path,
        JsonNode node,
        out string? result)
    {
        result = null;
        if (TryGetNode(path, node, out var resultNode))
        {
            result = resultNode?.ToString();
            return true;
        }

        return false;
    }

    public static bool TryGetNode(
        this ObjectPath path,
        JsonNode node,
        out JsonNode? result)
    {
        result = null;
        JsonNode? currentNode = node;

        foreach (var pathElem in path.Elems)
        {
            if (currentNode == null)
                return false;

            if (currentNode is JsonValue)
                return false;

            if (pathElem is ObjectPath.Elem.Array array)
            {
                var arrayNode = currentNode[array.Name] as JsonArray;

                if (arrayNode == null || array.Index >= arrayNode.Count)
                    return false;

                currentNode = arrayNode[array.Index];
            }
            else if (pathElem is ObjectPath.Elem.Field field)
            {
                // Обычное свойство объекта
                currentNode = currentNode[field.Name];
            }
        }

        if (currentNode == null)
            return false;

        result = currentNode;

        return true;
    }
}