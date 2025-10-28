using System;
using System.Collections.Generic; // Потрібен для List
using System.Drawing;
using System.Drawing.Drawing2D; // Потрібен для SmoothingMode
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    // [РЕФАКТОРИНГ] 4. Додано enum для практичного завдання
    /// <summary>
    /// Визначає стиль малювання графіка.
    /// </summary>
    public enum PlotStyle
    {
        Line,
        Points
    }

    public partial class Form1 : Form
    {
        // [РЕФАКТОРИНГ] 4. Винесення "магічних чисел" в константи/поля
        private const double X_MIN = 7.2;
        private const double X_MAX = 12;
        private const double DX = 0.01; // Крок для плавності
        private const int MARGIN = 40;   // Відступ

        // [РЕФАКТОРИНГ] 1. Кешування даних
        // Список для зберігання математичних точок (обчислюється 1 раз)
        private readonly List<PointF> _plotPoints = new List<PointF>();
        private double _zMin;
        private double _zMax;

        // [РЕФАКТОРИНГ] 4. Поле для практичного завдання (перемикання стилю)
        private PlotStyle _currentStyle = PlotStyle.Line;

        public Form1()
        {
            InitializeComponent();
            
            // [РЕФАКТОРИНГ] 4. Використання SetStyle для кращої продуктивності
            // Встановлює ті ж прапори, що і this.DoubleBuffered = true,
            // але додає ResizeRedraw, що еквівалентно підписці на Resize.
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint | // Ігнорує WM_ERASEBKGND
                ControlStyles.UserPaint |            // Малюємо самі
                ControlStyles.OptimizedDoubleBuffer | // Вмикає подвійну буферизацію
                ControlStyles.ResizeRedraw,          // Перемальовує при зміні розміру
                true);

            // [РЕФАКТОРИНГ] 1. Обчислюємо дані ОДИН РАЗ при запуску
            ComputePlotData();
        }

        // Міні-реалізація InitializeComponent
        private void InitializeComponent()
        {
            this.ClientSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Load += Form1_Load;
            this.MouseClick += Form1_MouseClick; // [РЕФАКТОРИНГ] 4. Додаємо клік
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Графік функції z = (2sin²(x + 2)) / (x² + 1) (Клацніть для зміни стилю)";
        }

        // [РЕФАКТОРИНГ] 4. Обробник для практичного завдання
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            // Перемикаємо стиль
            _currentStyle = (_currentStyle == PlotStyle.Line) 
                ? PlotStyle.Points 
                : PlotStyle.Line;
                
            // Даємо команду на перемалювання
            this.Invalidate();
        }

        /// <summary>
        /// [РЕФАКТОРИНГ] 1. Обчислює та кешує всі точки графіка.
        /// </summary>
        private void ComputePlotData()
        {
            _plotPoints.Clear();
            _zMin = double.MaxValue;
            _zMax = double.MinValue;

            for (double x = X_MIN; x <= X_MAX; x += DX)
            {
                // Розрахунок нової функції z(x)
                double sinVal = Math.Sin(x + 2);
                double z = (2 * sinVal * sinVal) / (x * x + 1);

                if (!double.IsNaN(z) && !double.IsInfinity(z))
                {
                    if (z < _zMin) _zMin = z;
                    if (z > _zMax) _zMax = z;
                    
                    _plotPoints.Add(new PointF((float)x, (float)z));
                }
            }
        }

        // [РЕФАКТОРИНГ] 4. Головний метод OnPaint тепер короткий
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Якщо вікно замале, нічого не малюємо
            if (this.ClientSize.Width < 2 * MARGIN || this.ClientSize.Height < 2 * MARGIN)
                return;

            DrawPlotArea(g);
            DrawPlot(g);
        }

        /// <summary>
        /// [РЕФАКТОРИНГ] 4. Малює осі, рамку та підписи.
        /// </summary>
        private void DrawPlotArea(Graphics g)
        {
            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;
            
            // Визначаємо робочу область
            int plotW = w - 2 * MARGIN;
            int plotH = h - 2 * MARGIN;
            Rectangle plotArea = new Rectangle(MARGIN, MARGIN, plotW, plotH);

            // [РЕФАКТОРИНГ] 2. Ресурси GDI+ обгорнуті в 'using'
            using (Pen axisPen = new Pen(Color.Black, 2))
            using (Pen gridPen = new Pen(Color.Gray, 1))
            using (Font font = new Font("Arial", 10))
            using (Brush brush = new SolidBrush(Color.Black))
            {
                // Вісь X (горизонтальна)
                g.DrawLine(axisPen, plotArea.Left, plotArea.Bottom, plotArea.Right, plotArea.Bottom);
                // Вісь Z (вертикальна)
                g.DrawLine(axisPen, plotArea.Left, plotArea.Top, plotArea.Left, plotArea.Bottom);

                // Підписи осей
                g.DrawString("X", font, brush, plotArea.Right + 10, plotArea.Bottom - 10);
                g.DrawString("Z", font, brush, plotArea.Left - 20, plotArea.Top - 20);

                // Підписи значень (тиків)
                g.DrawString(X_MIN.ToString("F1"), font, brush, plotArea.Left, plotArea.Bottom + 5);
                g.DrawString(X_MAX.ToString("F1"), font, brush, plotArea.Right - 20, plotArea.Bottom + 5);
                g.DrawString(_zMax.ToString("F4"), font, brush, plotArea.Left - MARGIN, plotArea.Top);
                g.DrawString(_zMin.ToString("F4"), font, brush, plotArea.Left - MARGIN, plotArea.Bottom - font.Height);

                // Малюємо рамку
                g.DrawRectangle(gridPen, plotArea);
            }
        }

        /// <summary>
        /// [РЕФАКТОРИНГ] 4. Малює сам графік (лінії або точки).
        /// </summary>
        private void DrawPlot(Graphics g)
        {
            int w = this.ClientSize.Width;
            int h = this.ClientSize.Height;
            
            // Визначаємо робочу область (знову, щоб не передавати купу параметрів)
            int plotW = w - 2 * MARGIN;
            int plotH = h - 2 * MARGIN;
            
            // [РЕФАКТОРИНГ] 3. Безпечне ділення
            double zRange = _zMax - _zMin;
            // Якщо графік - горизонтальна лінія, уникаємо ділення на 0
            if (Math.Abs(zRange) < 1e-9)
                zRange = 1;

            // [РЕФАКТОРИНГ] 2. Ресурси GDI+
            using (Pen graphPen = new Pen(Color.Red, 2))
            using (Brush pointBrush = new SolidBrush(Color.Blue))
            {
                PointF? prevScreenPoint = null;

                // Малюємо, використовуючи закешовані дані
                foreach (PointF mathPoint in _plotPoints)
                {
                    // Перераховуємо математичні (x, z) в екранні (px, py)
                    float px = (float)(MARGIN + (mathPoint.X - X_MIN) / (X_MAX - X_MIN) * plotW);
                    float py = (float)(h - MARGIN - (mathPoint.Y - _zMin) / zRange * plotH);
                    
                    PointF currentScreenPoint = new PointF(px, py);

                    // [РЕФАКТОРИНГ] 4. Вибір стилю малювання
                    switch (_currentStyle)
                    {
                        case PlotStyle.Line:
                            if (prevScreenPoint != null)
                                g.DrawLine(graphPen, prevScreenPoint.Value, currentScreenPoint);
                            prevScreenPoint = currentScreenPoint;
                            break;
                        
                        case PlotStyle.Points:
                            // Малюємо точку як маленький еліпс
                            g.FillEllipse(pointBrush, px - 2, py - 2, 4, 4);
                            break;
                    }
                }
            }
        }
    }

    // Клас запуску програми
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
