using Avalonia;
using Avalonia.Layout;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Avalonia.Media.Imaging;
using Newtonsoft.Json;
using Avalonia.Platform;
using CustomControls;

namespace GeodeIDE
{
    public class EditLayer
    {
        public static Hierarchy hierarchy;

        public static string path;

        public static void Create()
        {
            var wnd = ProjectManager.Get().window;

            try
            {
                JObject rss = JObject.Parse(File.ReadAllText(path));
                string str = (string)rss["layerData"];

                ProjectManager.Get().layer = JsonConvert.DeserializeObject<CCLayer>(str);
            }
            catch (Exception ex)
            {
                //probably an empty project so time to create one

                ProjectManager.Get().layer = new CCLayer();
            }

            ProjectManager.Get().layer.name = Path.GetFileNameWithoutExtension(path);

            hierarchy = wnd.Hierarchy;

            hierarchy.Selected = 0;

            UpdateProperties();
        }

        public static void SelectNode(int nodeID)
        {
            UpdateProperties();
        }

        public static void UpdateProperties()
        {
            var wnd = ProjectManager.Get().window;

            if (hierarchy.Selected == 0)
            {
                wnd.PropertiesPanel.IsVisible = false;
                wnd.PropertiesHelp.IsVisible = true;
            }
            else
            {
                wnd.PropertiesPanel.IsVisible = !false;
                wnd.PropertiesHelp.IsVisible = !true;

                int i = 0;

                switch (ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].alignment)
                {
                    case CCLayer.CCNode.Alignment.CENTER:
                        i = 0;
                        break;
                    case CCLayer.CCNode.Alignment.STRETCH:
                        i = 1;
                        break;
                    case CCLayer.CCNode.Alignment.LEFT:
                        i = 2;
                        break;
                    case CCLayer.CCNode.Alignment.RIGHT:
                        i = 3;
                        break;
                    case CCLayer.CCNode.Alignment.TOP:
                        i = 4;
                        break;
                    case CCLayer.CCNode.Alignment.BOTTOM:
                        i = 5;
                        break;
                    case CCLayer.CCNode.Alignment.TOPLEFT:
                        i = 6;
                        break;
                    case CCLayer.CCNode.Alignment.BOTTOMLEFT:
                        i = 7;
                        break;
                    case CCLayer.CCNode.Alignment.BOTTOMRIGHT:
                        i = 8;
                        break;
                    case CCLayer.CCNode.Alignment.TOPRIGHT:
                        i = 9;
                        break;
                    default:
                        break;
                }

                wnd.AnchorIcon.Source = Program.alignmentIcons[i];

                if (ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].alignment == CCLayer.CCNode.Alignment.STRETCH)
                {
                    wnd.L.Content = "Left";
                    wnd.T.Content = "Top";
                    wnd.R.Content = "Down";
                    wnd.B.Content = "Right";
                    wnd.XIn.Watermark = "Left";
                    wnd.YIn.Watermark = "Top";
                    wnd.WIn.Watermark = "Down";
                    wnd.HIn.Watermark = "Right";
                }
                else
                {
                    wnd.L.Content = "X Pos";
                    wnd.T.Content = "Y Pos";
                    wnd.R.Content = "Width";
                    wnd.B.Content = "Height";
                    wnd.XIn.Watermark = "X Pos";
                    wnd.YIn.Watermark = "Y Pos";
                    wnd.WIn.Watermark = "Width";
                    wnd.HIn.Watermark = "Height";
                }

                wnd.XIn.Text = ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].position.X.ToString();
                wnd.YIn.Text = ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].position.Y.ToString();
                wnd.WIn.Text = ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].size.X.ToString();
                wnd.HIn.Text = ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].size.Y.ToString();
                wnd.AlphaInput.Text = (ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].alpha * 255).ToString();
                wnd.LabelText.Text = ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].labelText;
                wnd.NodeName.Text = ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].name;
                wnd.NodeEnabled.IsChecked = ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].enabled;

                if (ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].spriteName == "")
                {
                    wnd.LayerImagePreview.Source = null;
                }
                else
                {
                    wnd.LayerImagePreview.Source = ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].GetImage().Item1;

                    wnd.LayerImagePreview.RenderTransformOrigin = RelativePoint.Center;
                    RotateTransform rT = new RotateTransform();


                    rT.Angle = ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].GetImage().Item2;
                    wnd.LayerImagePreview.RenderTransform = rT;
                    wnd.SpriteName.Content = ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].spriteName;
                }

                wnd.NodePanel.IsVisible = (ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].type == CCLayer.CCNode.nType.Node) || (ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].type == CCLayer.CCNode.nType.Button);
                wnd.LabelPanel.IsVisible = (ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].type == CCLayer.CCNode.nType.Label);
                wnd.SpriteType.SelectedIndex = ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].cclayer9sprite ? 1 : 0;
            }
        }

        public static void Save()
        {
            var wnd = ProjectManager.Get().window;

            JObject jo = new JObject();
            jo.Add("type", "cclayer");

            for (int i = 0; i < ProjectManager.Get().layer.nodes.Count; i++)
            {
                ProjectManager.Get().layer.nodes[i].img = null;
                ProjectManager.Get().layer.nodes[i].c = null;
            }

            jo.Add("layerData", JsonConvert.SerializeObject(ProjectManager.Get().layer));

            File.WriteAllText(path, jo.ToString());

            foreach (var item in wnd.MainPanel.Children)
            {
                item.IsVisible = false;
            }

            wnd.TitleBar.IsVisible = true;
            wnd.HomePanel.IsVisible = true;
        }

        public static void Edit(string path)
        {
            EditLayer.path = path;

            var wnd = ProjectManager.Get().window;

            foreach (var item in wnd.MainPanel.Children)
            {
                item.IsVisible = false;
            }

            wnd.TitleBar.IsVisible = true;
            wnd.LayerPanel.IsVisible = true;

            Create();
        }

        public static void UpdateText()
        {
            var wnd = ProjectManager.Get().window;

            float x, y, width, height;

            float.TryParse(wnd.XIn.Text, out x);
            float.TryParse(wnd.YIn.Text, out y);

            float.TryParse(wnd.WIn.Text, out width);
            float.TryParse(wnd.HIn.Text, out height);

            try
            {
                ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].position = new Point(x, y);
                ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].size = new Point(width, height);

                wnd.XIn.Text = x.ToString();
                wnd.YIn.Text = y.ToString();

                wnd.WIn.Text = width.ToString();
                wnd.HIn.Text = height.ToString();

                ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].name = wnd.NodeName.Text;
                ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].enabled = (bool)wnd.NodeEnabled.IsChecked;
                ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].alpha = float.Parse(wnd.AlphaInput.Text) / 255;
                ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].labelText = wnd.LabelText.Text;
                ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].cclayer9sprite = wnd.SpriteType.SelectedIndex == 1;
                if (ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].img != null)
                    ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].img.Opacity = ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].alpha;
            }
            catch (Exception)
            {

            }
        }

        public static void Resize()
        {
            //Margin="278,0,278,0" 

            ProjectManager.Get().window.BG1.Margin = new Thickness(278, 0, ProjectManager.Get().window.ClientSize.Width / 2 + (854 / 2), 0);
            ProjectManager.Get().window.BG2.Margin = new Thickness(ProjectManager.Get().window.ClientSize.Width / 2 + (854 / 2), 0, 278, 0);
        }

        public static void UpdateSpriteSize()
        {
            if (Program.gdSheetAssets.ContainsKey(ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].spriteName))
            {
                Dictionary<CroppedBitmap, bool> v;
                Program.gdSheetAssets.TryGetValue(ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].spriteName, out v);

                if (v.ElementAt(0).Value)
                {
                    ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].size = new Point(ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].GetImage().Item1.Size.Height, ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].GetImage().Item1.Size.Width);
                }
                else
                {
                    ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].size = new Point(ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].GetImage().Item1.Size.Width, ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].GetImage().Item1.Size.Height);
                }
            }
            else
            {
                ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].size = new Point(ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].GetImage().Item1.Size.Width, ProjectManager.Get().layer.nodes[hierarchy.Selected - 1].GetImage().Item1.Size.Height);
            }

            UpdateProperties();
        }

        public static void CreateNode(string type)
        {
            var wnd = ProjectManager.Get().window;

            ProjectManager.Get().layer.nodes.Add(new CCLayer.CCNode() { position = new Point(0, 0), size = new Point(128, 128), anchor = new Point(0.5f, 0.5f), alignment = CCLayer.CCNode.Alignment.CENTER, name = "New Node", enabled=false });
        }
    }
}
