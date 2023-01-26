using System.Linq;
using Kiota.Builder.CodeDOM;
using Kiota.Builder.Extensions;

namespace Kiota.Builder.Writers.TypeScript;
public class CodeNameSpaceWriter : BaseElementWriter<CodeNamespace, TypeScriptConventionService>
{
    public CodeNameSpaceWriter(TypeScriptConventionService conventionService) : base(conventionService) { }

    /// <summary>
    /// Writes export statements for classes and enums belonging to a namespace into a generated index.ts file. 
    /// The classes should be export in the order of inheritance so as to avoid circular dependency issues in javascript.
    /// </summary>
    /// <param name="codeElement">Code element is a code namespace</param>
    /// <param name="writer"></param>
    public override void WriteCodeElement(CodeNamespace codeElement, LanguageWriter writer)
    {
        foreach (var codeFunction in codeElement.Functions)
        {
            writer.WriteLine($"export * from './{codeFunction.Name.ToFirstCharacterLowerCase()}'");
        }

        foreach (var e in codeElement.Enums)
        {
            writer.WriteLine($"export * from './{e.Name.ToFirstCharacterLowerCase()}'");
        }
        foreach (var c in codeElement.CodeInterfaces)
        {
            writer.WriteLine($"export * from './{c.Name.ToFirstCharacterLowerCase()}'");
        }
        var requestBuilder = codeElement.Classes.FirstOrDefault(x => x.Kind == CodeClassKind.RequestBuilder);

        if (requestBuilder != null) {
            WriteCodeDeclaration(requestBuilder, writer);
        }
    }

    private void WriteCodeDeclaration(CodeClass requestBuilder, LanguageWriter writer)
    {
        var parentRequestBuilder = (requestBuilder.Parent.Parent as CodeNamespace).Classes.FirstOrDefault(x => x.Kind == CodeClassKind.RequestBuilder);
        if (parentRequestBuilder != null) {
            writer.WriteLine($"declare module \"../{parentRequestBuilder.Name}{{");
            writer.IncreaseIndent();
            writer.WriteLine($"interface {parentRequestBuilder.Name}{{");
            writer.IncreaseIndent();
            writer.WriteLine($"{requestBuilder.Name.Split("RequestBuilder")[0].ToFirstCharacterLowerCase()}:{requestBuilder.Name.ToFirstCharacterLowerCase()}");
            writer.WriteLine("}");
            writer.IncreaseIndent();
            writer.WriteLine("}");
        }
    }

//    Reflect.defineProperty(UserItemRequestBuilder.prototype, "settings", {
//    configurable: true,
//    enumerable: true,
//    get: function(this: UserItemRequestBuilder)
//    {
//        return this.create(SettingsRequestBuilder);
//    },
//});

    




}
