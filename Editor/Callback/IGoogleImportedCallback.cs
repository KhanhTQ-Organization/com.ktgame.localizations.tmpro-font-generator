using System.Collections.Generic;

namespace com.localizations.tmpro_font_generator.editor
{
    public interface IGoogleImportedCallback 
    {
        void OnImported(List<string> categories);
    }
}
