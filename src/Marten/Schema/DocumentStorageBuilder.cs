using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FubuCore;
using Marten.Codegen;
using Npgsql;
using NpgsqlTypes;
using Remotion.Linq;

namespace Marten.Schema
{
    public static class DocumentStorageBuilder
    {
        public static IDocumentStorage Build(Type documentType)
        {
            return Build(new DocumentMapping(documentType));
        }

        public static IDocumentStorage Build(DocumentMapping mapping)
        {
            return Build(new[] {mapping}).Single();
        }

        public static IEnumerable<IDocumentStorage> Build(DocumentMapping[] mappings)
        {
            var code = GenerateDocumentStorageCode(mappings);

            var generator = new AssemblyGenerator();
            generator.ReferenceAssembly(Assembly.GetExecutingAssembly());
            generator.ReferenceAssemblyContainingType<NpgsqlConnection>();
            generator.ReferenceAssemblyContainingType<QueryModel>();
            generator.ReferenceAssemblyContainingType<DbCommand>();
            generator.ReferenceAssemblyContainingType<Component>();

            mappings.Select(x => x.DocumentType.Assembly).Distinct().Each(assem => generator.ReferenceAssembly(assem));

            var assembly = generator.Generate(code);

            return assembly
                .GetExportedTypes()
                .Where(x => x.IsConcreteTypeOf<IDocumentStorage>())
                .Select(x => Activator.CreateInstance(x).As<IDocumentStorage>());
        }

        

        public static string GenerateDocumentStorageCode(DocumentMapping[] mappings)
        {
            var writer = new SourceWriter();

            // TODO -- get rid of the magic strings
            var namespaces = new List<string> {"System", "Marten", "Marten.Schema", "Marten.Linq", "Marten.Util", "Npgsql", "Remotion.Linq", typeof(NpgsqlDbType).Namespace};
            namespaces.AddRange(mappings.Select(x => x.DocumentType.Namespace));

            namespaces.Distinct().OrderBy(x => x).Each(x => writer.WriteLine($"using {x};"));
            writer.BlankLine();

            writer.StartNamespace("Marten.GeneratedCode");

            mappings.Each(x =>
            {
                x.GenerateDocumentStorage(writer);
                writer.BlankLine();
                writer.BlankLine();
            });

            writer.FinishBlock();

            return writer.Code();
        }
    }
}