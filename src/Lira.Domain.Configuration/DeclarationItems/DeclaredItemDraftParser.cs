using Lira.Common.Extensions;

namespace Lira.Domain.Configuration.DeclarationItems;

static class DeclaredItemDraftOrderer
{
    class Node(DeclaredItemDraft draft)
    {
        public DeclaredItemDraft Draft { get; } = draft;
        public HashSet<Node> DependsOn { get; } = new();
        public bool LoopDetectedFlag { get; set; }
    }

    public static IReadOnlySet<DeclaredItemDraft> OrderByDependencies(this IReadOnlySet<DeclaredItemDraft> drafts)
    {
        var nodes = new Dictionary<string, Node>();

        // generated graphs for each draft
        foreach (var draft in drafts)
        {
            if (!nodes.TryGetValue(draft.Name, out var node))
            {
                node = new Node(draft);
                nodes.Add(draft.Name, node);
            }

            foreach (var dr in drafts)
            {
                if (!draft.Pattern.Contains(dr.Name))
                    continue;

                // if (draft.Equals(dr))
                //     throw new Exception($"Declaration '{dr.Name}' refers to itself");

                if (!nodes.TryGetValue(dr.Name, out var dependsOn))
                {
                    dependsOn = new Node(dr);
                    nodes.Add(dr.Name, dependsOn);
                }

                node.DependsOn.Add(dependsOn);
            }
        }

        if (drafts.Count != nodes.Count)
            throw new Exception($"The count of nodes ({nodes.Count}) and drafts ({drafts.Count}) does not match");

        // checking loops
        foreach (var (_, node) in nodes)
        {
            foreach (var (_, n) in nodes)
            {
                n.LoopDetectedFlag = false;
            }

            RecursiveTraversal(node.Draft.Name, node, (current, route) =>
            {
                if (current.LoopDetectedFlag)
                    throw new Exception("Circular reference detected in declaration: " + route);

                current.LoopDetectedFlag = true;
            });
        }

        // generate ordered set

        var ordered = new OrderedHashSet<DeclaredItemDraft>();

        // for limit the number of iterations
        foreach (var _ in nodes)
        {
            foreach (var (_,node) in nodes)
            {
                if (node.DependsOn.Count == 0 || ordered.ContainsAll(node.DependsOn.Select(x => x.Draft)))
                {
                    ordered.Add(node.Draft);
                }
            }
        }

        if (drafts.Count != ordered.Count)
            throw new Exception($"The count of ordered result ({ordered.Count}) and drafts ({drafts.Count}) does not match");

        return ordered;
    }

    static void RecursiveTraversal(string route, Node node, Action<Node, string> handle)
    {
        handle(node, route);
        foreach (Node n in node.DependsOn)
        {
            RecursiveTraversal(route + " -> " + n.Draft.Name, n, handle);
        }
    }
}