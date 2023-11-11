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
    public partial class RenameDialogue : Window
    {
        public static string oldName, type;
        public static bool isDirectory;

        public RenameDialogue()
        {
            this.InitializeComponent();

            CanResize = false;
            Topmost = true;
            ClientSize = new Avalonia.Size(400, 270);

            NewName.Text = oldName;

            if (!isDirectory)
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

                //Path.Combine(dir, oldName + ".geodeasset")
                string data = File.ReadAllText(Path.Combine(dir, oldName + ".geodeasset"));
                JObject rss = JObject.Parse(data);
                type = (string)rss["type"];

                Title.Content = $"Rename {type.FirstCharToUpper()}";
            }
        }

        public void OnRename(object source, RoutedEventArgs args)
        {
            if (NewName.Text == "" || NewName.Text == oldName)
            {
                SystemSounds.Beep.Play();
            }
            else
            {
                try
                {
                    if (isDirectory)
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

                        Debug.WriteLine(Path.Combine(dir, oldName));
                        Debug.WriteLine(Path.Combine(dir, NewName.Text));

                        if (Directory.Exists(Path.Combine(dir, NewName.Text)))
                        {
                            SystemSounds.Beep.Play();
                        }
                        else
                        {
                            Directory.Move(Path.Combine(dir, oldName), Path.Combine(dir, NewName.Text));

                            ProjectManager.Get().window.RefreshProject();

                            Close();
                        }
                    }
                    else
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

                        Debug.WriteLine(Path.Combine(dir, oldName + ".geodeasset"));
                        Debug.WriteLine(Path.Combine(dir, NewName.Text + ".geodeasset"));

                        File.WriteAllText(Path.Combine(dir, NewName.Text + ".geodeasset"), File.ReadAllText(Path.Combine(dir, oldName + ".geodeasset")));
                        File.Delete(Path.Combine(dir, oldName + ".geodeasset"));

                        switch (type)
                        {
                            case "image":

                                ProjectManager.Get().window.Buttons.Children.Clear();
                                File.WriteAllBytes(Path.Combine(dir, NewName.Text + ".png"), File.ReadAllBytes(Path.Combine(dir, oldName + ".png")));
                                File.Delete(Path.Combine(dir, oldName + ".png"));

                                break;
                            default: break;
                        }

                        ProjectManager.Get().window.RefreshProject();

                        Close();
                    }
                }
                catch (Exception e)
                {
                    SystemSounds.Beep.Play();

                    Title.Content = e.Message;

                    throw;
                }
            }
        }

        public void OnCancel(object source, RoutedEventArgs args)
        {
            this.Close();
        }
    }
}