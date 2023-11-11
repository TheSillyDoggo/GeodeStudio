using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GeodeIDE.MainWindow;

namespace GeodeIDE
{
    public class ProjectManager
    {
        private static ProjectManager Instance;
        public static ProjectManager Get() { return Instance; }

        public ProjectWindow window;
        public ProjectItem project;
        public string Directory = "";
        public CCLayer layer;

        public string GetDirectoryString()
        {
            string str = "Root/";

            if (Directory != "") 
                str += Directory.Replace("\\", "/") + "/";

            return str;
        }

        public static void Create()
        {
            Instance = new ProjectManager();
        }

        public void LoadWindow(Window windowS)
        {
            this.window = new ProjectWindow();

            this.window.Show();
            windowS.Close();
            window.Title.Content = $"{project.name} - Geode Studio";
        }

        public void OpenDirectory(Panel a)
        {
            Directory = Path.Combine(Directory, a.Name);
            window.RefreshProject();
        }
    }
}
