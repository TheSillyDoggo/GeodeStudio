using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeodeIDE
{
    public class LabelBMFont
    {
        public static Dictionary<string, Dictionary<string, Char>> fonts = new Dictionary<string, Dictionary<string, Char>>();

        public enum Alignment
        {
            Center, Left, Right
        }

        public class Char
        {
            public string character;
            public PixelRect rect;
            public int xAdvance;
            public PixelPoint offset;
            public CroppedBitmap bitmap;
        }

        public static void InitFont(string fontName, string fntFile, IImage image)
        {
            Debug.WriteLine($"Loading Font {fontName}");

            Dictionary<string, Char> dic = new Dictionary<string, Char>();

            for (int i = 0; i < fntFile.Split("\n".ToCharArray()).Length; i++)
            {
                string ln = fntFile.Split("\n".ToCharArray())[i];

                if (ln.StartsWith("char "))
                {
                    string c = ln.Substring(121, ln.Length - 121 - 1);
                    if (c == "space")
                    {
                        c = " ";
                    }

                    Char d = new Char();
                    d.character = c;
                    d.rect = new PixelRect(int.Parse(ln.Substring(17, 6).TrimEnd()), int.Parse(ln.Substring(25, 6).TrimEnd()), int.Parse(ln.Substring(37, 6).TrimEnd()), int.Parse(ln.Substring(50, 6).TrimEnd()));
                    d.offset = new PixelPoint(int.Parse(ln.Substring(64, 6).TrimEnd()), int.Parse(ln.Substring(78, 6).TrimEnd()));
                    d.xAdvance = int.Parse(ln.Substring(93, 6).TrimEnd());

                    dic.Add(c, d);

                    d.bitmap = new CroppedBitmap(image, d.rect);
                }
            }

            fonts.Add(fontName, dic);
        }

        public static Canvas CreateAsChildren(string text, string fntFile, float scale, Alignment al)
        {
            var canvas = new Canvas();

            if (text == null || text.Length == 0)
                return canvas;

            float xAd = 0;

            Dictionary<string, Char> ch;
            fonts.TryGetValue(fntFile, out ch);

            if (ch == null)
                return canvas;

            for (int i = 0; i < text.Length; i++)
            {
                Char e;

                ch.TryGetValue(text[i].ToString(), out e);

                if (e == null)
                {
                    ch.TryGetValue(text[i].ToString().ToUpper(), out e);

                    if (e == null)
                        continue;
                }

                if (e.character == " ")
                {
                    xAd += e.xAdvance * scale;
                    continue;
                }

                var img = new Image();
                img.Source = e.bitmap;
                img.Width = e.rect.Width * scale;
                img.Height = e.rect.Height * scale;
                img.Margin = new Thickness((xAd + e.offset.X * scale), (e.offset.Y * scale));

                img.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                img.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;

                canvas.Children.Add(img);

                xAd += ((e.xAdvance + e.offset.X) * scale);
            }

            switch (al)
            {
                case Alignment.Center:
                    canvas.Width = (xAd);
                    break;
                default: break;
            }

            return canvas;
        }
    }
}
