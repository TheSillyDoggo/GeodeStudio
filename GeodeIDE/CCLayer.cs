using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;   
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VisualGeode;

namespace GeodeIDE
{
    public class CCLayer
    {
        public class CCNode
        {
            public enum nType
            {
                Node,
                Button,
                Label
            }

            public nType type = nType.Node;

            public string name = "Default Name";

            public bool enabled, expandInHierarchy = false, cclayer9sprite = false;

            public Point position, size, anchor;

            public Alignment alignment = Alignment.CENTER;

            public string spriteName = "", fontName = "", labelText = "";

            public float rotation = 0, alpha = 1;

            public List<CCNode> children = new List<CCNode>();

            public Image img;

            public Canvas c;

            public (IImage, int) GetImage()
            {
                if (Program.gdAssets.ContainsKey(spriteName))
                {
                    Bitmap bmp;
                    Program.gdAssets.TryGetValue(spriteName, out bmp);

                    return (bmp, 0);
                }
                else if (Program.gdSheetAssets.ContainsKey(spriteName))
                {
                    Dictionary<CroppedBitmap, bool> bmp;
                    Program.gdSheetAssets.TryGetValue(spriteName, out bmp);


                    return (bmp.ElementAt(0).Key, bmp.ElementAt(0).Value ? 90 : 0);
                }

                return (null, 0);
            }

            //854x480
            public Point GetSizeForAlignment()
            {
                Point point = size;

                point = new Point(point.X / 1920 * 854, point.Y / 1080 * 480);

                if (alignment == Alignment.STRETCH)
                {
                    point = new Point(854 - position.X - size.Y, 480 - position.Y - size.X);
                }
                else
                {
                    if (Program.gdSheetAssets.ContainsKey(spriteName))
                    {
                        Dictionary<CroppedBitmap, bool> v;
                        Program.gdSheetAssets.TryGetValue(spriteName, out v);

                        if (v.ElementAt(0).Value)
                        {
                            point = new Point(point.Y, point.X);
                        }
                    }
                }

                return point;
            }

            public Point GetPositionForAlignment(bool editor)
            {
                //i wrote this at 11 - 12 pm and have no idea what most of this does

                Point point = new Point();

                switch (alignment)
                {
                    case Alignment.CENTER:
                        if (editor)
                        {
                            point = new Point(854 / 2 + ((position.X / 1920) * 854), 480 / 2 + (position.Y / 1080 * 480));
                            point -= new Point((size.X / 1920 * 854) * anchor.X, -((size.Y / 1080 * 480) * anchor.Y));
                        }
                        break;
                    case Alignment.STRETCH:
                        //this shit mode caused so much pain for me even though it's this simple :(
                        //and i don't give a fuch that it isn't the 1920x1080 resolution thats for later
                        point = new Point(position.X, 480 - position.Y);
                        break;
                    case Alignment.LEFT:
                        if (editor)
                        {
                            point = new Point(((position.X / 1920) * 854), 480 / 2 + (position.Y / 1080 * 480));
                            point -= new Point((size.X / 1920 * 854) * anchor.X, -((size.Y / 1080 * 480) * anchor.Y));
                        }
                        break;
                    case Alignment.RIGHT:
                        if (editor)
                        {
                            point = new Point(854 - ((position.X / 1920) * 854), 480 / 2 + (position.Y / 1080 * 480));
                            point -= new Point((size.X / 1920 * 854) * anchor.X, -((size.Y / 1080 * 480) * anchor.Y));
                        }
                        break;
                    case Alignment.TOP:
                        if (editor)
                        {
                            point = new Point(854 / 2 + ((position.X / 1920) * 854), 480 - (position.Y / 1080 * 480));
                            point -= new Point((size.X / 1920 * 854) * anchor.X, -((size.Y / 1080 * 480) * anchor.Y));
                        }
                        break;
                    case Alignment.BOTTOM:
                        if (editor)
                        {
                            point = new Point(854 / 2 + ((position.X / 1920) * 854), position.Y / 1080 * 480);
                            point -= new Point((size.X / 1920 * 854) * anchor.X, -((size.Y / 1080 * 480) * anchor.Y));
                        }
                       
                        break;
                    case Alignment.TOPLEFT:
                        if (editor)
                        {
                            point = new Point((position.X / 1920) * 854, 480 - (position.Y / 1080 * 480));
                            point -= new Point((size.X / 1920 * 854) * anchor.X, -((size.Y / 1080 * 480) * anchor.Y));
                        }
                        break;
                    case Alignment.BOTTOMLEFT:
                        if (editor)
                        {
                            point = new Point((position.X / 1920) * 854, position.Y / 1080 * 480);
                            point -= new Point((size.X / 1920 * 854) * anchor.X, -((size.Y / 1080 * 480) * anchor.Y));
                        }
                        break;
                    case Alignment.BOTTOMRIGHT:
                        if (editor)
                        {
                            point = new Point(854 - ((position.X / 1920) * 854), position.Y / 1080 * 480);
                            point -= new Point((size.X / 1920 * 854) * anchor.X, -((size.Y / 1080 * 480) * anchor.Y));
                        }
                        break;
                    case Alignment.TOPRIGHT:
                        if (editor)
                        {
                            point = new Point(854 - ((position.X / 1920) * 854), 480 - (position.Y / 1080 * 480));
                            point -= new Point((size.X / 1920 * 854) * anchor.X, -((size.Y / 1080 * 480) * anchor.Y));
                        }
                        break;
                    default:
                        point = position;
                        break;
                }

                if (type == nType.Label)
                {
                    if (c != null)
                        point -= new Point(c.Width / 2, 0);
                }

                if (editor)
                    point = new Point(point.X, 480 - point.Y);
                else
                    point = new Point(position.X / 1920, position.Y / 1080);

                return point;
            }

            public void RefreshSprite()
            {
                return;


                img.RenderTransformOrigin = RelativePoint.Center;
                RotateTransform rT = new RotateTransform();
                float off = 0;

                if (type == nType.Node)
                {
                    if (Program.gdAssets.ContainsKey(spriteName))
                    {
                        Bitmap bmp;
                        Program.gdAssets.TryGetValue(spriteName, out bmp);
                        img.Source = bmp;
                    }
                    else if (Program.gdSheetAssets.ContainsKey(spriteName))
                    {
                        Dictionary<CroppedBitmap, bool> bmp;
                        Program.gdSheetAssets.TryGetValue(spriteName, out bmp);
                        img.Source = bmp.ElementAt(0).Key;

                        if (bmp.ElementAt(0).Value)
                        {
                            off -= 90;
                        }
                    }
                }
                else if (type == nType.Label)
                {
                    img.IsVisible = false;

                    if (c != null)
                    {
                        Panel pnl = ((Panel)(ProjectManager.Get().window.CCLayerPanel));
                        if (pnl.Children.Count > 0)
                        {
                            ((Canvas)(pnl.Children.ElementAt(0))).Children.Remove(c);

                            c = LabelBMFont.CreateAsChildren(labelText, fontName, 0.2f, LabelBMFont.Alignment.Center);

                            ((Canvas)(pnl.Children.ElementAt(0))).Children.Add(c);
                        }
                    }

                    //((Canvas)(ProjectManager.GetManager().window.CCLayerPanel.Children[0])).Children.Add(c);
                }

                rT.Angle = (off + rotation);
                img.RenderTransform = rT;

                ProjectManager.Get().window.SpriteName.Content = spriteName;
            }

            public enum Alignment
            {
                CENTER, STRETCH, LEFT, RIGHT, TOP, BOTTOM, TOPLEFT, BOTTOMLEFT, BOTTOMRIGHT, TOPRIGHT
            }
        }

        public string name;
        public bool expaned = false;

        public List<CCNode> nodes = new List<CCNode>();
        public NodeGraph graph = new NodeGraph();

        public static Canvas CreateAsChildren(CCLayer layer)
        {
            var c = new Canvas();

            foreach (var node in layer.nodes)
            {
                var a = new Image();
                node.img = a;

                if (node.type == CCNode.nType.Label)
                {
                    node.c = LabelBMFont.CreateAsChildren(node.labelText, node.fontName, 0.2f, LabelBMFont.Alignment.Center);
                }
                else
                {
                    node.c = null;
                }

                a.Stretch = Stretch.Fill;

                c.Children.Add(a);
                
                if (node.c != null && node.c.GetVisualParent() == null)
                    c.Children.Add(node.c);
            }

            return c;
        }
    }
}
