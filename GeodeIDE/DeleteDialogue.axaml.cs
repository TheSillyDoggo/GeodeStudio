using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Interactivity;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Drawing.Printing;
using Avalonia;
using System;
using System.Media;
using System.IO;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Media.Imaging;
using Avalonia.Media.TextFormatting;
using Newtonsoft.Json.Linq;

namespace GeodeIDE
{
    public partial class DeleteDialogue : Window
    {
        public static string dir;
        public static bool isDirectory;

        public DeleteDialogue()
        {
            this.InitializeComponent();

            CanResize = false;
            Topmost = true;
            ClientSize = new Avalonia.Size(400, 270);

            if (isDirectory)
            {
                Title.Content = "Delete Directory";
                Description.Text = "Are you sure you want to delete this Directory.\nThis will DELETE all the files in this folder.";
            }
            else
            {
                JObject rss = JObject.Parse(File.ReadAllText(dir));
                string type = (string)rss["type"];

                Title.Content = $"Delete {type.FirstCharToUpper()}";
                Description.Text = $"Are you sure you want to delete this {type.FirstCharToUpper()}";
            }
        }

        public void OnDelete(object source, RoutedEventArgs args)
        {
            if (isDirectory)
            {
                Debug.WriteLine($"Deleting {dir}");
                Directory.Delete(dir, true);
            }
            else
            {
                Debug.WriteLine($"Deleting {dir}");
                File.Delete(dir);
            }

            ProjectManager.Get().window.RefreshProject();

            this.Close();
        }

        public void OnCancel(object source, RoutedEventArgs args)
        {
            this.Close();
        }
    }
}