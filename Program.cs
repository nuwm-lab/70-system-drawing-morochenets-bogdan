// Повний код в одному файлі (GraphApp.cs)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GraphPlotterApp
{
    /// <summary>
    /// Головний клас форми, який відповідає за малювання.
    /// </summary>
    public partial class GraphForm : Form
    {
        // === 1. Визначення констант для функції ===
        private const double X_MIN = 7.2;
        private const double X_MAX = 12;
        private const double DX = 0.5;

        // Відступ графіку від країв форми в пікселях
        private const int PADDING = 50;

        public GraphForm()
        {
            // Налаштування форми
            this.Text = "Графік функції z(x)";
            this.BackColor = Color.White;

            // === КЛЮЧОВИЙ МОМЕНТ ===
            // Встановлюємо цю властивість, щоб форма 
            // автоматично перемальовувалась при зміні розміру.
            this.ResizeRedraw = true;

            // Встановлюємо мінімальний розмір, щоб графік було видно
            this.MinimumSize = new Size(400, 300);
        }

        /// <summary>
        /// Математична функція для обчислення z(x)
        /// z = (2*sin^2(x + 2)) / (x^2 + 1)
        /// </summary>
        private double FunctionZ(double x)
        {
            double sinVal = Math.Sin(x + 2);
            double numerator = 2 * Math.Pow(sinVal, 2);
            double denominator = Math.Pow(x, 2) + 1;
            return numerator / denominator;
        }

        /// <summary>
        /// Цей метод викликається щоразу, коли форма потребує перемалювання
        /// (при запуску, при зміні розміру, при відновленні згорнутого вікна).
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Викликаємо базовий метод OnPaint
            base.OnPaint(e);

            // Отримуємо об'єкт Graphics для малювання
            Graphics g = e.Graphics;
            
            // Вмикаємо згладжування для гарнішого вигляду ліній
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // === 2. Отримання поточних розмірів вікна ===
            // ClientSize - це "корисна" область форми, без рамок і заголовку
            int width = this.ClientSize.Width;
            int height = this.ClientSize.Height;

            // Визначаємо "робочу" область для малювання (з відступами)
            float drawWidth = width - 2 * PADDING;
            float drawHeight = height - 2 * PADDING;

            // Якщо вікно занадто мале, нічого не малюємо
            if (drawWidth <= 0 || drawHeight <= 0) return;

            // === 3. Обчислення точок та знаходження zMin, zMax ===
            // Нам потрібні zMin і zMax, щоб правильно масштабувати по осі Y
            List<PointF> mathPoints = new List<PointF>();
            double zMin = double.MaxValue;
            double zMax = double.MinValue;
            double currentX = X_MIN;

            while (currentX <= X_MAX)
            {
                double currentZ = FunctionZ(currentX);
                if (currentZ < zMin) zMin = currentZ;
                if (currentZ > zMax) zMax = currentZ;

                mathPoints.Add(new PointF((float)currentX, (float)currentZ));
                currentX += DX;
            }
            
            // Додаємо кінцеву точку, якщо крок DX не "потрапив" у X_MAX
            if (Math.Abs(mathPoints[mathPoints.Count - 1].X - X_MAX) > 1e-6)
            {
                 double finalZ = FunctionZ(X_MAX);
                 if (finalZ < zMin) zMin = finalZ;
                 if (finalZ > zMax) zMax = finalZ;
                 mathPoints.Add(new PointF((float)X_MAX, (float)finalZ));
            }

            // === 4. Малювання осей та сітки ===
            using (Pen axisPen = new Pen(Color.Black, 1))
            using (Brush textBrush = new SolidBrush(Color.Black))
            using (Font textFont = new Font("Arial", 8))
            using (StringFormat sfRight = new StringFormat { Alignment = StringAlignment.Far })
            {
                // Малюємо осі (ліва та нижня межі)
                // Вісь X (горизонтальна)
                float xAxisY = PADDING + drawHeight;
                g.DrawLine(axisPen, PADDING, xAxisY, PADDING + drawWidth, xAxisY);

                // Вісь Z (вертикальна)
                float zAxisX = PADDING;
                g.DrawLine(axisPen, zAxisX, PADDING, zAxisX, PADDING + drawHeight);

                // Підписи осей
                g.DrawString($"x={X_MIN}", textFont, textBrush, PADDING, xAxisY + 5);
                g.DrawString($"x={X_MAX}", textFont, textBrush, PADDING + drawWidth, xAxisY + 5, sfRight);

                g.DrawString($"z={zMax:F4}", textFont, textBrush, PADDING - 5, PADDING, sfRight);
                g.DrawString($"z={zMin:F4}", textFont, textBrush, PADDING - 5, xAxisY - textFont.Height, sfRight);
            }

            // === 5. Перетворення математичних координат в екранні та малювання ===
            List<PointF> screenPoints = new List<PointF>();
            
            // Якщо zMin і zMax однакові (горизонтальна лінія), додамо відступ, щоб уникнути ділення на 0
            if (Math.Abs(zMax - zMin) < 1e-6)
            {
                zMax += 1.0;
                zMin -= 1.0;
            }

            foreach (PointF p in mathPoints)
            {
                // p.X - це 'x', p.Y - це 'z'

                // Перетворення X в екранну координату:
                // ( (p.X - X_MIN) / (X_MAX - X_MIN) ) -> дає нам % положення по осі X (від 0 до 1)
                // множимо на drawWidth і додаємо відступ
                float sx = PADDING + (float)((p.X - X_MIN) / (X_MAX - X_MIN)) * drawWidth;

                // Перетворення Z в екранну координату:
                // ( (p.Y - zMin) / (zMax - zMin) ) -> дає % положення по осі Z (від 0 до 1)
                // ВАЖЛИВО: екранна вісь Y йде "згори-вниз", а математична "знизу-вгору",
                // тому ми віднімаємо отримане значення від нижньої межі.
                float sy = (PADDING + drawHeight) - (float)((p.Y - zMin) / (zMax - zMin)) * drawHeight;

                screenPoints.Add(new PointF(sx, sy));
            }

            // Малюємо сам графік
            if (screenPoints.Count > 1)
            {
                using (Pen graphPen = new Pen(Color.Blue, 2))
                {
                    g.DrawLines(graphPen, screenPoints.ToArray());
                }
            }
        }
    }

    /// <summary>
    /// Клас запуску програми.
    /// </summary>
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Запускаємо наш кастомний клас форми
            Application.Run(new GraphForm());
        }
    }
}
