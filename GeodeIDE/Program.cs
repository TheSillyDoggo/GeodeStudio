using Avalonia;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;

namespace GeodeIDE
{
    internal class Program
    {
        private static string gdPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Geometry Dash";
        public static string GetGDPath () { return gdPath; }
        public static string GetGDResourcesPath () { return Path.Combine(gdPath, "Resources\\"); }
        public static List<CroppedBitmap> alignmentIcons = new List<CroppedBitmap>();
        public static Dictionary<string, Bitmap> gdAssets = new Dictionary<string, Bitmap>();
        public static Dictionary<string, Dictionary<CroppedBitmap, bool>> gdSheetAssets = new Dictionary<string, Dictionary<CroppedBitmap, bool>>();
        public static List<bool> gdSheetRotated = new List<bool>();

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            ProjectManager.Create();

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}