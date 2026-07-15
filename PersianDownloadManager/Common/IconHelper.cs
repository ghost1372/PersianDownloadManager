using Microsoft.UI.Xaml.Media.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Shell;

namespace PersianDownloadManager.Common;

internal static partial class IconHelper
{
    public unsafe static WriteableBitmap GetCaptureWriteableBitmap(IntPtr hBitmap)
    {
        WriteableBitmap writeableBitmap = null;

        if (hBitmap != IntPtr.Zero)
        {
            BITMAP bm;
            PInvoke.GetObject(new HGDIOBJ(hBitmap), Unsafe.SizeOf<BITMAP>(), &bm);

            int nWidth = bm.bmWidth;
            int nHeight = bm.bmHeight;
            BITMAPV5HEADER bi = new BITMAPV5HEADER();
            bi.bV5Size = (uint)Unsafe.SizeOf<BITMAPV5HEADER>();
            bi.bV5Width = nWidth;
            bi.bV5Height = -nHeight;
            bi.bV5Planes = 1;
            bi.bV5BitCount = 32;
            bi.bV5Compression = Windows.Win32.Graphics.Gdi.BI_COMPRESSION.BI_BITFIELDS;
            bi.bV5AlphaMask = unchecked((uint)0xFF000000);
            bi.bV5RedMask = 0x00FF0000;
            bi.bV5GreenMask = 0x0000FF00;
            bi.bV5BlueMask = 0x000000FF;

            HDC hDC = PInvoke.CreateCompatibleDC(HDC.Null);
            HGDIOBJ hBitmapOld = PInvoke.SelectObject(hDC, new HGDIOBJ(hBitmap));

            try
            {
                int nNumBytes = (int)(nWidth * 4 * nHeight);
                byte[] pPixels = new byte[nNumBytes];

                fixed (byte* pPixelData = pPixels)
                {
                    int scanLines = PInvoke.GetDIBits(hDC, new HBITMAP(hBitmap), 0, (uint)nHeight, pPixelData, (BITMAPINFO*)&bi, DIB_USAGE.DIB_RGB_COLORS);
                }

                // Convert straight alpha to premultiplied alpha expected by XAML/WinUI
                PremultiplyAlphaInPlace(pPixels);

                writeableBitmap = new WriteableBitmap(nWidth, nHeight);
                using (var stream = writeableBitmap.PixelBuffer.AsStream())
                {
                    stream.Write(pPixels, 0, pPixels.Length);
                }
            }
            finally
            {
                PInvoke.SelectObject(hDC, hBitmapOld);
                PInvoke.DeleteDC(hDC);
            }
        }

        return writeableBitmap;
    }

    private static void PremultiplyAlphaInPlace(byte[] pixels)
    {
        if (pixels == null) return;
        int len = pixels.Length;
        for (int i = 0; i + 3 < len; i += 4)
        {
            byte b = pixels[i];
            byte g = pixels[i + 1];
            byte r = pixels[i + 2];
            byte a = pixels[i + 3];
            if (a == 255) continue; // already fully opaque
            if (a == 0)
            {
                pixels[i] = pixels[i + 1] = pixels[i + 2] = 0;
                continue;
            }
            // multiply color channels by alpha (with rounding)
            pixels[i] = (byte)((b * a + 127) / 255);
            pixels[i + 1] = (byte)((g * a + 127) / 255);
            pixels[i + 2] = (byte)((r * a + 127) / 255);
        }
    }

    public static SHFILEINFOW GetFileInfo(string extension)
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<SHFILEINFOW>()];

        Windows.Win32.PInvoke.SHGetFileInfo(extension, Windows.Win32.Storage.FileSystem.FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL, buffer, Windows.Win32.UI.Shell.SHGFI_FLAGS.SHGFI_TYPENAME | Windows.Win32.UI.Shell.SHGFI_FLAGS.SHGFI_USEFILEATTRIBUTES | SHGFI_FLAGS.SHGFI_ICON | SHGFI_FLAGS.SHGFI_LARGEICON);

        ref SHFILEINFOW info = ref MemoryMarshal.AsRef<SHFILEINFOW>(buffer);

        return info;
    }

    /// <summary>
    /// Returns only the shell file type name without requesting an icon.
    /// Use this instead of <see cref="GetFileInfo"/> when no icon handle is needed,
    /// to avoid leaking the GDI icon handle that <c>SHGFI_ICON</c> allocates.
    /// </summary>
    public static string GetFileTypeName(string extension)
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<SHFILEINFOW>()];

        Windows.Win32.PInvoke.SHGetFileInfo(extension, Windows.Win32.Storage.FileSystem.FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL, buffer, Windows.Win32.UI.Shell.SHGFI_FLAGS.SHGFI_TYPENAME | Windows.Win32.UI.Shell.SHGFI_FLAGS.SHGFI_USEFILEATTRIBUTES);

        ref SHFILEINFOW info = ref MemoryMarshal.AsRef<SHFILEINFOW>(buffer);

        return info.szTypeName.ToString();
    }
}
