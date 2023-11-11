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
using Avalonia.Platform.Storage;

namespace GeodeIDE
{
    public partial class CreateDialogue : Window
    {
        public static int type;
        public static Bitmap? bmp = null;
        public string path;

        public CreateDialogue()
        {
            this.InitializeComponent();

            CanResize = false;
            Topmost = true;
            ClientSize = new Avalonia.Size(400, 270);

            switch (type)
            {
                case 0:
                    Title.Content = "Create Folder";
                    NewName.Watermark = "Folder Name";
                    break;
                case 1:
                    Title.Content = "Create Texture";
                    NewName.Watermark = "Texture Name";
                    SelectedImage.IsVisible = true;
                    ButtonImage.IsVisible = true;
                    ImageBG.IsVisible = true;
                    break;
                case 3:
                    Title.Content = "Create CCLayer";
                    NewName.Watermark = "Layer Name";
                    break;
                case 5:
                    Title.Content = "Create Achievement";
                    NewName.Watermark = "Achievement Name";
                    break;
                default:
                    break;
            }

            
        }

        public void OnCreate(object source, RoutedEventArgs args)
        {
            string dir = "";

            if (ProjectManager.Get().Directory == "")
            {
                dir = Path.Combine(ProjectManager.Get().project.directory, "Assets");
            }
            else
            {
                dir = Path.Combine(Path.Combine(ProjectManager.Get().project.directory, "Assets"), ProjectManager.Get().Directory);
            }

            switch (type)
            {
                case 0:
                    if (NewName.Text != null)
                    {
                        dir = Path.Combine(dir, NewName.Text);
                    }

                    if (NewName.Text == "" || Directory.Exists(dir))
                    {
                        SystemSounds.Beep.Play();
                        return;
                    }

                    Directory.CreateDirectory(dir);

                    ProjectManager.Get().window.RefreshProject();

                    Close();

                    break;

                case 1:
                    string a = dir;
                    if (NewName.Text != null)
                    {
                        dir = Path.Combine(dir, NewName.Text + ".geodeasset");
                    }
                    else
                    {
                        return;
                    }

                    string dir2 = Path.Combine(a, NewName.Text + ".png");

                    bool b = false;

                    if (NewName.Text == "")
                        b = true;

                    if (!b)
                    {
                        b = (File.Exists(dir) || File.Exists(dir2));
                    }

                    if (bmp == null)
                    {
                        b = true;
                    }

                    if (b)
                    {
                        Debug.WriteLine("error");
#pragma warning disable CA1416 // Validate platform compatibility
                        SystemSounds.Beep.Play();
#pragma warning restore CA1416 // Validate platform compatibility
                        return;
                    }

                    string dataT = "{ \"type\": \"image\"}";

                    File.WriteAllText(dir, dataT);
                    File.WriteAllBytes(dir2, File.ReadAllBytes(path));

                    ProjectManager.Get().window.RefreshProject();

                    Close();

                    break;

                case 3:
                    if (NewName.Text != null)
                    {
                        dir = Path.Combine(dir, NewName.Text + ".geodeasset");
                    }

                    if (NewName.Text == "" || File.Exists(dir))
                    {
                        SystemSounds.Beep.Play();
                        return;
                    }

                    string dataL = "{ \"type\": \"cclayer\"}";

                    File.WriteAllText(dir, dataL);

                    ProjectManager.Get().window.RefreshProject();

                    Close();

                    break;

                case 5:
                    if (NewName.Text != null)
                    {
                        dir = Path.Combine(dir, NewName.Text + ".geodeasset");
                    }

                    if (NewName.Text == "" || File.Exists(dir))
                    {
                        SystemSounds.Beep.Play();
                        return;
                    }

                    string dataAch = "{ \"type\": \"achievement\"}";

                    File.WriteAllText(dir, dataAch);

                    ProjectManager.Get().window.RefreshProject();

                    Close();

                    break;

                default:
                    break;
            }
        }

        public async void OnImage(object source, RoutedEventArgs args)
        {
            var topLevel = TopLevel.GetTopLevel(this);

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open PNG File",
                AllowMultiple = false
            });

            if (files.Count >= 1)
            {
                path = files[0].Path.LocalPath;

                Debug.WriteLine(path);
                bmp = new Bitmap(File.OpenRead(path));
                SelectedImage.Source = bmp;
            }
        }

        public void OnCancel(object source, RoutedEventArgs args)
        {
            this.Close();
        }
    }
}