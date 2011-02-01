using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace PixelLab.Common
{
    public class FunctionExportDefinition<T> : ExportDefinition
    {
        private FunctionExportDefinition(ExportDefinition definition, ComposablePartDefinition partDefinition)
            : base(definition.ContractName, definition.Metadata)
        {
            m_exportDefinition = definition;
            m_partDefinition = partDefinition;
        }

        public static IEnumerable<FunctionExportDefinition<T>> GetExports(ComposablePartCatalog catalog)
        {
            return
              from part in catalog.Parts
              from definition in part.ExportDefinitions
              where definition.Metadata.ContainsKey(CompositionConstants.ExportTypeIdentityMetadataName)
              where typeof(T).FullName == definition.Metadata[CompositionConstants.ExportTypeIdentityMetadataName] as string
              select new FunctionExportDefinition<T>(definition, part);
        }

        public T GetValue()
        {
            return (T)m_partDefinition.CreatePart().GetExportedValue(m_exportDefinition);
        }

        private readonly ExportDefinition m_exportDefinition;
        private readonly ComposablePartDefinition m_partDefinition;
    }

    public static class CompositionHelpers
    {
        public static IEnumerable<FunctionExportDefinition<T>> GetExports<T>(this ComposablePartCatalog catalog)
        {
            return FunctionExportDefinition<T>.GetExports(catalog);
        }
    }
}
