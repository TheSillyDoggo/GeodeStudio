using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeodeIDE
{
    public class MovablePanel : Panel
    {
        private bool _isPressed;
        private Point _positionInBlock;
        private TranslateTransform _transform = null!;

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            Debug.WriteLine("a");

            _isPressed = true;
            _positionInBlock = e.GetPosition(this);

            if (_transform != null!)
                _positionInBlock = new Point(
                    _positionInBlock.X - _transform.X,
                    _positionInBlock.Y - _transform.Y);

            base.OnPointerPressed(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            Debug.WriteLine("b");

            _isPressed = false;

            base.OnPointerReleased(e);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (!_isPressed)
                return;

            if (Parent == null)
                return;

            var currentPosition = e.GetPosition(this);

            var offsetX = currentPosition.X - _positionInBlock.X;
            var offsetY = currentPosition.Y - _positionInBlock.Y;

            _transform = new TranslateTransform(offsetX, offsetY);
            RenderTransform = _transform;

            base.OnPointerMoved(e);
        }
    }
}
