using System.Text;

namespace LprSolver.Extensions;

public static class StringBuilderExtensions
{
    public static StringBuilder AppendTitle(this StringBuilder builder, string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return builder;
        }

        builder.AppendLine("");
        builder.AppendLine($"--- {title.Trim()} ---");
        builder.AppendLine("");

        return builder;
    }

    public static StringBuilder AppendListLines(
        this StringBuilder builder,
        IEnumerable<string> lines,
        string customListSymbol = ""
    )
    {
        foreach (string line in lines)
        {
            builder.AppendLine($"{customListSymbol}{line}");
        }

        return builder;
    }

    public static StringBuilder AppendEnums(
        this StringBuilder builder,
        List<Enum> enums,
        string customListSymbol = ""
    )
    {
        foreach (var en in enums)
        {
            builder.AppendLine($"{customListSymbol}{en.ToString()}");
        }

        return builder;
    }
}
