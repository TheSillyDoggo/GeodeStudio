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
    public partial class ChooseAssetDialogue : Window
    {
        public enum AssetType
        {
            Sprite,
            Font
        }

        public AssetType type;

        public ChooseAssetDialogue(AssetType type)
        {
            this.type = type;

            this.InitializeComponent();

            CanResize = false;
            Topmost = true;
            ClientSize = new Avalonia.Size(400 * 1.5f, 270 * 1.5f);

            Buttons.Children.Clear();
            Fonts.Children.Clear();

            if (type == AssetType.Sprite)
            {
                PopulateImages();
            }
            else if (type == AssetType.Font)
            {
                PopulateFonts();
            }
        }

        public void PopulateImages()
        {
            int count = 0;

            if (Buttons.Children.Count > 0)
                Buttons.Children.Clear();

            foreach (var item in Program.gdAssets)
            {

                count++;

                Panel panel = new Panel();

                panel.Width = 80;
                panel.Height = 95;

                var btn = new Button();
                btn.HorizontalAlignment = HorizontalAlignment.Stretch;
                btn.VerticalAlignment = VerticalAlignment.Stretch;
                btn.Name = item.Key;
                btn.Opacity = 0;
                btn.Click += delegate
                {
                    SelectSprite(btn.Name);

                    Close();
                };

                var img = new Image();
                img.Source = item.Value;
                img.Width = panel.Width;
                img.Width = panel.Width;
                img.VerticalAlignment = VerticalAlignment.Top;
                img.Stretch = Stretch.Uniform;

                var lbl = new Label();
                lbl.HorizontalContentAlignment = HorizontalAlignment.Center;
                lbl.VerticalContentAlignment = VerticalAlignment.Bottom;
                lbl.FontSize = 10;
                lbl.Content = item.Key;

                panel.Children.Add(img);
                panel.Children.Add(lbl);
                panel.Children.Add(btn);
                Buttons.Children.Add(panel);
            }

            
            foreach (var item in Program.gdSheetAssets)
            {

                count++;

                Panel panel = new Panel();

                panel.Width = 80;
                panel.Height = 95;

                var btn = new Button();
                btn.HorizontalAlignment = HorizontalAlignment.Stretch;
                btn.VerticalAlignment = VerticalAlignment.Stretch;
                btn.Name = item.Key;
                btn.Opacity = 0;
                btn.Click += delegate
                {
                    SelectSprite(btn.Name);

                    Close();
                };

                var img = new Image();
                img.Source = item.Value.ElementAt(0).Key;
                img.Width = panel.Width;
                img.Height = panel.Width;
                img.VerticalAlignment = VerticalAlignment.Top;
                img.Stretch = Stretch.Uniform;

                if (item.Value.ElementAt(0).Value)
                {
                    img.RenderTransformOrigin = RelativePoint.Center;
                    RotateTransform rT = new RotateTransform();
                    rT.Angle = -90;
                    img.RenderTransform = rT;
                }

                var lbl = new Label();
                lbl.HorizontalContentAlignment = HorizontalAlignment.Center;
                lbl.VerticalContentAlignment = VerticalAlignment.Bottom;
                lbl.FontSize = 10;
                lbl.Content = item.Key;

                panel.Children.Add(img);
                panel.Children.Add(lbl);
                panel.Children.Add(btn);
                Buttons.Children.Add(panel);
            }

            LoadedInfo.Content = $"Loaded {count}/{Program.gdAssets.Count + Program.gdSheetAssets.Count}";
        }

        public void PopulateFonts()
        {
            int count = 0;

            foreach (var font in LabelBMFont.fonts)
            {
                count++;

                Panel panel = new Panel();

                panel.Width = ClientSize.Width - 20;
                panel.Height = 40;

                panel.Name = font.Key;

                var label = LabelBMFont.CreateAsChildren(font.Key, font.Key, 0.3f, LabelBMFont.Alignment.Left);
                label.Margin = new Thickness(5, 0,0, 5);

                panel.Margin = new Thickness(0, 40 * (count - 1), 0, 0);
                panel.VerticalAlignment = VerticalAlignment.Top;

                var btn = new Button();
                btn.HorizontalAlignment = HorizontalAlignment.Stretch;
                btn.VerticalAlignment = VerticalAlignment.Stretch;
                btn.Name = font.Key;
                btn.Opacity = 0;
                btn.Click += delegate
                {
                    SelectFont(btn.Name);

                    Close();
                };

                panel.Children.Add(label);
                panel.Children.Add(btn);
                Fonts.Children.Add(panel);
            }

            Fonts.Height = 40 * count;
            LoadedInfo.Content = $"Loaded {count}/{LabelBMFont.fonts.Count}";
        }

        public void OnCreate(object source, RoutedEventArgs args)
        {
            ProjectManager.Get().window.RefreshProject();

            Close();
        }

        public void SelectSprite(string asset)
        {
            ProjectManager.Get().layer.nodes[EditLayer.hierarchy.Selected - 1].spriteName = asset;
            ProjectManager.Get().layer.nodes[EditLayer.hierarchy.Selected - 1].RefreshSprite();
            EditLayer.UpdateProperties();
        }

        public void SelectFont(string asset)
        {
            ProjectManager.Get().layer.nodes[EditLayer.hierarchy.Selected - 1].fontName = asset;
            ProjectManager.Get().layer.nodes[EditLayer.hierarchy.Selected - 1].RefreshSprite();
            EditLayer.UpdateProperties();
        }

        public void OnCancel(object source, RoutedEventArgs args)
        {
            this.Close();
        }

        public void UpdateSearch(object source, TextChangedEventArgs args)
        {
            foreach (var child in Buttons.Children)
            {
                child.IsVisible = (((Button)(((Panel)(child)).Children[2])).Name.ToLower().Contains(SearchBox.Text.ToLower()));
            }

            int c = 0;

            foreach (var child in Fonts.Children)
            {
                child.IsVisible = ((Panel)(child)).Name.ToLower().Contains(SearchBox.Text.ToLower());

                if (child.IsVisible)
                    c++;

                child.Margin = new Thickness(0, 40 * (c - 1), 0, 0);

                Fonts.Height = 40 * c;
                LoadedInfo.Content = $"Loaded {c}/{LabelBMFont.fonts.Count}";
            }
        }
    }
}