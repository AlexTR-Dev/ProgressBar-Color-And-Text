using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ProgressBar_Color_And_Text
{

    #region enumeration types
    public enum ProgressBarGradient
    {
        Horizontal,
        Vertical,
        ForwardDiagonal,
        BackwardDiagonal
    }
    public enum ProgressBarMode
    {
        Percentage,
        Progress,
        Text,
        TextAndPercentage,
        TextAndProgress,
        NoText,
    }
    #endregion

    public partial class ProgressBarColorAndText: ProgressBar
    {
        public ProgressBarColorAndText()
        {
            InitializeComponent();
            FixComponentBlinking();
            Style = ProgressBarStyle.Marquee; // no aplica el marquee pero ayuda a una visualización mas fluida
        }
        private const string CategoryName = "Appearance ProgressBar";
        #region Progress Bar Style

        [Category(CategoryName), Browsable(true)]
        public ProgressBarMode VisualMode
        {
            get => _visualMode;
            set
            {
                _visualMode = value;
                Invalidate();
            }
        }
        private ProgressBarMode _visualMode = ProgressBarMode.Percentage;

        [Category(CategoryName), Browsable(true)]
        public ProgressBarGradient GradientMode
        {
            get => gradientMode;
            set
            {
                gradientMode = value;
                Invalidate();
            }
        }
        private ProgressBarGradient gradientMode = ProgressBarGradient.Vertical;


        [Description("The Border Color of the gradient in the Progress Bar"), Category(CategoryName), Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public Color BarColor1
        {
            get => barColor1;
            set
            {
                barColor1 = value;
                Invalidate(); ;
            }
        }
        private Color barColor1 = Color.LightCyan;

        [Description("The Center Color of the gradient in the Progress Bar"), Category(CategoryName), Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public Color BarColor2
        {
            get { return barColor2; }
            set
            {
                barColor2 = value;
                Invalidate(); ;
            }
        }
        private Color barColor2 = Color.MediumSeaGreen;

        [Category(CategoryName), Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public override Color BackColor
        {
            get => backColor;
            set
            {
                if (value != Color.Transparent)
                {
                    backColor = base.BackColor = value;
                    Invalidate();
                }
                else
                {
                    MessageBox.Show("El Color seleccionado no es válido", "Ventana de Propiedades", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

            }
        }
        private Color backColor = Color.White;
        private int thickness = 0;

        [Description("Separation from the edges"), Category(CategoryName), Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public int Thickness
        {
            get => thickness;
            set
            {
                thickness = value;
                Invalidate();
            }
        }
        #endregion

        #region Text

        [Category(CategoryName), Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public override string Text
        {
            get => text;
            set
            {
                text = value;
                Invalidate();//redraw component after change value from VS Properties section
            }
        }
        private string text = string.Empty;

        private string GetTextToDraw()
        {
            switch (VisualMode)
            {
                case ProgressBarMode.Percentage:
                    return PercentageStr;
                case ProgressBarMode.Progress:
                    return CurrProgressStr;
                case ProgressBarMode.Text:
                    return text;
                case ProgressBarMode.TextAndPercentage:
                    return $"{text} {PercentageStr}";
                case ProgressBarMode.TextAndProgress:
                    return $"{text} {CurrProgressStr}";
                case ProgressBarMode.NoText:
                    break;
            }
            return text;
        }
        private string PercentageStr => $"{(int)((float)Value - Minimum) / ((float)Maximum - Minimum) * 100 } %";

        private string CurrProgressStr => $"{Value}/{Maximum}";


        [Category(CategoryName), Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public override Font Font
        {
            get => font;
            set
            {
                font.Dispose();
                font = base.Font = value;
                Invalidate();
            }
        }
        private Font font = new Font(FontFamily.GenericSerif, 11, FontStyle.Bold | FontStyle.Italic);

        [Category(CategoryName), Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public override Color ForeColor
        {
            get => foreColor;
            set
            {
                foreColor = base.ForeColor = value;
                Invalidate();
            }
        }
        private Color foreColor;

        #endregion

        #region On Paint

        private void FixComponentBlinking()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            DrawProgressBar(e.Graphics);
            DrawString(e.Graphics);
        }

        protected override Size DefaultSize => new Size(200, 15);
        protected override void OnSizeChanged(EventArgs e)
        {
            if (Height < 12)
            {
                Height = 12;
            }
            else if (Width < 80)
            {
                Width = 80;
            }
            base.OnSizeChanged(e);
            Invalidate();
        }
        #endregion

        #region DrawProgressBar

        private void DrawProgressBar(Graphics graphics)
        {
            Rectangle rectangle = ClientRectangle;

            const int inset = 0;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (Bitmap bitmap = new Bitmap(Width, Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    if (ProgressBarRenderer.IsSupported)
                        ProgressBarRenderer.DrawHorizontalBar(g, rectangle);
                    // Cambia el color de Fondo
                    g.Clear(backColor);
                    rectangle.Inflate(new Size(-inset, -thickness)); //Se rellena el rectangulo con las especificaciones dadas.
                    rectangle.Width = Convert.ToInt32(value: Math.Round(rectangle.Width * (double)Value / Maximum) + 1); // se agrega 1 para compensar que se sesajusta en -1  en la posición de offscree.FillRectangle

                    if (rectangle.Width <= 0) rectangle.Width = 1;
                    if (rectangle.Height <= 0) rectangle.Height = 1;

                    using (LinearGradientBrush brush = new LinearGradientBrush(rectangle, barColor1, barColor2, linearGradientMode: (LinearGradientMode)GradientMode))
                    {
                        g.FillRectangle(brush, inset - 1, thickness, rectangle.Width, rectangle.Height); ///Set desajusta la posición x en -1 para ocultar el valor minimo recuadro.
                        graphics.DrawImage(bitmap, 0, 0);
                    }
                }
            }
        }

        private void DrawString(Graphics graphics)
        {
            if (VisualMode != ProgressBarMode.NoText)
            {
                string text = GetTextToDraw();
                SizeF size = graphics.MeasureString(text, font);
                Point point = new Point(Convert.ToInt16((Width - size.Width) / 2), Convert.ToInt16((Height - size.Height) / 2));
                using (SolidBrush brush = new SolidBrush(foreColor))
                {
                    graphics.DrawString(text, font, brush, point);
                }
            }
        }
        protected new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
