using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            // Цей рядок каже формі перемалюватися при зміні розміру
            this.Resize += (s, e) => this.Invalidate(); 
            // Це вмикає подвійну буферизацію, щоб графік не мерехтів
            this.DoubleBuffered = true;
        }

        // Міні-реалізація InitializeComponent, щоб проект компілювався без дизайнерських файлів.
        // Налаштовує розміри форми та підписує подію Load.
        private void InitializeComponent()
        {
            this.ClientSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Load += Form1_Load;
            this.Text = "Графік функції";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // === ЗМІНА 1: Оновлено заголовок ===
            this.Text = "Графік функції z = (2sin²(x + 2)) / (x² + 1)";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;

            // === ЗМІНА 2: Оновлено константи з варіанту 17 ===
            double xMin = 7.2;
            double xMax = 12;
            // dx для плавності, як у твоєму прикладі
            double dx = 0.01; 

            
            // Шукаємо zMin та zMax, щоб правильно масштабувати графік
            // (Використовуємо 'z' замість 'y')
            double zMin = double.MaxValue;
            double zMax = double.MinValue;

            for (double x = xMin; x <= xMax; x += dx)
            {
                // === ЗМІНА 3: Розрахунок нової функції z(x) ===
                double sinVal = Math.Sin(x + 2);
                double z = (2 * sinVal * sinVal) / (x * x + 1);
                
                if (!double.IsNaN(z) && !double.IsInfinity(z))
                {
                    if (z < zMin) zMin = z;
                    if (z > zMax) zMax = z;
                }
            }

            
            int margin = 40;
            int plotW = w - 2 * margin;
            int plotH = h - 2 * margin;
            
            // Якщо вікно замале, не малюємо
            if (plotW <= 0 || plotH <= 0) return;

            
            // Малюємо осі (X - внизу, Y/Z - зліва)
            Pen axisPen = new Pen(Color.Black, 2);
            // Вісь X (горизонтальна)
            g.DrawLine(axisPen, margin, h - margin, w - margin, h - margin); 
            // Вісь Z (вертикальна)
            g.DrawLine(axisPen, margin, margin, margin, h - margin); 

            
            Pen graphPen = new Pen(Color.Red, 2);
            PointF? prev = null;

            // Другий прохід: малюємо сам графік
            for (double x = xMin; x <= xMax; x += dx)
            {
                // === ЗМІНА 4: Розрахунок нової функції z(x) (знову) ===
                double sinVal = Math.Sin(x + 2);
                double z = (2 * sinVal * sinVal) / (x * x + 1);
                
                if (double.IsNaN(z) || double.IsInfinity(z))
                {
                    prev = null; // Розриваємо лінію, якщо є розрив функції
                    continue;
                }

                // Перераховуємо математичні (x, z) в екранні (px, py)
                float px = (float)(margin + (x - xMin) / (xMax - xMin) * plotW);
                // (z - zMin) / (zMax - zMin) - це відсоток положення по осі Z (0.0 ... 1.0)
                // h - margin - ... : перевертаємо вісь Y (бо 0 на екрані - це згори)
                float py = (float)(h - margin - (z - zMin) / (zMax - zMin) * plotH);

                if (prev != null)
                    g.DrawLine(graphPen, prev.Value, new PointF(px, py));

                prev = new PointF(px, py);
            }           
            // Підписи осей
            Font font = new Font("Arial", 10);
            g.DrawString("X", font, Brushes.Black, w - margin + 10, h - margin - 10);
            // === ЗМІНА 5: 'Y' стала 'Z' ===
            g.DrawString("Z", font, Brushes.Black, margin - 20, margin - 20);

            // Малюємо рамку навколо графіка, як у твоєму прикладі
            g.DrawRectangle(Pens.Gray, margin, margin, plotW, plotH);
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
