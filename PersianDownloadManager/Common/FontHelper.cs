using Microsoft.UI.Xaml.Media;

namespace PersianDownloadManager.Common;

public static partial class FontHelper
{
    public static List<double> FontSize { get; } = Enumerable.Range(6, 48).Select(x => (double)x).ToList();

    public static List<FontOption> UIFonts { get; } = new()
    {
        new FontOption("IRANSansXFont", "IRANSans"),
        new FontOption("IRANYekanFont", "IRANYekan"),
        new FontOption("VazirmatnFont", "Vazirmatn"),
    };
    
    public static void SetUIFontFamily(FontOption font)
    {
        var uiFont = Application.Current.Resources[font.FontKey] as FontFamily;
        Application.Current.Resources["PersianDownloadManagerFont"] = new FontFamily(uiFont.Source);
        Application.Current.Resources["XamlAutoFontFamily"] = new FontFamily(uiFont.Source);
        Application.Current.Resources["ContentControlThemeFontFamily"] = new FontFamily(uiFont.Source);
    }
}
