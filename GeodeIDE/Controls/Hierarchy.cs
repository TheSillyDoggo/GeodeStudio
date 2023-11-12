using Avalonia.Controls;
using Avalonia;
using Avalonia.Media;
using System;
using GeodeIDE;
using Avalonia.Input;
using System.Diagnostics;
using Avalonia.Interactivity;
using System.Xml.Linq;

namespace CustomControls
{
    public class Hierarchy : Control
    {
        public bool expandedLayer = false;

        private DrawingContext context;

        int i = 0, s = -1;

        int hovered = 0;
        public int Selected = 0;

        public int dragged = -1;
        public float offset;

        public static readonly RoutedEvent<RoutedEventArgs> ChangedSelectedEvent =
        RoutedEvent.Register<Hierarchy, RoutedEventArgs>(nameof(ChangedSelection), RoutingStrategies.Direct);

        public event EventHandler<RoutedEventArgs> ChangedSelection
        {
            add => AddHandler(ChangedSelectedEvent, value);
            remove => RemoveHandler(ChangedSelectedEvent, value);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            TopLevel.GetTopLevel(this).KeyUp += OnKeyUp;

            var ctx = new ContextMenu();
            this.ContextMenu = ctx;

            MenuItem oDir = new MenuItem() { Header = "Duplicate" };
            oDir.Click += delegate
            {
                CCLayer.CCNode n = ProjectManager.Get().layer.nodes[hovered - 1];

                n.c = null;
                n.img = null;

                ProjectManager.Get().layer.nodes.Insert(hovered, n);

                ProjectManager.Get().layer.nodes[hovered].name += " (Copy)";

                InvalidateVisual();
            };

            MenuItem rDir = new MenuItem() { Header = "Rename" };
            rDir.Click += delegate
            {

            };

            MenuItem dDir = new MenuItem() { Header = "Delete" };
            dDir.Click += delegate
            {

            };

            ctx.Items.Add(oDir);
            ctx.Items.Add(rDir);
            ctx.Items.Add(dDir);
        }

        public override void Render(DrawingContext context)
        {
            this.context = context;

            i = 0;

            if (ProjectManager.Get().layer == null)
                return;

            AddToHierarchy(ProjectManager.Get().layer.name, 0, true, ProjectManager.Get().layer.expaned);

            if (!ProjectManager.Get().layer.expaned)
                return;

            foreach (var node in ProjectManager.Get().layer.nodes)
            {
                AddToHierarchy(node.name, 1, node.children.Count > 0, node.expandInHierarchy);

                if (node.children.Count > 0)
                {
                    foreach (var child in node.children)
                    {
                        //AddToHierarchy(child.name, 2);
                    }
                }    
            }

            try
            {
                Height = 25 * i;
            }
            catch(Exception)
            {
                //no more "Visual was invalidated during the render pass" >:)
            }

            TopLevel.GetTopLevel(this).RequestAnimationFrame(delegate { InvalidateVisual(); });
        }

        public void AddToHierarchy(string name, int depth, bool hasChildren, bool expanded)
        {
            var wnd = ProjectManager.Get().window;
            var hoveredColour = new Color(255, (byte)Math.Round(wnd.PlatformSettings.GetColorValues().AccentColor1.R * 0.35f), (byte)Math.Round(wnd.PlatformSettings.GetColorValues().AccentColor1.G * 0.35f), (byte)Math.Round(wnd.PlatformSettings.GetColorValues().AccentColor1.B * 0.35f));
            var bgColour = (Selected == i) ? wnd.PlatformSettings.GetColorValues().AccentColor1 : (hovered == i ? hoveredColour : new Color(255, 17, 17, 17));

            float off = 0;

            if (s != -1)
            {
                if (i == s || i == Selected)
                {
                    if (s == Selected || i == Selected)
                        bgColour = wnd.PlatformSettings.GetColorValues().AccentColor1;
                    else
                    {
                        bgColour = new Color(255, (byte)Math.Round(wnd.PlatformSettings.GetColorValues().AccentColor1.R * 0.2f), (byte)Math.Round(wnd.PlatformSettings.GetColorValues().AccentColor1.G * 0.2f), (byte)Math.Round(wnd.PlatformSettings.GetColorValues().AccentColor1.B * 0.2f));

                        if (i != hovered)
                        {
                            off = offset;
                        }
                    }
                }
                else
                {
                    bgColour = new Color(255, 17, 17, 17);
                }
            }

            context.FillRectangle(new SolidColorBrush(bgColour), new Rect(0, off == 0 ? (25 * i) : off - 15, Bounds.Width, 25));
            context.DrawText(new FormattedText(name, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 14, new SolidColorBrush(new Color(255, 255, 255, 255))), new Point(5 + (20 * depth) + (hasChildren ? 20 : 0), off == 0 ? ((25 * i) + 2) : off - 15 + 2));

            if (hasChildren)
                context.DrawText(new FormattedText(expanded ? "ꜜ" : "ꜛ", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 36, new SolidColorBrush(new Color(100, 255, 255, 255))), new Point(3 + (off == 0 ? (20 * depth) : off), (25 * i) - 5));

            i++;
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            hovered = (int)Math.Floor((e.GetPosition(this).Y / 25));

            if (s != hovered)
            {
                offset = (float)e.GetPosition(this).Y;
            }

            this.InvalidateVisual();
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            if (!ContextMenu.IsOpen)
                hovered = -1;

            dragged = -1;
            s = -1;
            this.InvalidateVisual();
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                s = (int)Math.Floor((e.GetPosition(this).Y / 25));

                if (s > Bounds.Height)
                    s = -1;
            }
            else if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                hovered = (int)Math.Floor((e.GetPosition(this).Y / 25));

                s = -1;
            }

            this.InvalidateVisual();
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (s == -1)
            {
                this.InvalidateVisual();
                return;
            }

            if (s == (int)Math.Floor((e.GetPosition(this).Y / 25)))
            {
                Selected = s;
            }

            RoutedEventArgs args = new RoutedEventArgs(ChangedSelectedEvent);
            RaiseEvent(args);

            if (e.GetPosition(this).X < 20)
            {
                if (Selected == 0)
                { 
                    ProjectManager.Get().layer.expaned = !ProjectManager.Get().layer.expaned;
                }
                else
                {
                    ProjectManager.Get().layer.nodes[Selected - 1].expandInHierarchy = !ProjectManager.Get().layer.nodes[Selected].expandInHierarchy;
                }
            }

            s = -1;
            this.InvalidateVisual();
        }

        public void OnKeyUp(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                Selected--;

                if (Selected < 0)
                    Selected = 0;
            }

            if (e.Key == Key.Down)
            {
                Selected++;

                if (Selected > ProjectManager.Get().layer.nodes.Count)
                    Selected = ProjectManager.Get().layer.nodes.Count;
            }

            if (e.Key == Key.Enter || e.Key == Key.Right)
            {
                if (Selected == 0)
                {
                    ProjectManager.Get().layer.expaned = !ProjectManager.Get().layer.expaned;
                }
                else
                {
                    ProjectManager.Get().layer.nodes[Selected - 1].expandInHierarchy = !ProjectManager.Get().layer.nodes[Selected - 1].expandInHierarchy;
                }
            }


            if (e.Key == Key.Delete)
            {
                if (Selected != 0 && hovered != -1)
                {
                    ProjectManager.Get().layer.nodes.RemoveAt(Selected - 1);

                    if (Selected > ProjectManager.Get().layer.nodes.Count - 1)
                        Selected = ProjectManager.Get().layer.nodes.Count;
                }
            }

            EditLayer.UpdateProperties();
            this.InvalidateVisual();
        }
    }
}