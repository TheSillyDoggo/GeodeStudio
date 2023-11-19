using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CustomControls;
using GeodeIDE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualGeode
{
    public class NodeGraph
    {
        public List<Node> nodes = new List<Node>();

        public void Draw(DrawingContext context)
        {
            foreach (var node in nodes)
            {
                node.Render(context);
            }
        }

        /// <summary>
        /// Returns:
        /// 0 for not over ui
        /// 1 for over top
        /// 2 for over middle
        /// </summary>
        public int isOverNode(Point p)
        {
            int i = 0;

            foreach (var node in nodes)
            {
                var r = new Rect(node.position.X + NodeViewer.Get().position.X, node.position.Y + NodeViewer.Get().position.Y, 180, 20);

                if (p.X > r.X && p.X < r.X + r.Width && p.Y > r.Y && p.Y < r.Y + (20 + 4 + ((node.height) * 30)))
                {
                    if (p.Y < r.Y + r.Height)
                    {
                        i = 1;
                    }
                    else
                    {
                        i = 2;
                    }
                }
            }

            return i;
        }

        /// <summary>
        /// Returns null if not over anything
        /// </summary>
        public Node getOverNode(Point p)
        {
            foreach (var node in nodes)
            {
                var r = new Rect(node.position.X + NodeViewer.Get().position.X, node.position.Y + NodeViewer.Get().position.Y, 180, 20);

                if (p.X > r.X && p.X < r.X + r.Width && p.Y > r.Y && p.Y < r.Y + (20 + 4 + ((node.height) * 30)))
                {
                    return node;
                }
            }

            return null;
        }

        public class Node
        {
            public string name = "", description = "";

            public Color colour = new Color(255, 255, 0, 0);

            public Point position;

            public Node connected;



            public int height = 3;

            public void Render(DrawingContext context)
            {
                context.DrawRectangle(new SolidColorBrush(colour), null, new RoundedRect(new Rect(position.X + NodeViewer.Get().position.X, position.Y + NodeViewer.Get().position.Y, 180, 20), 5, 5, 0, 0));
                context.DrawRectangle(new SolidColorBrush(new Color(100, 0, 0, 0)), null, new RoundedRect(new Rect(position.X + NodeViewer.Get().position.X, position.Y + NodeViewer.Get().position.Y + 20, 180, height * 30), 0, 0, 0, 0));

                context.DrawRectangle(null, new Pen(new SolidColorBrush(new Color(100, 0, 0, 0)), 4), new RoundedRect(new Rect(position.X + NodeViewer.Get().position.X - 2, position.Y + NodeViewer.Get().position.Y - 2, 180 + 4, 20 + 4 + ((height) * 30)), 5, 5, 0, 0));

                context.DrawText(new FormattedText(name, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 14, new SolidColorBrush(new Color(255, 255, 255, 255))), new Point(position.X + NodeViewer.Get().position.X + 2, position.Y + NodeViewer.Get().position.Y));

                var pos = new Point(position.X + NodeViewer.Get().position.X, position.Y + NodeViewer.Get().position.Y);

                context.DrawEllipse(new SolidColorBrush(new Color(255, 255, 255, 255)), null, pos + new Point(5, 30), 4, 4);
                context.DrawEllipse(new SolidColorBrush(new Color(255, 255, 255, 255)), null, pos + new Point(180, 0) + new Point(-5, 30), 4, 4);

                if (connected != null)
                {
                    if (connected == this)
                    {
                        connected = null;
                    }
                    else
                        context.DrawLine(new Pen(new SolidColorBrush(new Color(255, 255, 255, 255)), 3), pos, connected.position + NodeViewer.Get().position);
                }

                OnRender(context);
            }

            public string Compile()
            {
                string data = "";

                if (connected != null)
                    data += connected.Compile();

                return data;
            }

            public virtual void OnRender(DrawingContext context) { }
        }
    }
}
