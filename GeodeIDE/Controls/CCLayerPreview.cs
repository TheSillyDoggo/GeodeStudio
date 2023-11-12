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

namespace CustomControls
{
    public class CCLayerPreview : Control
    {
        private DrawingContext context;

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            TopLevel.GetTopLevel(this).KeyUp += OnKeyUp;
        }

        public override void Render(DrawingContext context)
        {
            context.FillRectangle(Brushes.Black, new Rect(0,0, Bounds.Width, Bounds.Height));

            if (ProjectManager.Get().layer == null)
                return;

            this.context = context;

            foreach (var node in ProjectManager.Get().layer.nodes)
            {
                //Matrix translate = Matrix.CreateTranslation(200, 150);

                float off = 0;
                IImage img = null;

                img = node.GetImage().Item1;
                off = node.GetImage().Item2;

                if (node.type == CCLayer.CCNode.nType.Node)
                {
                    Matrix rotate = Matrix.CreateRotation((Math.PI / 180) * (off + node.rotation));

                    using (context.PushTransform(rotate))
                    {
                        if (img != null)
                        {
                            if (node.enabled)
                            {
                                if (node.cclayer9sprite)
                                    Draw9Sprite(img, new Rect(node.GetPositionForAlignment(true), new Size(node.GetSizeForAlignment().X, node.GetSizeForAlignment().Y)), context);
                                else
                                    context.DrawImage(img, new Rect(node.GetPositionForAlignment(true), new Size(node.GetSizeForAlignment().X, node.GetSizeForAlignment().Y)));
                            }
                        }
                    }
                }    
            }

            TopLevel.GetTopLevel(this).RequestAnimationFrame(delegate { InvalidateVisual(); });
        }

        public void Draw9Sprite(IImage image, Rect rect, DrawingContext context)
        {
            float scale = 0.5f;

            context.DrawImage(image, new Rect(0,0, image.Size.Width / 3, image.Size.Height / 3), new Rect(rect.Position, new Size(image.Size.Width / 3, image.Size.Height / 3) * scale ));
            context.DrawImage(image, new Rect(image.Size.Width / 3 * scale,0, image.Size.Width / 3, image.Size.Height / 3), new Rect(rect.Position + new Point(image.Size.Width / 3 * scale, 0), new Size(rect.Width - image.Size.Width / 3 * 2, image.Size.Height / 3) * scale ));

            //context.DrawImage(image, new Rect(image.Size.Width / 3, 0, image.Size.Width / 3, image.Size.Height / 3), new Rect(rect.Position + new Point(rect.Width / 3, 0), new Size(rect.Width / 3, rect.Width / 3) ));
        }

        public void OnKeyUp(object? sender, KeyEventArgs e)
        {
            this.InvalidateVisual();
        }
    }
}