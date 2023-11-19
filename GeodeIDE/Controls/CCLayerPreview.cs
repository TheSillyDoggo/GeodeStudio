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

                if (node.type == CCLayer.CCNode.nType.Node || node.type == CCLayer.CCNode.nType.Button)
                {
                    Matrix rotate = Matrix.CreateRotation((Math.PI / 180) * (off + node.rotation));
                    context.PushTransform(rotate);

                    using (context.PushOpacity(node.alpha))
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

        public void OnKeyUp(object? sender, KeyEventArgs e)
        {
            this.InvalidateVisual();
        }
    }
}