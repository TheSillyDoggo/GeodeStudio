using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Interactivity;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Drawing.Printing;
using Avalonia;
using System;
using System.IO;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Media.Imaging;
using Avalonia.Media.TextFormatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AsyncImageLoader;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using VisualGeode.Exporter;
using VisualGeode;

namespace GeodeIDE
{
    public partial class ProjectWindow : Window
    {
        public ProjectWindow()
        {
            this.InitializeComponent();

            DirectoryName.Content = ProjectManager.Get().GetDirectoryString();
            RefreshProject();
        }

        public void OnResize(object obj, SizeChangedEventArgs args)
        {
            LeftBar.Margin = new Thickness(0, 0, ClientSize.Width - 40, 0);
            TopBar.Margin = new Thickness(0, 0, 0, ClientSize.Height - 60);

            Buttons.Columns = (int)(MathF.Floor((float)(ClientSize.Width) / 90));
            Buttons.Rows = (int)(MathF.Floor((float)(ClientSize.Height) / 90));

            //MenuBar.Margin = new Thickness(35, -32, ClientSize.Width - MenuBar.Bounds.Width - 35, 0);

            ButtonsStack.Height = MathF.Max((float)ClientSize.Height - (float)Buttons.Margin.Top - (float)Buttons.Margin.Bottom, ((MathF.Floor((Buttons.Children.Count / Buttons.Columns) + 1) - 1) * 90) + (float)Buttons.Margin.Bottom);

            float achScale = 0.75f;
            //1080, 362
            AchievementPreview.Width = 1080 * achScale;
            AchievementPreview.Height = 362 * achScale;
            AchievementPreview.MaxWidth = 1080;
            AchievementPreview.MaxHeight = 362;

            //EditAchievement.Create();

            EditLayer.Resize();

            //AchievementPreview.Margin = new Thickness((ClientSize.Width / 2 - ((508 / 2) + 1) * achScale) - 30, (ClientSize.Height / 2 - (83) * achScale) - 30, (ClientSize.Width / 2 - ((508 / 2) + 1) * achScale) - 30, (ClientSize.Height / 2 - (83) * achScale) - 30);
        }

        public void RefreshProject()
        {
            DirectoryName.Content = ProjectManager.Get().GetDirectoryString();

            if (Buttons.Children.Count > 0)
                Buttons.Children.Clear();

            string dir = "";

            if (ProjectManager.Get().Directory == "")
            {
                dir = Path.Combine(ProjectManager.Get().project.directory, "Assets");
            }
            else
            {
                dir = Path.Combine(Path.Combine(ProjectManager.Get().project.directory, "Assets"), ProjectManager.Get().Directory);
            }

            var dirs = Directory.GetDirectories(dir);

            for (int i = 0; i < dirs.Length; i++)
            {
                AddFolder(dirs[i]);
            }

            foreach (var file in Directory.GetFiles(dir, "*.geodeasset"))
            {
                AddFile(file);
            }

            NoElements.IsVisible = (Buttons.Children.Count == 0);
        }

        public void GoRoot(object sender, RoutedEventArgs e)
        {
            ProjectManager.Get().Directory = "";

            RefreshProject();
        }

        public void genTest(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(ExportLayer.CreateLayerH(ProjectManager.Get().layer));
        }

        public void GoBack(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProjectManager.Get().Directory != "")
                {
                    ProjectManager.Get().Directory = ProjectManager.Get().Directory.Remove(ProjectManager.Get().Directory.LastIndexOf('\\'));
                }
            }
            catch
            {
                ProjectManager.Get().Directory = "";
            }

            RefreshProject();
        }

        public void AddFolder(string dirLabel)
        {
            var a = new Panel();
            a.Opacity = 1;
            a.Name = Path.GetFileName(dirLabel);
            a.Width = 80;
            a.Height = 80;

            var label = new TextBlock();
            label.HorizontalAlignment = HorizontalAlignment.Stretch;
            label.VerticalAlignment = VerticalAlignment.Bottom;
            label.TextAlignment = TextAlignment.Center;
            label.TextWrapping = TextWrapping.WrapWithOverflow;
            label.Text = Path.GetFileName(dirLabel);
            label.FontSize = 10;

            var image = new Image();

            image.Source = FolderIcon.Source;
            image.Margin = new Thickness(10, 0, 10, 10);

            var button = new Button();
            button.HorizontalAlignment = HorizontalAlignment.Stretch;
            button.VerticalAlignment = VerticalAlignment.Stretch;
            button.Margin = new Thickness(0, 0, 0, 0);
            button.Opacity = 0;
            button.Click += delegate
            {
                ProjectManager.Get().OpenDirectory(a);
            };

            var ctx = new ContextMenu();
            button.ContextMenu = ctx;

            MenuItem oDir = new MenuItem() { Header = "Open Directory" };
            oDir.Click += delegate
            {
                ProjectManager.Get().OpenDirectory(a);
            };

            MenuItem rDir = new MenuItem() { Header = "Rename" };
            rDir.Click += delegate
            {
                RenameDialogue.oldName = Path.GetFileName(dirLabel);
                RenameDialogue.isDirectory = true;

                var wnd = new RenameDialogue();

                wnd.ShowDialog(this);
            };

            MenuItem dDir = new MenuItem() { Header = "Delete" };
            dDir.Click += delegate
            {
                DeleteDialogue.isDirectory = true;
                DeleteDialogue.dir = dirLabel;

                var wnd = new DeleteDialogue();

                wnd.ShowDialog(this);
            };

            ctx.Items.Add(oDir);
            ctx.Items.Add(rDir);
            ctx.Items.Add(dDir);

            a.Children.Add(image);
            a.Children.Add(label);
            a.Children.Add(button);
            Buttons.Children.Add(a);
        }

        public void AddFile(string dirLabel)
        {
            string data = File.ReadAllText(dirLabel);
            JObject rss = JObject.Parse(data);
            string type = (string)rss["type"];
            string fName = Path.GetFileNameWithoutExtension(dirLabel);

            var a = new Panel();
            a.Opacity = 1;
            a.Name = dirLabel;
            a.Width = 80;
            a.Height = 80;

            var label = new TextBlock();
            label.HorizontalAlignment = HorizontalAlignment.Stretch;
            label.VerticalAlignment = VerticalAlignment.Bottom;
            label.TextAlignment = TextAlignment.Center;
            label.TextWrapping = TextWrapping.WrapWithOverflow;
            label.Text = fName;
            label.FontSize = 10;

            var image = new Image();

            switch (type)
            {
                case "image":
                    try
                    {
                        var b = new Bitmap(File.OpenRead(dirLabel.Replace(".geodeasset", ".png")));
                        image.Source = b;
                    }
                    catch
                    {
                        image.Source = MissingIcon.Source;
                    }
                    break;
                case "achievement":
                    image.Source = AchievementIcon.Source;
                    break;
                case "cclayer":
                    image.Source = LayerIcon.Source;
                    break;
                default:
                    break;
            }

            image.Margin = new Thickness(10, 0, 10, 10);

            var button = new Button();
            button.HorizontalAlignment = HorizontalAlignment.Stretch;
            button.VerticalAlignment = VerticalAlignment.Stretch;
            button.Margin = new Thickness(0, 0, 0, 0);
            button.Opacity = 0;
            button.Click += delegate
            {
                Debug.WriteLine("clicked");
            };

            var ctx = new ContextMenu();
            button.ContextMenu = ctx;

            MenuItem oDir = new MenuItem() { Header = "Edit" };
            oDir.Click += delegate
            {
                switch (type)
                {
                    case "achievement":
                        EditAchievement.Edit(dirLabel);
                        break;
                    case "cclayer":
                        EditLayer.Edit(dirLabel);
                        break;
                    default:
                        break;
                }
            };

            MenuItem rDir = new MenuItem() { Header = "Rename" };
            rDir.Click += delegate
            {
                RenameDialogue.oldName = Path.GetFileNameWithoutExtension(dirLabel);
                RenameDialogue.isDirectory = false;


                var wnd = new RenameDialogue();

                wnd.ShowDialog(this);
            };

            MenuItem dDir = new MenuItem() { Header = "Delete" };
            dDir.Click += delegate
            {
                DeleteDialogue.dir = dirLabel;
                DeleteDialogue.isDirectory = false;

                var wnd = new DeleteDialogue();

                wnd.ShowDialog(this);
            };

            ctx.Items.Add(oDir);
            ctx.Items.Add(rDir);
            ctx.Items.Add(dDir);

            a.Children.Add(image);
            a.Children.Add(label);
            a.Children.Add(button);
            Buttons.Children.Add(a);
        }

        public void CreateFolder(object? sender, RoutedEventArgs e)
        {
            CreateDialogue.type = 0;
            var cr = new CreateDialogue();

            cr.ShowDialog(this);
        }

        public void CreateTexture(object? sender, RoutedEventArgs e)
        {
            CreateDialogue.type = 1;
            var cr = new CreateDialogue();

            cr.ShowDialog(this);
        }

        public void CreateAchievement(object? sender, RoutedEventArgs e)
        {
            CreateDialogue.type = 5;
            var cr = new CreateDialogue();

            cr.ShowDialog(this);
        }

        public void CreateCCLayer(object? sender, RoutedEventArgs e)
        {
            CreateDialogue.type = 3;
            var cr = new CreateDialogue();

            cr.ShowDialog(this);
        }

        public void OnSaveAch(object? sender, RoutedEventArgs e)
        {
            if (AchievementPanel.IsVisible)
                EditAchievement.Save();

            if (LayerPanel.IsVisible)
                EditLayer.Save();
        }

        public void OnCancelAch(object? sender, RoutedEventArgs e)
        {
            var wnd = ProjectManager.Get().window;

            foreach (var item in wnd.MainPanel.Children)
            {
                item.IsVisible = false;
            }

            wnd.TitleBar.IsVisible = true;
            wnd.HomePanel.IsVisible = true;
        }

        public void OnBlueAch(object? sender, RoutedEventArgs e)
        {
            EditAchievement.OnBlueToggle();
        }

        public void AchievementText(object source, TextChangedEventArgs args)
        {
            EditAchievement.OnText();
        }

        public void RefreshLayerInput(object source, TextChangedEventArgs args)
        {
            if (((Control)source).Name == "LabelText")
                ProjectManager.Get().layer.nodes[EditLayer.hierarchy.Selected - 1].RefreshSprite();

            EditLayer.UpdateText();
        }

        public void RefreshNodeButton(object source, RoutedEventArgs args)
        {
            EditLayer.UpdateText();
        }

        public void ShowAnchorButton(object source, RoutedEventArgs args)
        {
            AnchorPanel.IsVisible = true;
        }

        public void OnAnchorClick(object source, RoutedEventArgs args)
        {
            Button btn = ((Button)(source));

            Debug.WriteLine($"pressed anchor: {btn.Name}");

            int type = int.Parse(btn.Name.Replace("ANC", ""));

            switch (type)
            {
                case 1:
                    ProjectManager.Get().layer.nodes[EditLayer.hierarchy.Selected - 1].alignment = CCLayer.CCNode.Alignment.TOPLEFT;
                    break;
                case 2:
                    ProjectManager.Get().layer.nodes[EditLayer.hierarchy.Selected - 1].alignment = CCLayer.CCNode.Alignment.TOP;
                    break;
                case 3:
                    ProjectManager.Get().layer.nodes[EditLayer.hierarchy.Selected - 1].alignment = CCLayer.CCNode.Alignment.TOPRIGHT;
                    break;
                case 4:
                    ProjectManager.Get().layer.nodes[EditLayer.hierarchy.Selected - 1].alignment = CCLayer.CCNode.Alignment.LEFT;
                    break;
                case 5:
                    ProjectManager.Get().layer.nodes[EditLayer.hierarchy.Selected - 1].alignment = CCLayer.CCNode.Alignment.CENTER;
                    break;
                case 6:
                    ProjectManager.Get().layer.nodes[EditLayer.hierarchy.Selected - 1].alignment = CCLayer.CCNode.Alignment.RIGHT;
                    break;
                case 7:
                    ProjectManager.Get().layer.nodes[EditLayer.hierarchy.Selected - 1].alignment = CCLayer.CCNode.Alignment.BOTTOMLEFT;
                    break;
                case 8:
                    ProjectManager.Get().layer.nodes[EditLayer.hierarchy.Selected - 1].alignment = CCLayer.CCNode.Alignment.BOTTOM;
                    break;
                case 9:
                    ProjectManager.Get().layer.nodes[EditLayer.hierarchy.Selected - 1].alignment = CCLayer.CCNode.Alignment.BOTTOMRIGHT;
                    break;
                case 10:
                    ProjectManager.Get().layer.nodes[EditLayer.hierarchy.Selected - 1].alignment = CCLayer.CCNode.Alignment.STRETCH;
                    break;
                default:
                    break;
            }

            EditLayer.UpdateProperties();
            AnchorPanel.IsVisible = false;
        }

        public void OnPointerReleased(object source, PointerReleasedEventArgs args)
        {
            bool a = false;

            if (ANC1.IsPointerOver)
                a = true;
            if (ANC2.IsPointerOver)
                a = true;
            if (ANC3.IsPointerOver)
                a = true;
            if (ANC4.IsPointerOver)
                a = true;
            if (ANC5.IsPointerOver)
                a = true;
            if (ANC6.IsPointerOver)
                a = true;
            if (ANC7.IsPointerOver)
                a = true;
            if (ANC8.IsPointerOver)
                a = true;
            if (ANC9.IsPointerOver)
                a = true;
            if (ANC10.IsPointerOver)
                a = true;

            AnchorPanel.IsVisible = a;
        }

        public void HierarchySelectionChanged(object source, RoutedEventArgs args)
        {
            EditLayer.SelectNode(Hierarchy.Selected);
        }

        public void PropertiesValueChanged(object source, SelectionChangedEventArgs args)
        {
            if (ProjectManager.Get().layer != null)
                EditLayer.UpdateText();
        }


        public void ChooseResourceButton(object source, RoutedEventArgs args)
        {
            var icn = new ChooseAssetDialogue(ChooseAssetDialogue.AssetType.Sprite);

            icn.ShowDialog(this);
        }

        public void ChooseFontButton(object source, RoutedEventArgs args)
        {
            var icn = new ChooseAssetDialogue(ChooseAssetDialogue.AssetType.Font);

            icn.ShowDialog(this);
        }

        public void SpriteSizeButton(object source, RoutedEventArgs args)
        {
            EditLayer.UpdateSpriteSize();
        }

        public void CreateNode(object source, RoutedEventArgs args)
        {
            string type = ((MenuItem)(source)).Header.ToString();
            Debug.WriteLine($"Creating Node of type {type}");
            EditLayer.CreateNode(type);
        }

        public void OnEditNode(object source, RoutedEventArgs args)
        {
            EditNodePanel.IsVisible = true;
        }

        public void TestButton(object source, RoutedEventArgs args)
        {
            //ProjectManager.Get().layer.graph.nodes.Add(new VisualGeode.NodeGraph.Node() { name = "aesa" });
            ProjectManager.Get().layer.graph.nodes.Add(new GeneralNodes.setAnimationInterval());
        }
    }
}