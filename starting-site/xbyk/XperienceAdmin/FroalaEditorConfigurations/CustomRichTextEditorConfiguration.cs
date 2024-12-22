using Admin.FroalaEditorConfigurations;
using Kentico.Xperience.Admin.Base.Forms;

namespace Admin.FroalaEditorConfigurations
{
    public class CustomRichTextEditorConfiguration : RichTextEditorConfiguration
    {
        // Relative path to the configuration file within the specified assembly
        // This example configuration is located under: <AssemblyName>\Misc\RichTextEditorConfiguration\
        private const string CONFIGURATION_PATH = "FroalaEditorConfigurations\\CustomEditorConfigurations.json";

        // String identifier assigned to the configuration
        public const string IDENTIFIER = "Site.EditorConfiguration";

        // User-friendly name of the configuration
        public const string DISPLAY_NAME = "Custom Site Configuration";

        public CustomRichTextEditorConfiguration()
            : base(CONFIGURATION_PATH)
        {
        }
    }
}
