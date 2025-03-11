using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Laba1Form
{
    public partial class Form1 : Form
    {
        private Bitmap main_image;
        private int pictureBoxTop = 154;
        private List<Layer> layers = new List<Layer>();
        private int maxHeight = 0;
        private int maxWidh = 0;


        private CurveControl curveControl;
        private HistogramControl histogramControl;

        public Form1()
        {
            InitializeComponent();
            main_image = new Bitmap(mainPBox.Width, mainPBox.Height);
            mainPBox.Image = main_image;

            curveControl = new CurveControl
            {
                Size = new Size(371, 371),
                Location = new Point(1200, 40)
            };

            curveControl.CurveUpdated += new Action<List<PointF>>(ApplyCurveToImage);
            this.Controls.Add(curveControl);

            Button bResetCurve = new Button
            {
                Location = new Point(1580, 50),
                Text = "Сбросить",
                AutoSize = true,
                UseVisualStyleBackColor = true
            };

            bResetCurve.Click += (s, ev) => ResetCurve();
            this.Controls.Add(bResetCurve);

            histogramControl = new HistogramControl
            {
                Size = new Size(371, 150),
                Location = new Point(1200, 430),
                
            };

            this.Controls.Add(histogramControl);
        }

        private void ResetCurve()
        {
            ApplyCurveToImage(curveControl.ResetPoint());
        }

        private void ApplyCurveToImage(List<PointF> controlPoints)
        {
            if (mainPBox.Image == null) return;

            // Создаём LUT (таблицу соответствий) на основе кривой
            byte[] lut = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                float x = i / 255f * curveControl.Width;
                float newY = InterpolateCurve(x, controlPoints);
                lut[i] = (byte)(255 - (newY / curveControl.Height * 255));
            }
            Bitmap sourceImage = new Bitmap(main_image);
            Bitmap newImage = new Bitmap(sourceImage.Width, sourceImage.Height, PixelFormat.Format32bppArgb);

            BitmapData sourceData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
                                                         ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData newData = newImage.LockBits(new Rectangle(0, 0, newImage.Width, newImage.Height),
                                                   ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int bytes = Math.Abs(sourceData.Stride) * sourceImage.Height;
            byte[] sourceBuffer = new byte[bytes];
            byte[] newBuffer = new byte[bytes];

            Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, bytes);
            sourceImage.UnlockBits(sourceData);

            for (int i = 0; i < bytes; i += 4) 
            {
                newBuffer[i] = lut[sourceBuffer[i]];       
                newBuffer[i + 1] = lut[sourceBuffer[i + 1]]; 
                newBuffer[i + 2] = lut[sourceBuffer[i + 2]]; 
                newBuffer[i + 3] = sourceBuffer[i + 3];    
            }

            Marshal.Copy(newBuffer, 0, newData.Scan0, bytes);
            newImage.UnlockBits(newData);

            mainPBox.Image = newImage;

            histogramControl.UpdateHistogram(newImage);
        }



        private float InterpolateCurve(float x, List<PointF> points)
        {
            // Интерполяция значений между контрольными точками
            for (int i = 0; i < points.Count - 1; i++)
            {
                if (x >= points[i].X && x <= points[i + 1].X)
                {
                    float t = (x - points[i].X) / (points[i + 1].X - points[i].X);
                    return points[i].Y + t * (points[i + 1].Y - points[i].Y);
                }
            }
            return points[points.Count - 1].Y;
        }

        private void bOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Картинки|*.png;*.jpg;*.bmp;*.gif|Все файлы|*.*";
                openFileDialog.RestoreDirectory = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Bitmap image = new Bitmap(openFileDialog.FileName);
                    AddLayer(image);
                }
                resultLabel.Visible = false;
            }
        }

        private void AddLayer(Bitmap image)
        {
            PictureBox pictureBox = new PictureBox
            {
                Size = new Size(195, 122),
                Location = new Point(948, pictureBoxTop),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = image
            };
            this.Controls.Add(pictureBox);

            Label deleteLabel = new Label
            {
                Text = "×",
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.Red,
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(20, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Location = new Point(pictureBox.Right + 20, pictureBox.Top)
            };
            deleteLabel.Click += (s, ev) =>
            {
                int index = layers.FindIndex(l => l.DeleteLabel == deleteLabel);
                if (index != -1) DeleteLayer(index);
            };
            this.Controls.Add(deleteLabel);

            ListBox effectsListBox = new ListBox
            {
                Items = { "Нет", "Сумма", "Разность", "Умножение", "Среднее", "Максимум", "Минимум" },
                Size = new Size(195, 60),
                Location = new Point(pictureBox.Left, pictureBox.Top + pictureBox.Height + 5)
            };
            effectsListBox.SelectedIndexChanged += (s, ev) => UpdateMainImage();
            this.Controls.Add(effectsListBox);

            TrackBar opacityTrackBar = new TrackBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 100,
                TickFrequency = 10,
                Size = new Size(195, 30),
                Location = new Point(pictureBox.Left, effectsListBox.Top + effectsListBox.Height + 5)
            };
            opacityTrackBar.Scroll += (s, ev) => UpdateMainImage();
            this.Controls.Add(opacityTrackBar);

            int checkBoxY = opacityTrackBar.Top + opacityTrackBar.Height;

            CheckBox redCheck = new CheckBox
            {
                Text = "R",
                Checked = true,
                Location = new Point(pictureBox.Left, checkBoxY)
            };
            CheckBox greenCheck = new CheckBox
            {
                Text = "G",
                Checked = true,
                Visible = true,
                Location = new Point(pictureBox.Left + 50, checkBoxY)
            };
            CheckBox blueCheck = new CheckBox
            {
                Text = "B",
                Checked = true,
                Location = new Point(pictureBox.Left + 100, checkBoxY)
            };

            redCheck.BackColor = Color.Red;
            redCheck.ForeColor = Color.White;

            greenCheck.BackColor = Color.Green;
            greenCheck.ForeColor = Color.White;

            blueCheck.BackColor = Color.Blue;
            blueCheck.ForeColor = Color.White;

            redCheck.AutoSize = true;
            greenCheck.AutoSize = true;
            blueCheck.AutoSize = true;

            redCheck.CheckedChanged += (s, ev) => UpdateMainImage();
            greenCheck.CheckedChanged += (s, ev) => UpdateMainImage();
            blueCheck.CheckedChanged += (s, ev) => UpdateMainImage();

            this.Controls.Add(redCheck);
            this.Controls.Add(greenCheck);
            this.Controls.Add(blueCheck);

            layers.Add(new Layer
            {
                Image = image,
                BlendModeBox = effectsListBox,
                OpacityBar = opacityTrackBar,
                RedChannel = redCheck,
                GreenChannel = greenCheck,
                BlueChannel = blueCheck,
                DeleteLabel = deleteLabel,
                PictureBox = pictureBox
            });

            if (image.Width > maxWidh) maxWidh = image.Width;
            if (image.Height > maxHeight) maxHeight = image.Height;
            pictureBoxTop += 280;
            UpdateMainImage();
        }

        private void DeleteLayer(int index)
        {
            if (index < 0 || index >= layers.Count) return;


            Layer layer = layers[index];
            this.Controls.Remove(layer.DeleteLabel);
            this.Controls.Remove(layer.RedChannel);
            this.Controls.Remove(layer.GreenChannel);
            this.Controls.Remove(layer.BlueChannel);
            this.Controls.Remove(layer.OpacityBar);
            this.Controls.Remove(layer.BlendModeBox);
            this.Controls.Remove(layer.PictureBox);

            // Удаляем слой из списка
            layers.RemoveAt(index);

            pictureBoxTop = 154;
            foreach (var l in layers)
            {
                l.PictureBox.Location = new Point(948, pictureBoxTop);
                l.BlendModeBox.Location = new Point(948, pictureBoxTop + 122 + 5);
                l.OpacityBar.Location = new Point(948, pictureBoxTop + 122 + 5 + 60);
                l.RedChannel.Location = new Point(948, pictureBoxTop + 122 + 30 + 60 + 30);
                l.GreenChannel.Location = new Point(948 + 50, pictureBoxTop + 122 + 30 + 60 + 30);
                l.BlueChannel.Location = new Point(948 + 100, pictureBoxTop + 122 + 30 + 60 + 30);
                l.DeleteLabel.Location = new Point(l.PictureBox.Right + 20, l.PictureBox.Top);

                pictureBoxTop += 280;
            }

            UpdateMainImage();


            if (layers.Count == 0)
            {
                mainPBox.Image = null;
            }
        }

        private void UpdateMainImage()
        {
            if (layers.Count == 0) return;

            Bitmap result = new Bitmap(maxWidh, maxHeight, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.Clear(Color.Transparent);
            }

            foreach (var layer in layers)
            {
                ApplyLayerOptimized(result, layer);
            }

            mainPBox.Image?.Dispose();
            mainPBox.Image = result;

            main_image = result;

            ApplyCurveToImage(curveControl.GetControlPoints());
            histogramControl.UpdateHistogram(main_image);
        }


        private void ApplyLayerOptimized(Bitmap baseImage, Layer layer)
        {
            if (layer.Image == null) return;

            float opacity = layer.OpacityBar.Value / 100f;
            BlendMode mode = GetBlendMode(layer.BlendModeBox.SelectedItem?.ToString() ?? "Нет");

            if (layer.Image.Width != baseImage.Width || layer.Image.Height != baseImage.Height)
            {
                layer.Image = ScaleImage(layer.Image, baseImage.Width, baseImage.Height);
            }

            // Блокируем память для быстрого доступа к данным
            BitmapData baseData = baseImage.LockBits(new Rectangle(0, 0, baseImage.Width, baseImage.Height),
                                                     ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData layerData = layer.Image.LockBits(new Rectangle(0, 0, layer.Image.Width, layer.Image.Height),
                                                        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            int stride = baseData.Stride;
            IntPtr basePtr = baseData.Scan0;
            IntPtr layerPtr = layerData.Scan0;
            int bytes = stride * baseImage.Height;

            byte[] baseBuffer = new byte[bytes];
            byte[] layerBuffer = new byte[bytes];

            // Копируем данные изображения в массивы
            Marshal.Copy(basePtr, baseBuffer, 0, bytes);
            Marshal.Copy(layerPtr, layerBuffer, 0, bytes);

            for (int y = 0; y < baseImage.Height; y++)
            {
                for (int x = 0; x < baseImage.Width; x++)
                {
                    int index = (y * stride) + (x * 4); // 4 байта на пиксель (BGRA)

                    byte b1 = baseBuffer[index];
                    byte g1 = baseBuffer[index + 1];
                    byte r1 = baseBuffer[index + 2];
                    byte a1 = baseBuffer[index + 3];

                    byte b2 = layerBuffer[index];
                    byte g2 = layerBuffer[index + 1];
                    byte r2 = layerBuffer[index + 2];
                    byte a2 = (byte)(layerBuffer[index + 3] * opacity);

                    Color basePixel = Color.FromArgb(a1, r1, g1, b1);
                    Color layerPixel = Color.FromArgb(a2, r2, g2, b2);
                    Color blendedPixel = BlendPixels(basePixel, layerPixel, mode, opacity, layer);

                    // Записываем обработанные пиксели обратно
                    baseBuffer[index] = blendedPixel.B;
                    baseBuffer[index + 1] = blendedPixel.G;
                    baseBuffer[index + 2] = blendedPixel.R;
                    baseBuffer[index + 3] = blendedPixel.A;
                }
            }

            // Копируем измененные данные обратно в Bitmap
            Marshal.Copy(baseBuffer, 0, basePtr, bytes);

            baseImage.UnlockBits(baseData);
            layer.Image.UnlockBits(layerData);
        }

        private Color BlendPixels(Color basePixel, Color layerPixel, BlendMode mode, float opacity, Layer layer)
        {
            int r, g, b;

            // Применяем маску каналов
            if (!layer.RedChannel.Checked) layerPixel = Color.FromArgb(layerPixel.A, 0, layerPixel.G, layerPixel.B);
            if (!layer.GreenChannel.Checked) layerPixel = Color.FromArgb(layerPixel.A, layerPixel.R, 0, layerPixel.B);
            if (!layer.BlueChannel.Checked) layerPixel = Color.FromArgb(layerPixel.A, layerPixel.R, layerPixel.G, 0);

            switch (mode)
            {
                case BlendMode.Sum:
                    r = Clamp(basePixel.R + layerPixel.R, 0, 255);
                    g = Clamp(basePixel.G + layerPixel.G, 0, 255);
                    b = Clamp(basePixel.B + layerPixel.B, 0, 255);
                    break;
                case BlendMode.Difference:
                    r = Math.Abs(basePixel.R - layerPixel.R);
                    g = Math.Abs(basePixel.G - layerPixel.G);
                    b = Math.Abs(basePixel.B - layerPixel.B);
                    break;
                case BlendMode.Multiply:
                    r = (basePixel.R * layerPixel.R) / 255;
                    g = (basePixel.G * layerPixel.G) / 255;
                    b = (basePixel.B * layerPixel.B) / 255;
                    break;
                case BlendMode.Maximum:
                    r = Math.Max(basePixel.R, layerPixel.R);
                    g = Math.Max(basePixel.G, layerPixel.G);
                    b = Math.Max(basePixel.B, layerPixel.B);
                    break;
                case BlendMode.Minimum:
                    r = Math.Min(basePixel.R, layerPixel.R);
                    g = Math.Min(basePixel.G, layerPixel.G);
                    b = Math.Min(basePixel.B, layerPixel.B);
                    break;
                case BlendMode.Averange:
                    r = (basePixel.R + layerPixel.R) / 2;
                    g = (basePixel.G + layerPixel.G) / 2;
                    b = (basePixel.B + layerPixel.B) / 2;
                    break;
                default:
                    r = layerPixel.R;
                    g = layerPixel.G;
                    b = layerPixel.B;
                    break;
            }

            // Применяем прозрачность только к активным каналам
            r = (int)(basePixel.R * (1 - opacity) + r * opacity);
            g = (int)(basePixel.G * (1 - opacity) + g * opacity);
            b = (int)(basePixel.B * (1 - opacity) + b * opacity);

            return Color.FromArgb(
                Clamp(r, 0, 255),
                Clamp(g, 0, 255),
                Clamp(b, 0, 255)
            );
        }

        private BlendMode GetBlendMode(string mode)
        {
            return mode switch
            {
                "Сумма" => BlendMode.Sum,
                "Разность" => BlendMode.Difference,
                "Умножение" => BlendMode.Multiply,
                "Среднее" => BlendMode.Averange,
                "Максимум" => BlendMode.Maximum,
                "Минимум" => BlendMode.Minimum,
                _ => BlendMode.None,
            };
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void bSave_Click(object sender, EventArgs e)
        {
            mainPBox.Image.Save("..\\..\\..\\outt1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            resultLabel.Visible = true;
            resultLabel.Text = "Изображение успешно сохранено!";
        }
        static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            return value.CompareTo(min) < 0 ? min :
                   value.CompareTo(max) > 0 ? max :
                   value;
        }


        static Bitmap ScaleImage(Bitmap img, int width, int height)
        {
            Bitmap scaledImg = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(scaledImg))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(img, new Rectangle(0, 0, width, height));
            }
            return scaledImg;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void mainPBox_Click(object sender, EventArgs e)
        {

        }
    }


    public class Layer
    {
        public Bitmap Image { get; set; }
        public ListBox BlendModeBox { get; set; }
        public TrackBar OpacityBar { get; set; }
        public CheckBox RedChannel { get; set; }
        public CheckBox GreenChannel { get; set; }
        public CheckBox BlueChannel { get; set; }
        public Label DeleteLabel { get; set; } 
        public PictureBox PictureBox { get; set; }
    }

    public enum BlendMode { None, Sum, Difference, Multiply, Averange, Maximum, Minimum }
}
