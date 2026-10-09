using PdfSharp.Fonts;

namespace Espuma.Application.Common
{
    /// <summary>Resolves the fonts embedded in this assembly so text rendering works on every platform.</summary>
    internal sealed class EmbeddedFontResolver : IFontResolver
    {
        public const string Roboto = "Roboto";
        private const string RobotoRegularFace = "Roboto#Regular";
        private const string RobotoRegularResource = "Espuma.Application.Resources.Fonts.Roboto-Regular.ttf";

        private static readonly Lock RegisterLock = new();

        public static void EnsureRegistered()
        {
            lock (RegisterLock)
            {
                GlobalFontSettings.FontResolver ??= new EmbeddedFontResolver();
            }
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic) =>
            familyName.Equals(Roboto, StringComparison.OrdinalIgnoreCase)
                ? new FontResolverInfo(RobotoRegularFace)
                : null;

        public byte[]? GetFont(string faceName)
        {
            if (faceName != RobotoRegularFace)
                return null;

            using var stream = typeof(EmbeddedFontResolver).Assembly.GetManifestResourceStream(RobotoRegularResource)
                ?? throw new InvalidOperationException($"Recurso de fuente no encontrado: {RobotoRegularResource}");
            var data = new byte[stream.Length];
            stream.ReadExactly(data);
            return data;
        }
    }
}
