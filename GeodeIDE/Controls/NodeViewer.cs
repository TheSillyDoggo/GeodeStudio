using Avalonia.Controls;
using Avalonia;
using Avalonia.Media;
using System;
using GeodeIDE;
using Avalonia.Input;
using System.Diagnostics;
using Avalonia.Interactivity;
using System.Xml.Linq;
using Avalonia.Media.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Skia;
using SkiaSharp;
using System.Numerics;
using System.Security.Cryptography;

namespace CustomControls
{
    public class NodeViewer : Control
    {
        private static NodeViewer Instance = null;
        public static NodeViewer Get() { return Instance; }

        private DrawingContext context;

        public Point position = new Point(0, 0);
        private Point p;

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            Instance = this;

            position = new Point(0 - Bounds.Width / 2, 0 - Bounds.Height / 2);
            p = new Point(0 - Bounds.Width / 2, 0 - Bounds.Height / 2);
        }

        public override void Render(DrawingContext context)
        {
            //Update Every Frame
            TopLevel.GetTopLevel(this).RequestAnimationFrame(delegate { InvalidateVisual(); });


            this.context = context;
            context.FillRectangle(new SolidColorBrush(new Color(255, 17, 17, 17)), new Rect(0,0, Bounds.Width, Bounds.Height));

            position = position.Lerp(p, 0.1f);

            if (ProjectManager.Get().layer == null)
                return;

            DrawDots();

            if (draggingMouse)
            {
                context.DrawLine(new Pen(new SolidColorBrush(new Color(255, 255, 255, 255)), 3), ProjectManager.Get().layer.graph.nodes[nIndex].position + position, dragMouseLine);
            }

            ProjectManager.Get().layer.graph.Draw(context);
        }

        public void Draw9Sprite(IImage image, Rect rect, DrawingContext context)
        {
            float scale = 50;

            //Middle
            context.DrawImage(image, new Rect(image.Size.Width / 3, image.Size.Height / 3, image.Size.Width / 3, image.Size.Height / 3), new Rect(rect.Position.X + scale, rect.Position.Y + scale, rect.Width - scale * 2, rect.Height - scale * 2));

            //Corners
            context.DrawImage(image, new Rect(0, 0, image.Size.Width / 3, image.Size.Height / 3), new Rect(rect.Position.X, rect.Position.Y, scale, scale));
            context.DrawImage(image, new Rect(image.Size.Width / 3 * 2, 0, image.Size.Width / 3, image.Size.Height / 3), new Rect(rect.Position.X + rect.Width - scale, rect.Position.Y, scale, scale));
            context.DrawImage(image, new Rect(0, image.Size.Height / 3 * 2, image.Size.Width / 3, image.Size.Height / 3), new Rect(rect.Position.X, rect.Position.Y + rect.Height - scale, scale, scale));
            context.DrawImage(image, new Rect(image.Size.Width / 3 * 2, image.Size.Height / 3 * 2, image.Size.Width / 3, image.Size.Height / 3), new Rect(rect.Position.X + rect.Width - scale, rect.Position.Y + rect.Height - scale, scale, scale));

            //Edges
            context.DrawImage(image, new Rect(image.Size.Width / 3, 0, image.Size.Width / 3, image.Size.Height / 3), new Rect(rect.Position.X + scale, rect.Position.Y, rect.Width - scale * 2, scale));
            context.DrawImage(image, new Rect(image.Size.Width / 3, image.Size.Height / 3 * 2, image.Size.Width / 3, image.Size.Height / 3), new Rect(rect.Position.X + scale, rect.Position.Y + rect.Height - scale, rect.Width - scale * 2, scale));
            context.DrawImage(image, new Rect(0, image.Size.Height / 3, image.Size.Width / 3, image.Size.Height / 3), new Rect(rect.Position.X, rect.Position.Y + scale, scale, rect.Height - scale * 2));
            context.DrawImage(image, new Rect(image.Size.Width / 3 * 2, image.Size.Height / 3, image.Size.Width / 3, image.Size.Height / 3), new Rect(rect.Position.X + rect.Width - scale, rect.Position.Y + scale, scale, rect.Height - scale * 2));
        }

        public void DrawDots()
        {
            for (int x = 0; x < Bounds.Width / 30; x++)
            {
                for (int y = 0; y < Bounds.Height / 30; y++)
                {
                    float xOFF = x * 30, yOFF = y * 30;

                    xOFF += (float)position.X;

                    while (xOFF < 0)
                    {
                        xOFF += (float)Bounds.Width;
                    }

                    while (xOFF > Bounds.Width)
                    {
                        xOFF -= (float)Bounds.Width;
                    }

                    yOFF += (float)position.Y;

                    while (yOFF < 0)
                    {
                        yOFF += (float)Bounds.Height;

                        //why tf is this only needed for the y axis?
                        //it breaks the dots on x
                        yOFF += 15;
                    }

                    while (yOFF > Bounds.Height)
                    {
                        yOFF -= (float)Bounds.Height;

                        //yet another example of stuff that has no explanation :(
                        yOFF -= 15;
                    }

                    context.DrawEllipse(new SolidColorBrush(new Color(50, 255, 255, 255)), new Pen(), new Point(xOFF, yOFF), 2, 2);
                }
            }
        }

        public Point cOrigin, nOrigin, dragMouseLine;
        public bool held, draggingMouse;
        public int nIndex = -1;

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (ProjectManager.Get().layer.graph.getOverNode(e.GetPosition(this)) != null)
            {
                ProjectManager.Get().layer.graph.nodes.Move(ProjectManager.Get().layer.graph.nodes.IndexOf(ProjectManager.Get().layer.graph.getOverNode(e.GetPosition(this))), ProjectManager.Get().layer.graph.nodes.Count);
            }

            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                switch (ProjectManager.Get().layer.graph.isOverNode(e.GetCurrentPoint(this).Position))
                {
                    case 0:

                        cOrigin = p - e.GetPosition(this);
                        held = true;

                        break;

                    case 1:

                        nOrigin = ProjectManager.Get().layer.graph.getOverNode(e.GetPosition(this)).position - e.GetPosition(this);
                        nIndex = ProjectManager.Get().layer.graph.nodes.IndexOf(ProjectManager.Get().layer.graph.getOverNode(e.GetPosition(this)));
                        held = true;
                        break;
                    case 2:

                        draggingMouse = true;
                        nIndex = ProjectManager.Get().layer.graph.nodes.IndexOf(ProjectManager.Get().layer.graph.getOverNode(e.GetPosition(this)) );
                        dragMouseLine = e.GetPosition(this);
                        ProjectManager.Get().layer.graph.nodes[nIndex].connected = null;

                        break;
                    default:
                        break;
                }
            }
            else if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {

            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (held)
            {
                if (nIndex != -1)
                {
                    ProjectManager.Get().layer.graph.nodes[nIndex].position = e.GetPosition(this) + nOrigin;
                }
                else
                {
                    p = e.GetPosition(this) + cOrigin;
                }
            }

            if (draggingMouse)
            {
                dragMouseLine = e.GetPosition(this);
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (draggingMouse)
            {
                Debug.WriteLine(ProjectManager.Get().layer.graph.nodes.IndexOf(ProjectManager.Get().layer.graph.getOverNode(e.GetPosition(this))));
                ProjectManager.Get().layer.graph.nodes[nIndex].connected = ProjectManager.Get().layer.graph.getOverNode(e.GetPosition(this));
            }

            draggingMouse = false;
            held = false;
            nIndex = -1;
        }
    }
}