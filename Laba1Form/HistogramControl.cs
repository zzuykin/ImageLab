using System;
using System.Drawing;
using System.Windows.Forms;

namespace Laba1Form
{
    public class HistogramControl : Control
    {
        private int[] histogram = new int[256];
        
        public void UpdateHistogram(Bitmap image)
        {
            if (image == null) return;

            Array.Clear(histogram, 0, histogram.Length);
            for(int y = 0; y < image.Height; y++)
            {
                for(int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    int brightness = (int)((pixel.R + pixel.G + pixel.B)/3);
                    histogram[brightness]++; 
                }
            }

            Invalidate();
        }
        private void DrawHistogram(Graphics g, Rectangle bounds)
        {
            if (histogram == null) return;

            int max = histogram.Max();
            if (max == 0) return;

            int width = bounds.Width;
            int height = bounds.Height;
            Point[] points = new Point[histogram.Length + 2];

            points[0] = new Point(0, height);

            for (int i = 0; i < histogram.Length; i++)
            {
                int x = i * width / histogram.Length;
                int y = height - (histogram[i] * height / max); 
                points[i + 1] = new Point(x, y);
            }

            // Правая нижняя точка (закрываем область)
            points[histogram.Length + 1] = new Point(width, height);

            using (Brush fillBrush = new SolidBrush(Color.FromArgb(100, Color.Blue)))
            using (Pen linePen = new Pen(Color.Blue, 2))
            {
                g.FillPolygon(fillBrush, points); 
                g.DrawLines(linePen, points.Skip(1).Take(histogram.Length).ToArray()); 
            }
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawHistogram(e.Graphics, this.ClientRectangle);
            //if (histogram == null) return;

            //int maxVal = 1;
            //foreach(int value in histogram)
            //{
            //    if (value > maxVal) maxVal = value;
            //}
            //double X = (double)Width / histogram.Length; ;
            //double Y = (double)Height / maxVal;

            //using(Pen pen = new Pen(Color.Gray))
            //{
            //    for(int i = 0; i < histogram.Length - 1; i++)
            //    {
            //        int x1 = (int)(i * X);
            //        int y1 = Height - (int)(histogram[i] * Y);

            //        int x2 = (int)((i + 1) * X);
            //        int y2 = Height - (int)(histogram[i + 1] * Y);

            //        e.Graphics.DrawLine(pen,x1,y1,x2,y2);   
            //    }
            //}

        }
    }
}
