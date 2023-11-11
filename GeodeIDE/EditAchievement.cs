using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GeodeIDE
{
    public class EditAchievement
    {
        public static string title, description, path;
        public static bool blue;

        public static void Create()
        {
            var wnd = ProjectManager.Get().window;

            wnd.TitleInput.Text = title;
            wnd.DescriptionInput.Text = description;
            wnd.BlueBtn.IsChecked = blue;

            if (wnd.ImageAE.Children.Count > 0)
                wnd.ImageAE.Children.Clear();

            var a = LabelBMFont.CreateAsChildren(title, blue ? "bigFont" : "goldFont", blue ? 0.3f : 0.42f, LabelBMFont.Alignment.Center);
            if (a != null)
            {
                a.Margin = new Thickness(0, 75, 0, 0);

                wnd.ImageAE.Children.Add(a);
            }

            var b = LabelBMFont.CreateAsChildren(description, "chatFont", 0.45f, LabelBMFont.Alignment.Left);
            if (b != null)
            {
                b.Margin = new Thickness(-0.5f * (a.Width), 125, 0, 0);
                wnd.ImageAE.Children.Add(b);
            }

            wnd.AchievementImage.Source = blue ? wnd.QuestSprite.Source : wnd.AchievementSprite.Source;
        }

        public static void Save()
        {
            JObject s = new JObject();

            s.Add("type", "achievement");
            s.Add("title", title);
            s.Add("description", description);
            s.Add("blue", blue);

            var wnd = ProjectManager.Get().window;

            foreach (var item in wnd.MainPanel.Children)
            {
                item.IsVisible = false;
            }

            wnd.TitleBar.IsVisible = true;
            wnd.HomePanel.IsVisible = true;
        }

        public static void Edit(string path)
        {
            var wnd = ProjectManager.Get().window;

            foreach (var item in wnd.MainPanel.Children)
            {
                item.IsVisible = false;
            }

            wnd.TitleBar.IsVisible = true;
            wnd.AchievementPanel.IsVisible = true;

            string title = "";
            string description = "";
            bool blue = false;

            try
            {
                JObject rss = JObject.Parse(File.ReadAllText(path));
                title = (string)rss["title"];
            }
            catch (Exception)
            {

            }

            try
            {
                JObject rss = JObject.Parse(File.ReadAllText(path));
                description = (string)rss["description"];
            }
            catch (Exception)
            {

            }

            try
            {
                JObject rss = JObject.Parse(File.ReadAllText(path));
                blue = bool.Parse(rss["blue"].ToString());
            }
            catch (Exception)
            {

            }

            EditAchievement.title = title;
            EditAchievement.description = description;
            EditAchievement.path = path;
            EditAchievement.blue = blue;

            Create();
        }

        public static void OnBlueToggle()
        {
            blue = (bool)ProjectManager.Get().window.BlueBtn.IsChecked;

            Create();
        }

        public static void OnText()
        {
            title = ProjectManager.Get().window.TitleInput.Text;
            description = ProjectManager.Get().window.DescriptionInput.Text;

            Create();
        }
    }
}
