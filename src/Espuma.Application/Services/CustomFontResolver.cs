using PdfSharp.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Espuma.Application.Services
{
    public class CustomFontResolver : IFontResolver
    {
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("Roboto", StringComparison.OrdinalIgnoreCase))
                return new FontResolverInfo("Roboto#Regular");

            return null!;
        }

        public byte[] GetFont(string faceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = faceName switch
            {
                "Roboto#Regular" => "Espuma.Application.Resources.Fonts.Roboto-Regular.ttf",
                _ => throw new InvalidOperationException("Fuente no soportada")
            };

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new FileNotFoundException($"Fuente no encontrada: {resourceName}");
            }

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
