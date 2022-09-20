using System;
using System.IO;
using System.Linq;

namespace eShopCloudNative.Architecture.SourceGen
{
    public static class CodeWriterExtensions
    {
        public static StringWriter WriteNamespace(this StringWriter writer, string namespaceName)
        {
            writer.WriteLine($@"namespace {namespaceName};");
            return writer;
        }
        public static StringWriter WriteEmptyLine(this StringWriter writer)
        {
            writer.WriteLine();
            return writer;
        }

        public static StringWriter WriteClassDeclaration(this StringWriter writer, string className, string modifiers, string[] baseTypes = null, Action<StringWriter> child = null)
        {
            writer.Write($"{modifiers} class {className}");
            if (baseTypes != null && baseTypes.Any())
            {
                writer.Write($"{string.Join(", ", baseTypes)}");
            }
            writer.WriteEmptyLine();
            writer.WriteLine("{");
            child?.Invoke(writer);
            writer.WriteLine("}");

            return writer;
        }

        public static StringWriter DeclareMembers(this StringWriter writer, Member[] members)
        {
            foreach (var member in members)
            {
                writer.DeclareMember(member);
            }
            return writer;
        }

        public static StringWriter DeclareMember(this StringWriter writer, Member members)
        {
            writer.WriteLine($@"    {members.Modifiers} {members.Type} {members.Name};");
            return writer;
        }

        public static StringWriter DeclareMember(this StringWriter writer, string type, string modifiers, string name)
        {
            writer.WriteLine($@"    {modifiers} {type} {name};");
            return writer;
        }

        public static StringWriter WriteConstructor(this StringWriter writer, string modifiers, string name,  Member[] members = null, Action<StringWriter> pre = null, Action<StringWriter> pos = null)
        {
            pre?.Invoke(writer);

            string constructorParameters = members != null
                ? string.Join(", ", members.Select(it => $"{it.Type} {it.Name}").ToArray())
                : string.Empty;

            writer.WriteLine($@"    {modifiers} {name}({constructorParameters})");
            writer.WriteLine($@"    {{");
            if (members != null)
            {
                //atribuição dentro do construtor
                foreach (var member in members)
                {
                    writer.WriteLine($@"        this.{member.Name} = {member.Name};");
                }
            }
            writer.WriteLine($@"    }}");

            pos?.Invoke(writer);
            return writer;
        }

    }

    public class Member { 

        public string Modifiers { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

    }
}
