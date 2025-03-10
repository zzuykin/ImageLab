using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MathNet.Numerics.Interpolation;

namespace Laba1Form
{


    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using MathNet.Numerics.Interpolation;

    public class CurveControl : Control
    {
        public event Action<List<PointF>> CurveUpdated;
        private List<PointF> points;
        private int selectedPointIndex = -1;
        private const int PointSize = 8;

        public CurveControl()
        {
            this.Size = new Size(370, 370); // Установленный размер
            this.DoubleBuffered = true;

            points = new List<PointF>
        {
            new PointF(0, 370),    // Нижний левый угол
            //new PointF(92, 278),   // Промежуточная точка
            new PointF(185, 185),  // Средняя точка
            //new PointF(278, 92),   // Промежуточная точка
            new PointF(370, 0)     // Верхний правый угол
        };
        }

        public List<PointF> GetControlPoints()
        {
            return points;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (Math.Abs(points[i].X - e.X) < PointSize && Math.Abs(points[i].Y - e.Y) < PointSize)
                {
                    selectedPointIndex = i;
                    return;
                }
            }
            points.Add(new PointF(e.X, e.Y));
            points = points.OrderBy(p => p.X).ToList();
            Invalidate();
            CurveUpdated?.Invoke(GetInterpolatedCurve());
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (selectedPointIndex != -1 && e.Button == MouseButtons.Left)
            {
                points[selectedPointIndex] = new PointF(e.X, e.Y);
                Invalidate();
                CurveUpdated?.Invoke(GetInterpolatedCurve());
            }
        }

        protected override void OnMouseUp(MouseEventArgs e) => selectedPointIndex = -1;

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            using (var pen = new Pen(Color.Black, 2))
            {
                var curve = GetInterpolatedCurve();
                for (int i = 1; i < curve.Count; i++)
                    e.Graphics.DrawLine(pen, curve[i - 1], curve[i]);
            }
            foreach (var p in points)
                e.Graphics.FillEllipse(Brushes.Red, p.X - PointSize / 2, p.Y - PointSize / 2, PointSize, PointSize);
        }

        private List<PointF> GetInterpolatedCurve()
        {
            if (points.Count < 2) return new List<PointF>(points);
            var xValues = points.Select(p => (double)p.X).ToArray();
            var yValues = points.Select(p => (double)p.Y).ToArray();
            var spline = CubicSpline.InterpolateNatural(xValues, yValues);
            var result = new List<PointF>();
            for (int x = 0; x < Width; x++)
                result.Add(new PointF(x, (float)spline.Interpolate(x)));
            return result;
        }
    }


}
