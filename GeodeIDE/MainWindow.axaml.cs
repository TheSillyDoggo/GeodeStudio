using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia;
using System.Reflection.Emit;
using Avalonia.Platform;
using System;
using System.Threading.Tasks;
using Claunia.PropertyList;
using Avalonia.Platform.Storage;
using Avalonia.Media;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;

namespace GeodeIDE
{
    public partial class MainWindow : Window
    {
        [System.Serializable]
        public class ProjectItem
        {
            public string name = "", directory = "";
        }

        public List<ProjectItem> projects, shownProjects;

        public static bool a = false;

        public MainWindow()
        {
            this.InitializeComponent();

            if (a)
            {
                projects = new List<ProjectItem>();

                string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string folderPath = Path.Combine(roamingPath, "GeodeStudio");
                string dataPath = Path.Combine(folderPath, "projects.ini");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                if (!File.Exists(dataPath))
                    File.WriteAllText(dataPath, "");

                Debug.WriteLine(dataPath);

                if (File.ReadAllText(dataPath).Length > 1)
                {
                    foreach (var line in File.ReadAllText(dataPath).Split("\n"))
                    {
                        if (Directory.Exists(line))
                        {
                            if (File.Exists(Path.Combine(line, "mod.json")))
                            {
                                JObject rss = JObject.Parse(File.ReadAllText(Path.Combine(line, "mod.json")));

                                projects.Add(new ProjectItem { name = (string)rss["modName"], directory = line });
                            }
                        }
                        else
                            Debug.WriteLine($"Missing path: {line}");
                    }
                }

                ProjectPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                UpdateList("");
            }

            Prepare();
        }

        public void Prepare()
        {
            if (!a)
            {
                a = true;
                this.ClientSize = new Avalonia.Size(680, 420);

                Action<TimeSpan> testAction = async (a) =>
                {
                    await Task.Delay(10);

                    #region Fonts

                    Debug.WriteLine("Loading Fonts");

                    foreach (var file in Directory.GetFiles(Program.GetGDResourcesPath(), "*.fnt"))
                    {
                        if (!Path.GetFileNameWithoutExtension(file).EndsWith("-uhd"))
                            continue;

                        Stream strm = File.OpenRead(file.Replace(".fnt", ".png"));

                        Debug.WriteLine(Path.GetFileNameWithoutExtension(file.Replace("-uhd", "")));

                        LabelBMFont.InitFont(Path.GetFileNameWithoutExtension(file.Replace("-uhd", "")), File.ReadAllText(file), new Bitmap(strm));

                        strm.Close();
                    }

                    //LabelBMFont.InitFont("bigFont", File.ReadAllText(Program.GetGDResourcesPath() + "bigFont-uhd.fnt"), new Bitmap(File.OpenRead(Program.GetGDResourcesPath() + "bigFont-uhd.png")));
                    //LabelBMFont.InitFont("goldFont", File.ReadAllText(Program.GetGDResourcesPath() + "goldFont-uhd.fnt"), new Bitmap(File.OpenRead(Program.GetGDResourcesPath() + "goldFont-uhd.png")));
                    //LabelBMFont.InitFont("chatFont", File.ReadAllText(Program.GetGDResourcesPath() + "chatFont-uhd.fnt"), new Bitmap(File.OpenRead(Program.GetGDResourcesPath() + "chatFont-uhd.png")));

                    #endregion

                    await Task.Delay(10);

                    #region AlignmentIcons

                    Debug.WriteLine("Loading Alignment");

                    LoadingInfo.Content = "Loading Alignment icons";
                    this.UpdateLayout();

                    var s = AssetLoader.Open(new Uri("avares://Geode Studio/Assets/Anchor.png"));
                    Bitmap bmp = new Bitmap(s);

                    for (int i = 0; i < 10; i++)
                    {
                        var rec = new PixelPoint();
                        switch (i)
                        {
                            case 0:
                                rec = new PixelPoint(256, 256);
                                break;
                            case 1:
                                rec = new PixelPoint(0, 768);
                                break;
                            case 2:
                                rec = new PixelPoint(0, 256);
                                break;
                            case 3:
                                rec = new PixelPoint(512, 256);
                                break;
                            case 4:
                                rec = new PixelPoint(256, 0);
                                break;
                            case 5:
                                rec = new PixelPoint(256, 512);
                                break;
                            case 6:
                                rec = new PixelPoint(0, 0);
                                break;
                            case 7:
                                rec = new PixelPoint(0, 512);
                                break;
                            case 8:
                                rec = new PixelPoint(512, 512);
                                break;
                            case 9:
                                rec = new PixelPoint(512, 0);
                                break;
                            default:
                                break;
                        }

                        Program.alignmentIcons.Add(new CroppedBitmap(bmp, new PixelRect(rec.X, rec.Y, 256, 256)));
                    }

                    s.Close();

                    #endregion

                    LoadingInfo.Content = "Loading Geometry Dash Resources";
                    await Task.Delay(10);

                    #region GDAssets

                    Debug.WriteLine("Loading GDAssets");

                    this.UpdateLayout();

                    this.RequestAnimationFrame(delegate{

                        foreach (var file in Directory.GetFiles(Program.GetGDResourcesPath(), "*.png").OrderBy(f => f))
                        {
                            string type = "";

                            if (Path.GetFileNameWithoutExtension(file).EndsWith("-hd"))
                                type = "-hd";

                            if (Path.GetFileNameWithoutExtension(file).EndsWith("-uhd"))
                                type = "-uhd";

                            if (!(Path.GetFileNameWithoutExtension(file).EndsWith("-hd") || Path.GetFileNameWithoutExtension(file).EndsWith("-uhd")))
                                continue;

                            if (Program.gdAssets.ContainsKey(file.Replace($"{type}.png", "")))
                                continue;

                            if (type != "-uhd")
                            {
                                if (File.Exists(file.Replace($"{type}.png", "-uhd.png")))
                                    continue;
                            }

                            if (File.Exists(file.Replace(".png", ".plist")))
                            {
                                FileInfo fileSA = new FileInfo(file.Replace(".png", ".plist"));
                                NSDictionary rootDict = (NSDictionary)PropertyListParser.Parse(fileSA);

                                Stream strm = File.OpenRead(file);
                                Bitmap bmpSS = new Bitmap(strm);

                                foreach (var param in ((NSDictionary)rootDict.Get("frames")).GetDictionary())
                                {
                                    try
                                    {
                                        string textureRectV = ((NSDictionary)(param.Value)).Get("textureRect").ToString();

                                        string texPos = textureRectV.Split(',')[0] + "," + textureRectV.Split(',')[1];
                                        texPos = texPos.Remove(0, 2);
                                        texPos = texPos.Remove(texPos.Length - 1, 1);
                                        PixelPoint pos = new PixelPoint(int.Parse(texPos.Split(",".ToCharArray())[0]), int.Parse(texPos.Split(",".ToCharArray())[1]));

                                        string texSize = textureRectV.Split(',')[2] + "," + textureRectV.Split(',')[3];
                                        texSize = texSize.Remove(0, 1);
                                        texSize = texSize.Remove(texSize.Length - 2, 2);
                                        PixelSize size = new PixelSize(int.Parse(texSize.Split(",".ToCharArray())[0]), int.Parse(texSize.Split(",".ToCharArray())[1]));

                                        bool rotated = false;

                                        NSNumber num = (NSNumber)(((NSDictionary)(param.Value)).Get("textureRotated"));

                                        if (num.isBoolean())
                                        {
                                            rotated = num.ToBool();
                                        }

                                        if (rotated)
                                        {
                                            size = new PixelSize(size.Height, size.Width);
                                        }


                                        CroppedBitmap cbmp = new CroppedBitmap(bmpSS, new PixelRect(pos, size));

                                        if (!Program.gdSheetAssets.ContainsKey(param.Key.Replace(".png", "")))
                                        {
                                            var b = new Dictionary<CroppedBitmap, bool>(1);
                                            b.Add(cbmp, rotated);
                                            Program.gdSheetAssets.Add(param.Key.Replace(".png", ""), b);
                                        }

                                        Debug.WriteLine($"path: {file}\ntype: {type}\nname: {param.Key.Replace(".png", "")}\npos: {pos}\nsize: {texSize}\nsizeX: {size}\nrotated: {rotated}");
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine($"Error with {param.Key}. {ex.Message} at {ex.StackTrace}");

                                        continue;
                                    }

                                    /*
                                    <key>GJLargeLock_001.png</key>
                                    <dict>
                                        <key>aliases</key>
                                        <array/>
                                        <key>spriteOffset</key>
                                        <string>{0,0}</string>
                                        <key>spriteSize</key>
                                        <string>{60,73}</string>
                                        <key>spriteSourceSize</key>
                                        <string>{60,73}</string>
                                        <key>textureRect</key>
                                        <string>{{298,355},{60,73}}</string>
                                        <key>textureRotated</key>
                                        <true/>
                                    </dict>
                                    */
                                }

                                strm.Close();
                                continue;
                            }


                            if (File.Exists(file.Replace(".png", ".fnt")))
                                continue;

                            Debug.WriteLine($"path: {file}\ntype: {type}\nname: {Path.GetFileNameWithoutExtension(file.Replace($"{type}.png", ""))}");

                            Stream stream = File.OpenRead(file);
                            Program.gdAssets.Add(Path.GetFileNameWithoutExtension(file.Replace($"{type}.png", "")), new Bitmap(stream));
                            stream.Close();
                        }

                    });



                    #endregion

                    await Task.Delay(10);

                    var b = new MainWindow();
                    b.Show();

                    this.Close();
                };

                this.RequestAnimationFrame(testAction);
            }
            else
            {
                this.ClientSize = new Avalonia.Size(940, 740);

                this.IntroPanel.IsVisible = false;
                this.LauncherPanel.IsVisible = true;
                this.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
            }

            //LoadingInfo.Content = "Loading GD Assets";
            //IntroPanel.IsVisible = false;
            //LauncherPanel.IsVisible = true;
            //w.Show(this);
            //Close();
        }

        public void OnResized(object source, SizeChangedEventArgs e)
        {
            SearchProjects.Margin = new Avalonia.Thickness(30, 140, ClientSize.Width - 30 - 300, 700);
            //Buttons.Margin = new Avalonia.Thickness(ClientSize.Width - 382, 170, 20, 30);
            //Create.Margin = new Avalonia.Thickness(0, 0, 0, (ClientSize.Height - 170 - 30) - 100);
            //Import.Margin = new Avalonia.Thickness(0, 75, 0, (ClientSize.Height - 170 - 30) - 175);
            //Samples.Margin = new Avalonia.Thickness(0, 150, 0, (ClientSize.Height - 170 - 30) - 250);
        }

        public void CreateClicked(object source, RoutedEventArgs args)
        {
            LauncherPanel.IsVisible = false;
            CreatePanel.IsVisible = true;
        }

        public void CreateBottomClicked(object source, RoutedEventArgs args)
        {
            if (((Button)source).Content == "Create")
            {
                string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string folderPath = Path.Combine(roamingPath, "GeodeStudio");
                string dataPath = Path.Combine(folderPath, "projects.ini");

                File.WriteAllText(dataPath, File.ReadAllText(dataPath) + $"\n{Path.Combine(ProjectPath.Text, ProjectName.Text).Replace("/", "\\")}");

                ProjectManager.Get().project = new ProjectItem() { name = ProjectName.Text, directory = Path.Combine(ProjectPath.Text, ProjectName.Text)};

                Directory.CreateDirectory(Path.Combine(ProjectPath.Text, ProjectName.Text, "Assets"));

                JObject s = new JObject();

                s.Add("modName", ProjectName.Text);
                s.Add("modAuthor", ProjectAuth.Text);

                File.WriteAllText(Path.Combine(ProjectPath.Text, ProjectName.Text, "mod.json"), s.ToString());

                LoadingPanel.IsVisible = true;
                LauncherPanel.IsVisible = false;
                ProjectManager.Get().LoadWindow(this);
            }   
            else
            {
                LauncherPanel.IsVisible = true;
                CreatePanel.IsVisible = false;
            }
        }

        public async void ChoosePath(object source, RoutedEventArgs args)
        {
            var topLevel = TopLevel.GetTopLevel(this);

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Choose Project Root Path",
                AllowMultiple = false
            });

            if (files.Count >= 1)
            {
                ProjectPath.Text = files[0].Path.AbsolutePath;
            }
        }

        public void ListClicked(object source, SelectionChangedEventArgs args)
        {
            ProjectManager.Get().project = shownProjects[ProjectList.SelectedIndex];
            LoadingPanel.IsVisible = true;
            LauncherPanel.IsVisible = false;
            ProjectManager.Get().LoadWindow(this);
        }

        public void TextUpdated(object source, TextChangedEventArgs args)
        {
            string errorTxt = "";
            bool red = false;

            if (ProjectName.Text == null)
            {
                errorTxt = "Project Name cannot be empty";
                red = true;
            }
            else
            {
                if (ProjectAuth.Text == null)
                {
                    errorTxt = "Project Author cannot be empty";
                    red = true;
                }
                else
                {
                    if (ProjectName.Text != null)
                    {
                        if (Directory.Exists(Path.Combine(ProjectPath.Text, ProjectName.Text)))
                        {
                            errorTxt = $"Project Directory already exists.\nPath: {Path.Combine(ProjectPath.Text, ProjectName.Text).Replace("/", "\\")}";
                            red = true;
                        }
                        else
                        {
                            errorTxt = $"Project will be saved to {Path.Combine(ProjectPath.Text, ProjectName.Text).Replace("/", "\\")}";
                            red = false;
                        }
                    }
                }
            }

            SavePath.Foreground = new SolidColorBrush(new Color(255, 255, (byte)(red ? 0 : 255), (byte)(red ? 0 : 255)));
            SavePath.Content = errorTxt;
        }

        public void SearchUpdated(object source, TextChangedEventArgs args)
        {
            #pragma warning disable 
            UpdateList(SearchProjects.Text);
            
            #pragma warning enable
        }

        public void UpdateList(string search)
        {
            shownProjects = new List<ProjectItem>();
            ProjectList.Items.Clear();

            foreach (var item in projects)
            {
                if (search != "")
                {
                    if (item.name.Contains(search))
                    {
                        ProjectList.Items.Add(item.name);
                        shownProjects.Add(item);
                    }
                }
                else
                {
                    ProjectList.Items.Add(item.name);
                    shownProjects.Add(item);
                }
            }

            Nothing.IsVisible = (ProjectList.ItemCount == 0);
        }
    }
}