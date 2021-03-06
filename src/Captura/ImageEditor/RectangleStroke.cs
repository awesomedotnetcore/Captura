﻿using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace Captura.ImageEditor
{
    public class RectangleStroke : Stroke
    {
        public RectangleStroke(StylusPointCollection StylusPoints, DrawingAttributes DrawingAttributes) : base(StylusPoints, DrawingAttributes) { }

        protected override void DrawCore(DrawingContext DrawingContext, DrawingAttributes DrawingAttribs)
        {
            var start = StylusPoints.First().ToPoint();
            var end = StylusPoints.Last().ToPoint();

            if (end.X < start.X)
            {
                var t = start.X;
                start.X = end.X;
                end.X = t;
            }

            if (end.Y < start.Y)
            {
                var t = start.Y;
                start.Y = end.Y;
                end.Y = t;
            }

            var w = end.X - start.X;
            var h = end.Y - start.Y;

            var r = new Rect(start, new Size(w <= 0 ? 1 : w, h <= 0 ? 1 : h));

            DrawingContext.DrawRectangle(null, new Pen(new SolidColorBrush(DrawingAttribs.Color), DrawingAttribs.Width), r);
        }
    }
}