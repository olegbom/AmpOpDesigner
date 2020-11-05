using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AmpOpDesigner.Converters;

namespace AmpOpDesigner
{
    public class Scheme: FrameworkElement
    {
        private VisualCollection _children;

        private DrawingVisual _scheme;


        public static readonly DependencyProperty SchemeSolutionProperty = DependencyProperty.Register(
            "SchemeSolution", typeof(SchemeSolution), typeof(Scheme), new PropertyMetadata(default(SchemeSolution), SchemeSolutionChangedCallback));

        private static void SchemeSolutionChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Scheme s)
            {
                s.DrawScheme();
            }
        }

        public SchemeSolution SchemeSolution
        {
            get => (SchemeSolution) GetValue(SchemeSolutionProperty);
            set => SetValue(SchemeSolutionProperty, value);
        }

        public Scheme()
        {
            _children = new VisualCollection(this);

            this.Loaded += (o, e) =>
            {
                _children.Clear();
                _scheme = new DrawingVisual();
                _children.Add(_scheme);
                DrawScheme();
            };

        }

        private Vector _penCoord = new Vector(0,0);
        

        private void ResetPen()
        {
            _penCoord = new Vector(0, 0);
        }

        private void SetPenCoord(double x, double y)
        {
            _penCoord = new Vector(x, y);
        }

        private void DrawScheme()
        {
            
            using (var context = _scheme.RenderOpen())
            {
                int gridStep = 10;
                SetPenCoord(50, 100);
                DrawPoint(context);
                DrawText(context, "+10V", AlignmentX.Center, AlignmentY.Bottom);
                DrawHLine(context, 50);

                DrawResistor(context, 5, "R4",SchemeSolution?.R4 ?? 0);

                DrawHLine(context, 40);
                DrawPoint(context);
                Vector pIn1 = _penCoord;
                DrawVLine(context, -40);
                DrawHLine(context, 40);
                DrawResistor(context, 5, "R3", SchemeSolution?.R3 ?? 0, Orientation.Horizontal);
                DrawHLine(context, 40);
                DrawVLine(context, 65);
                DrawPoint(context);

                SetPenCoord(50, 150);
                DrawPoint(context);
                DrawText(context, "Uin", AlignmentX.Center, AlignmentY.Bottom);
                DrawHLine(context, 50);
                DrawResistor(context, 5, "R5", SchemeSolution?.R5 ?? 0);
                DrawHLine(context, 40);
                DrawPoint(context);
                Vector pIn2 = _penCoord;
                DrawVLine(context, 40);
                DrawResistor(context, 5, "R6", SchemeSolution?.R6 ?? 0, Orientation.Vertical);
                DrawVLine(context, 40);
                DrawGround(context, 5);
                DrawText(context, "GND");

                _penCoord = pIn1;
                DrawHLine(context, 30);
                _penCoord.X += 5;
                DrawText(context, "-", AlignmentX.Left, AlignmentY.Center);
                _penCoord.X -= 5;
                DrawVLine(context, -15);
                DrawLine(context, 70, 40);
                Vector pOut = _penCoord;
                DrawLine(context, -70, 40);
                DrawVLine(context, -65);



                _penCoord = pIn2;
                DrawHLine(context, 30);
                _penCoord.X += 5;
                DrawText(context, "+", AlignmentX.Left, AlignmentY.Center);
                _penCoord = pOut;
                DrawHLine(context, 50);
                DrawPoint(context);
            }
        }

        private static readonly Pen SchemePen = new Pen(Brushes.Black, 1);
        private static readonly ResistorNominalConverter ResNominalConverter = new ResistorNominalConverter();
        private void DrawResistor(DrawingContext context, double scale, string designator, double value, Orientation orientation = Orientation.Horizontal)
        {
            if (orientation == Orientation.Horizontal)
            {
                context.DrawRectangle(null, SchemePen, new Rect(
                    (_penCoord + new Vector(0, -2) * scale).ToPoint(),
                    (_penCoord + new Vector(10, 2) * scale).ToPoint()));
                var oldPenCoord = _penCoord;

                _penCoord += new Vector(5, -2) * scale;
                DrawText(context, designator, AlignmentX.Center, AlignmentY.Bottom);
                _penCoord += new Vector(0, 4) * scale;

                string strNominal = ResNominalConverter.Convert(value, typeof(string), null, CultureInfo.CurrentCulture) as string;
                DrawText(context, strNominal, AlignmentX.Center, AlignmentY.Top);
                _penCoord = oldPenCoord + new Vector(10 * scale, 0);


            }
            else
            {
                context.DrawRectangle(null, SchemePen, new Rect(
                    (_penCoord + new Vector(-2, 0) * scale).ToPoint(),
                    (_penCoord + new Vector(2, 10) * scale).ToPoint()));
                var oldPenCoord = _penCoord;
                _penCoord += new Vector(3, 5) * scale;
                string strNominal = ResNominalConverter.Convert(value, typeof(string), null, CultureInfo.CurrentCulture) as string;
                DrawText(context, $"{designator}{Environment.NewLine}{strNominal}", AlignmentX.Left, AlignmentY.Center, TextAlignment.Left);
                _penCoord = oldPenCoord + new Vector(0, 10 * scale);
            }
        }

        private void DrawHLine(DrawingContext context, double length)
        {
            context.DrawLine(SchemePen, _penCoord.ToPoint(), _penCoord + new Vector(length, 0).ToPoint());
            _penCoord.X += length;
        }

        private void DrawVLine(DrawingContext context, double length)
        {
            context.DrawLine(SchemePen, _penCoord.ToPoint(), _penCoord + new Vector(0, length).ToPoint());
            _penCoord.Y += length;
        }

        private void DrawLine(DrawingContext context, double dx, double dy)
        {
            var dv = new Vector(dx, dy);
            context.DrawLine(SchemePen, _penCoord.ToPoint(), _penCoord.ToPoint() + dv);
            _penCoord += dv;
        }

        private void DrawPoint(DrawingContext context)
        {
            context.DrawEllipse(Brushes.Black, null, _penCoord.ToPoint(), 2.5,2.5);
        }

        private void DrawGround(DrawingContext context, double scale)
        {
            context.DrawLine(SchemePen, _penCoord + new Vector(-3*scale,0).ToPoint(), _penCoord + new Vector(3* scale, 0).ToPoint());
        }

        private static readonly Typeface Typeface = new Typeface("Century Gothic");
        private void DrawText(DrawingContext context, string text, 
            AlignmentX alignmentX = AlignmentX.Center, AlignmentY alignmentY = AlignmentY.Top,
            TextAlignment textAlignment = TextAlignment.Center)
        {
            var formText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                Typeface, 14, Brushes.Black, null, 96)
            {
                TextAlignment = textAlignment,
            };
            var v = _penCoord;
            switch (alignmentX)
            {
                case AlignmentX.Left: v.X += formText.Width / 2; break;
                case AlignmentX.Right: v.X -= formText.Width / 2; break;
            }

            switch (textAlignment)
            {
                case TextAlignment.Left: v.X -= formText.Width / 2; break;
                case TextAlignment.Right: v.X += formText.Width / 2; break;
            }
            switch (alignmentY)
            {
                case AlignmentY.Center: v.Y -= formText.Height / 2; break;
                case AlignmentY.Bottom: v.Y -= formText.Height; break;
            }

        
            context.DrawText(formText, v.ToPoint());
        }

        protected override int VisualChildrenCount => _children.Count;

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
                throw new ArgumentOutOfRangeException();

            return _children[index];
        }
    }
    
}
