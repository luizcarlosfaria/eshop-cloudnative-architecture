using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System;
using System.Text;
using System.Linq;
using CaseExtensions;
using System.Collections.Generic;

namespace eShopCloudNative.Architecture.SourceGen
{
    [Generator]
    public class ControllerGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a factory that can create our custom syntax receiver
            context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // the generator infrastructure will create a receiver and populate it
            // we can retrieve the populated instance via the context
            MySyntaxReceiver syntaxReceiver = (MySyntaxReceiver)context.SyntaxReceiver;

            // get the recorded user class
            ClassDeclarationSyntax targetClass = syntaxReceiver.ClassToAugment;
            if (targetClass is null)
            {
                // if we didn't find the user class, there is nothing to do
                return;
            }

            string classNamespace = SourceGenUtility.GetNamespace(targetClass);

            var services = targetClass.BaseList?.Types.Where(it => it.Type is IdentifierNameSyntax id && id.Identifier.Text.StartsWith("I") && id.Identifier.Text.EndsWith("Service"))
                .Select(it => it.Type)
                .Cast<IdentifierNameSyntax>()
                .ToList();

            System.IO.StringWriter writer = new System.IO.StringWriter();
            writer
                .WriteNamespace(classNamespace)
                .WriteEmptyLine()
                .WriteClassDeclaration(targetClass.Identifier.Text, "public partial", child: w =>
                {
                    var members = services.Select(it => new Member()
                    {
                        Modifiers = "private readonly",
                        Type = it.Identifier.Text,
                        Name = it.Identifier.Text.Substring(1).ToCamelCase()
                    }).ToArray();

                    w
                    .DeclareMembers(members)
                    .WriteEmptyLine()
                    .WriteConstructor("public", targetClass.Identifier.Text, members)
                    .WriteEmptyLine();

                });




            context.AddSource($"{targetClass.Identifier}.g.cs", SourceText.From(writer.ToString(), Encoding.UTF8));
        }

        class MySyntaxReceiver : ISyntaxReceiver
        {
            public ClassDeclarationSyntax ClassToAugment { get; private set; }

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                // Business logic to decide what we're interested in goes here
                if (syntaxNode is ClassDeclarationSyntax cds
                    && cds.Identifier.ValueText.EndsWith("Controller")
                    && cds.Modifiers.Check(
                        (m) => m.Contains("public"),
                        (m) => m.Contains("partial"),
                        (m) => !m.Contains("abstract")
                    )
                    && cds.BaseList?.Types != null
                    && cds.BaseList.Types.Any(it => it.Type is IdentifierNameSyntax id && id.Identifier.Text.StartsWith("I") && id.Identifier.Text.EndsWith("Service"))
                    )
                {
                    this.ClassToAugment = cds;
                }
            }
        }
    }

}
