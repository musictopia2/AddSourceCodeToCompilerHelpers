using Microsoft.CodeAnalysis;
namespace AddSourceCodeToCompilerHelpers;
internal static class Extensions
{

    public static bool DidIncludeCodeAtLeastOnce(this Compilation compilation)
    {
        return compilation.SyntaxTrees.Any(xx =>
        {
            string text = xx.ToString();
            text = text.Replace("//[IncludeCode", "");
            return text.Contains("[IncludeCode");
        });
    }
    public static bool DidIncludeCode(this SyntaxTree syntax)
    {
        string text = syntax.ToString();
        text = text.Replace("//[IncludeCode", "");
        return text.Contains("[IncludeCode");
    }
    public static string GetCSharpString(this string content)
    {
        content = content.Replace("\"", "\"\"");
        return content;
    }
    public static string RemoveAttribute(this string content, string attributeName)
    {
        content = content.Replace($"    [{attributeName}]{Environment.NewLine}", "");
        content = content.Replace($"[{attributeName}]{Environment.NewLine}", "");
        return content;
    }
}