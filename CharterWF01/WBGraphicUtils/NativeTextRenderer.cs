// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NativeTextRenderer.cs" company="Wynand">
//   
//   Permission is hereby granted, free of charge, to any person obtaining annotation
//   copy of this software and associated documentation files (the
//   "Software"), to deal in the Software without restriction, including
//   without limitation the rights to use, copy, modify, merge, publish,
//   distribute, sublicense, and/or sell copies of the Software, and to
//   permit persons to whom the Software is furnished to do so, subject to
//   the following conditions:
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//   OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//   MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//   IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//   CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//   TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//   SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// <summary>
//   Obtained from the web, added XorDrawString.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace WBGraphicUtils
{
   using System;
   using System.Collections.Generic;
   using System.Drawing;
   using System.Runtime.InteropServices;

   /// <summary>     
   /// Wrapper for GDI text rendering functions compiled by WB from the Web<br/>     
   /// This class is not thread-safe as GDI function should be called from the UI thread.     
   ///  </summary>     
   public sealed class NativeTextRenderer : IDisposable
   {
      #region Fields and Consts

      /// <summary>     
      /// cache of all the font used not to create same font again and again     
      /// </summary>     
      private static readonly Dictionary<string, Dictionary<float, Dictionary<FontStyle, IntPtr>>> _fontsCache = new Dictionary<string, Dictionary<float, Dictionary<FontStyle, IntPtr>>>(StringComparer.InvariantCultureIgnoreCase);

      /// <summary>     
      /// The wrapped WinForms graphics object     
      /// </summary>     
      private readonly Graphics graphics;

      /// <summary>     
      /// the initialized HDC used     
      /// </summary>     
      private IntPtr hdc;

      #endregion


      /// <summary>     
      /// Init.     
      /// </summary>     
      public NativeTextRenderer(Graphics g)
      {
         graphics = g;
         var clip = graphics.Clip.GetHrgn(graphics);

         hdc = graphics.GetHdc();
         SetBkMode(hdc, 1);
         SelectClipRgn(hdc, clip);
         DeleteObject(clip);
      }

      /// <summary>     
      /// Measure the width and height of string  <paramref name="str"/> when drawn on device context HDC     
      /// using the given font <paramref  name="font"/>.     
      /// </summary>     
      /// <param name="str">the  string to measure</param>     
      /// <param name="font">the  font to measure string with</param>     
      /// <returns>the size of the  string</returns>     
      public Size MeasureString(string str, Font font)
      {
         SetFont(font);

         var size = new Size();
         GetTextExtentPoint32(hdc, str, str.Length, ref size);
         return size;
      }

      /// <summary>     
      /// Measure the width and height of string <paramref name="str"/> when drawn on device context HDC     
      /// using the given font <paramref name="font"/>.<br/>     
      /// Restricts the width of the string and gets the number of characters able to fit in the restriction and     
      /// the width that those characters takes.     
      /// </summary>     
      /// <param name="str">The string to measure</param>     
      /// <param name="font">The font to measure the string with</param>     
      /// <param  name="maxWidth">The max width to render the string in</param>     
      /// <param  name="CharsThatFitCount">The number of characters that will fit inside the <see cref="maxWidth"/> restriction</param>     
      /// <param  name="widthUsed"></param>     
      /// <returns>The size of the string</returns>     
      public Size MeasureString(string str, Font font, float maxWidth, out int CharsThatFitCount, out int widthUsed)
      {
         int[] _charFit = new int[1];
         int[] _charFitWidth = new int[1000]; 
         
         SetFont(font);

         var size = new Size();
         GetTextExtentExPoint(hdc, str, str.Length, (int)Math.Round(maxWidth), _charFit, _charFitWidth, ref size);
         CharsThatFitCount = _charFit[0];
         widthUsed = CharsThatFitCount > 0 ? _charFitWidth[CharsThatFitCount - 1] : 0;
         return size;
      }

      /// <summary>     
      /// Draws the given string using the given font and foreground color at the given location.     
      /// </summary>     
      /// <param name="str">The string to draw</param>     
      /// <param name="font">The font to use to draw the string</param>     
      /// <param name="color">The text color to set</param>     
      /// <param name="point">The location to start string draw (top-LeftThumbPoint)</param>     
      public void DrawString(String str, Font font, Color color, Point point)
      {
         SetFont(font);
         SetTextColor(color);
         TextOut(hdc, point.X, point.Y, str, str.Length);
      }

      /// <summary>     
      /// Draw the given string using the given  font and foreground color at given location.<br/>     
      /// See [http://msdn.microsoft.com/en-us/library/windows/desktop/dd162498(v=vs.85).aspx][12].     
      /// </summary>     
      /// <param name="str">the  string to draw</param>     
      /// <param name="font">the  font to use to draw the string</param>     
      /// <param name="color">the  text color to set</param>     
      /// <param name="rect">the  rectangle in which the text is to be formatted</param>     
      /// <param name="flags">The  method of formatting the text</param>     
      public void DrawString(String str, Font font, Color color, Rectangle rect, TextFormatFlags flags)
      {
         SetFont(font);
         SetTextColor(color);
         var rect2 = new Rect(rect);
         DrawText(hdc, str, str.Length, ref rect2, (uint)flags);
      }

      /// <summary>     
      /// Special draw logic to draw transparent  text using GDI.<br/>     
      /// 1. Create in-memory DC<br/>     
      /// 2. Copy background to in-memory  DC<br/>     
      /// 3. Draw the text to in-memory  DC<br/>     
      /// 4. Copy the in-memory DC to the proper  location with alpha blend<br/>     
      /// </summary>     
      public void DrawTransparentText(string str, Font font, Color color, Point point, Size size)
      {
         // Create annotation memory DC so we can work off-screen     
         IntPtr memoryHdc = CreateCompatibleDC(hdc);
         SetBkMode(memoryHdc, 1);

         // Create annotation device-independent bitmap  and highlight it into our DC     
         var info = new BitMapInfo();
         info.biSize = Marshal.SizeOf(info);
         info.biWidth = size.Width;
         info.biHeight = -size.Height;
         info.biPlanes = 1;
         info.biBitCount = 32;
         info.biCompression = 0; // BI_RGB     
         IntPtr ppvBits;
         IntPtr dib = CreateDIBSection(hdc, ref  info, 0, out ppvBits, IntPtr.Zero, 0);
         SelectObject(memoryHdc, dib);

         try
         {
            // copy target background to memory  HDC so when copied back it will have the proper background     
            BitBlt(memoryHdc, 0, 0, size.Width, size.Height, hdc, point.X, point.Y, 0x00CC0020);

            // Create and highlight font     
            SelectObject(memoryHdc, GetCachedHFont(font));
            SetTextColor(memoryHdc, (color.B & 0xFF) << 16 | (color.G & 0xFF) << 8 | color.R);

            // Draw text to memory HDC     
            TextOut(memoryHdc, 0, 0, str, str.Length);

            // copy from memory HDC to normal  HDC with alpha blend so achieve the transparent text     
            AlphaBlend(hdc, point.X, point.Y, size.Width, size.Height, memoryHdc, 0, 0, size.Width, size.Height, new BlendFunction(color.A));
         }
         finally
         {
            DeleteObject(dib);
            DeleteDC(memoryHdc);
         }
      }

      /// <summary>     
      /// Release current HDC to be able to use  <see cref="Graphics"/> methods.     
      /// </summary>     
      public void Dispose()
      {
         if (hdc != IntPtr.Zero)
         {
            SelectClipRgn(hdc, IntPtr.Zero);
            graphics.ReleaseHdc(hdc);
            hdc = IntPtr.Zero;
         }
      }

      public void XorDrawString(String str, Font font, Point point)
      {
         SetFont(font);
         SetBkMode(hdc, System.Drawing.Color.Transparent.ToArgb());

         // save output to annotation Path
         BeginPath(hdc);
         TextOut(hdc, point.X, point.Y, str, str.Length);
         EndPath(hdc);
         SetROP2(hdc, RasterOps.R2_XORPEN);
         FillPath(hdc);
      }

      #region Private methods

      /// <summary>     
      /// Set annotation resource (mouseEventArgs.g. annotation font) for the  specified device context.     
      /// </summary>     
      private void SetFont(Font font)
      {
         SelectObject(hdc, GetCachedHFont(font));
      }

      /// <summary>     
      /// Get cached unmanaged font handle for  given font.<br/>     
      /// </summary>     
      /// <param name="font">the  font to get unmanaged font handle for</param>     
      /// <returns>handle to unmanaged  font</returns>     
      private static IntPtr GetCachedHFont(Font font)
      {
         IntPtr hfont = IntPtr.Zero;
         Dictionary<float, Dictionary<FontStyle, IntPtr>> dic1;
         if (_fontsCache.TryGetValue(font.Name, out dic1))
         {
            Dictionary<FontStyle, IntPtr> dic2;
            if (dic1.TryGetValue(font.Size, out  dic2))
            {
               dic2.TryGetValue(font.Style, out hfont);
            }
            else
            {
               dic1[font.Size] = new Dictionary<FontStyle, IntPtr>();
            }
         }
         else
         {
            _fontsCache[font.Name] = new Dictionary<float, Dictionary<FontStyle, IntPtr>>();
            _fontsCache[font.Name][font.Size] = new Dictionary<FontStyle, IntPtr>();
         }

         if (hfont == IntPtr.Zero)
         {
            _fontsCache[font.Name][font.Size][font.Style] = hfont = font.ToHfont();
         }

         return hfont;
      }

      /// <summary>     
      /// Set the text color of the device  context.     
      /// </summary>     
      private void SetTextColor(Color color)
      {
         int rgb = (color.B & 0xFF) << 16 | (color.G & 0xFF) << 8 | color.R;
         SetTextColor(hdc, rgb);
      }

      [DllImport("gdi32.dll")]
      private static extern int SetROP2(IntPtr hdc, RasterOps drawMode);

      [DllImport("gdi32.dll")]
      private static extern bool BeginPath(IntPtr hdc);

      [DllImport("gdi32.dll")]
      private static extern bool FillPath(IntPtr hdc);

      [DllImport("gdi32.dll")]
      private static extern bool EndPath(IntPtr hdc);

      [DllImport("gdi32.dll")]
      private static extern int SaveDC(IntPtr hdc);

      [DllImport("gdi32.dll")]
      private static extern int SetBkMode(IntPtr hdc, int mode);

      [DllImport("gdi32.dll")]
      private static extern int SelectObject(IntPtr hdc, IntPtr hgdiObj);

      [DllImport("gdi32.dll")]
      private static extern int SetTextColor(IntPtr hdc, int color);

      [DllImport("gdi32.dll", EntryPoint = "GetTextExtentPoint32W")]
      private static extern int GetTextExtentPoint32(IntPtr hdc, [MarshalAs(UnmanagedType.LPWStr)] string str, int len, ref Size size);

      [DllImport("gdi32.dll", EntryPoint = "GetTextExtentExPointW")]
      private static extern bool GetTextExtentExPoint(IntPtr hDc, [MarshalAs(UnmanagedType.LPWStr)]string str, int nLength, int nMaxExtent, int[] lpnFit, int[] alpDx, ref Size size);

      [DllImport("gdi32.dll", EntryPoint = "TextOutW")]
      private static extern bool TextOut(IntPtr hdc, int x, int y, [MarshalAs(UnmanagedType.LPWStr)] string str, int len);

      [DllImport("user32.dll", EntryPoint = "DrawTextW")]
      private static extern int DrawText(IntPtr hdc, [MarshalAs(UnmanagedType.LPWStr)] string str, int len, ref Rect rect, uint uFormat);

      [DllImport("gdi32.dll")]
      private static extern int SelectClipRgn(IntPtr hdc, IntPtr hrgn);

      [DllImport("gdi32.dll")]
      private static extern bool DeleteObject(IntPtr hObject);

      [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
      public static extern bool DeleteDC(IntPtr hdc);

      [DllImport("gdi32.dll")]
      [return: MarshalAs(UnmanagedType.Bool)]
      private static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

      [DllImport("gdi32.dll", EntryPoint = "GdiAlphaBlend")]
      private static extern bool AlphaBlend(IntPtr hdcDest, int nXOriginDest, int nYOriginDest, int nWidthDest, int nHeightDest, IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc, BlendFunction blendFunction);

      [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
      private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

      [DllImport("gdi32.dll")]
      private static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref BitMapInfo pbmi, uint iUsage, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);

      private struct Rect
      {
         private int _left;
         private int _top;
         private int _right;
         private int _bottom;

         public Rect(Rectangle r)
         {
            _left = r.Left;
            _top = r.Top;
            _bottom = r.Bottom;
            _right = r.Right;
         }
      }

      [StructLayout(LayoutKind.Sequential)]
      private struct BlendFunction
      {
         public byte BlendOp;
         public byte BlendFlags;
         public byte SourceConstantAlpha;
         public byte AlphaFormat;

         public BlendFunction(byte alpha)
         {
            BlendOp = 0;
            BlendFlags = 0;
            AlphaFormat = 0;
            SourceConstantAlpha = alpha;
         }
      }

      [StructLayout(LayoutKind.Sequential)]
      public struct BitMapInfo
      {
         public int biSize;
         public int biWidth;
         public int biHeight;
         public short biPlanes;
         public short biBitCount;
         public int biCompression;
         public int biSizeImage;
         public int biXPelsPerMeter;
         public int biYPelsPerMeter;
         public int biClrUsed;
         public int biClrImportant;
         public byte bmiColors_rgbBlue;
         public byte bmiColors_rgbGreen;
         public byte bmiColors_rgbRed;
         public byte bmiColors_rgbReserved;
      }

      #endregion
   }

   /// <summary>     
   /// See [http://msdn.microsoft.com/en-us/library/windows/desktop/dd162498(v=vs.85).aspx][12]     
   ///  </summary>     
   [Flags]
   public enum TextFormatFlags : uint
   {
      Default = 0x00000000,
      Center = 0x00000001,
      Right = 0x00000002,
      VCenter = 0x00000004,
      Bottom = 0x00000008,
      WordBreak = 0x00000010,
      SingleLine = 0x00000020,
      ExpandTabs = 0x00000040,
      TabStop = 0x00000080,
      NoClip = 0x00000100,
      ExternalLeading = 0x00000200,
      CalcRect = 0x00000400,
      NoPrefix = 0x00000800,
      Internal = 0x00001000,
      EditControl = 0x00002000,
      PathEllipsis = 0x00004000,
      EndEllipsis = 0x00008000,
      ModifyString = 0x00010000,
      RtlReading = 0x00020000,
      WordEllipsis = 0x00040000,
      NoFullWidthCharBreak = 0x00080000,
      HidePrefix = 0x00100000,
      ProfixOnly = 0x00200000,
   }
}