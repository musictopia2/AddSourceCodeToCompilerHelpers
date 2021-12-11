using Microsoft.CodeAnalysis;
using System.Text;
namespace AddSourceCodeToCompilerHelpers;
[Generator]
public class MySourceGenerator : ISourceGenerator
{
    private void AppendToBuilder(ref StringBuilder builder, SyntaxTree tree)
    {
        string text = tree.ToString();
        string spaces = "        ";
        string content = text.GetCSharpString();
        content = content.RemoveAttribute("IncludeCode");
        content = content.RemoveAttribute("IgnoreCode");
        builder.AppendLine($@"{spaces}text = @""{content}"";");
        builder.AppendLine($"{spaces}compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(text,Encoding.UTF8), options));");
    }
    public void Execute(GeneratorExecutionContext context)
    {
        Compilation compilation = context.Compilation;
        bool includeglobal = compilation.DidIncludeCodeAtLeastOnce();
        if (includeglobal == false)
        {
            return;
        }
        StringBuilder builder = new();
        //extension upon compilation.  so it can be used outside of source generators.
        builder.AppendLine("using Microsoft.CodeAnalysis;");
        builder.AppendLine("using Microsoft.CodeAnalysis.CSharp;");
        builder.AppendLine("using Microsoft.CodeAnalysis.Text;");
        builder.AppendLine("using System.Text;");
        builder.AppendLine($"namespace {compilation.AssemblyName};");
        builder.AppendLine("internal static class CompilationExtensions");
        builder.AppendLine("{");
        builder.AppendLine("    internal static Compilation GetCompilationWithHelpers(this Compilation oldcompilation)");
        builder.AppendLine("    {");
        builder.AppendLine("        string text;");
        builder.AppendLine("        var options = oldcompilation.SyntaxTrees.First().Options as CSharpParseOptions;");
        builder.AppendLine("        Compilation compilation = oldcompilation;");
        bool hadOne = false;
        foreach (var item in compilation.SyntaxTrees)
        {
            if (item.ToString().Contains("namespace ") == false)
            {
                continue; //because there was none.
            }
            if (item.ToString().Contains("ISourceGenerator"))
            {
                continue; //you cannot generate source code for itself obviously
            }
            if (item.ToString().Contains("ISyntaxReceiver"))
            {
                continue; //you cannot generate source code for syntax receivers
            }
            bool includSingle = item.DidIncludeCode();
            if (includSingle == false)
            {
                continue;
            }
            AppendToBuilder(ref builder, item);
            hadOne = true;
        }
        if (hadOne == false)
        {
            return;
        }
        builder.AppendLine("        return compilation;");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        string results = builder.ToString();
        context.AddSource("compliationlibrary.g", results);
    }
    public void Initialize(GeneratorInitializationContext context)
    {
    }
}