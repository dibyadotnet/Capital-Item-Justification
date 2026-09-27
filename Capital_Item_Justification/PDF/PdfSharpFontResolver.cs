using PdfSharp.Fonts;

namespace Capital_Item_Justification.PDF
{
    public class PdfSharpFontResolver : IFontResolver
    {
        public byte[] GetFont(string faceName)
        {
            var fontPath = faceName switch
            {
                "Arial#Regular" =>
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.Fonts),
                        "arial.ttf"),

                "Arial#Bold" =>
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.Fonts),
                        "arialbd.ttf"),

                "Arial#Italic" =>
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.Fonts),
                        "ariali.ttf"),

                "Arial#BoldItalic" =>
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.Fonts),
                        "arialbi.ttf"),

                _ => throw new InvalidOperationException(
                    $"Font '{faceName}' not found.")
            };

            if (!File.Exists(fontPath))
            {
                throw new FileNotFoundException(
                    $"Font file not found: {fontPath}");
            }

            return File.ReadAllBytes(fontPath);
        }

        public FontResolverInfo? ResolveTypeface(
            string familyName,
            bool isBold,
            bool isItalic)
        {
            if (!familyName.Equals(
                    "Arial",
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (isBold && isItalic)
                return new FontResolverInfo("Arial#BoldItalic");

            if (isBold)
                return new FontResolverInfo("Arial#Bold");

            if (isItalic)
                return new FontResolverInfo("Arial#Italic");

            return new FontResolverInfo("Arial#Regular");
        }
    }
}

