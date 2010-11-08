using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

namespace FlySightViewer.Controls
{
    public partial class Graph : UserControl
    {
        public enum DisplayMode
        {
            HorizontalVelocity,
            VerticalVelocity,
            GlideRatio,
            Altitude,
        }

        public enum Units
        {
            Metric,
            Imperial
        }

        private enum DragMode
        {
            None,
            SelectRange,
            GraphDrag
        }

        private struct Value
        {
            public float X;
            public float Y;
        }

        private const float FeetPerMeter = 3.2808399f;
        private const float MeterPerSecondToMilesPerHour = 2.23693629f;
        private const float MeterPerSecondToKilometerPerHour = 3.6f;

        private static Pen mPen = new Pen(Color.Purple, 2.0f);
        private static Brush mPurpleBrush = new SolidBrush(Color.Purple);
        private static Brush mBrush = new SolidBrush(Color.FromArgb(128, 80, 80, 192));
        private static Font mFont = new Font("Arial", 8, FontStyle.Regular, GraphicsUnit.Pixel);

        private LogEntry mEntry;
        private DisplayMode mMode = DisplayMode.Altitude;
        private Units mUnits = Units.Metric;
        private Value[] mValues;

        private float mMinValueY;
        private float mMaxValueY;
        private float mMinValueX;
        private float mMaxValueX;

        private float mMinZoomValueY;
        private float mMaxZoomValueY;
        private float mZoomStepY;

        private float mMinZoomValueX;
        private float mMaxZoomValueX;
        private float mZoomStepX;

        private int mMouseDownX;
        private int mMouseDownY;
        private Rectangle mGraphRect;
        private Rectangle mSelectRect;
        private Brush mBackground;
        private DragMode mDragMode = DragMode.None;

        private Value mCurrentValue;
        private bool mDisplayValue = false;

        public Graph()
        {
            InitializeComponent();
            DoubleBuffered = true;
            UpdateGradient();
        }

        #region -- Properties -------------------------------------------------

        public LogEntry LogEntry
        {
            get { return mEntry; }
            set
            {
                if (!object.ReferenceEquals(mEntry, value))
                {
                    mEntry = value;
                    Setup(true);
                }
            }
        }

        public DisplayMode Mode
        {
            get { return mMode; }
            set
            {
                if (!object.ReferenceEquals(mMode, value))
                {
                    mMode = value;
                    Setup(false);
                    Invalidate();
                }
            }
        }

        public Units Unit
        {
            get { return mUnits; }
            set
            {
                if (!object.ReferenceEquals(mUnits, value))
                {
                    mUnits = value;
                    Setup(false);
                }
            }
        }

        #endregion

        #region -- Rendering methods ------------------------------------------

        public static void DrawGridY(Graphics aG, Rectangle aRect, float aMin, float aMax, float aStep)
        {
            float dy = aRect.Height / (aMax - aMin);

            float fheight = mFont.Height;
            int minI = (int)(aMin / aStep) - 1;
            int maxI = (int)(aMax / aStep) + 1;
            for (int i = minI; i < maxI; ++i)
            {
                float value = i * aStep;
                float y = aRect.Bottom - ((value - aMin) * dy);
                aG.DrawLine(Pens.Gray, aRect.Left, y, aRect.Right, y);

                if (dy * aStep > 40)
                {
                    for (int j = 1; j < 5; ++j)
                    {
                        float v = value + j * aStep * 0.2f;
                        float l = aRect.Bottom - ((v - aMin) * dy);
                        aG.DrawLine(Pens.Silver, aRect.Left, l, aRect.Right, l);
                    }
                }

                aG.DrawString(value.ToString(), mFont, Brushes.Black, aRect.Left + 1, y - fheight);
            }
        }

        public static void DrawGridX(Graphics aG, Rectangle aRect, float aMin, float aMax, float aStep)
        {
            float dx = aRect.Width / (aMax - aMin);

            float fheight = mFont.Height;
            int minI = (int)(aMin / aStep) - 1;
            int maxI = (int)(aMax / aStep) + 1;
            for (int i = minI; i < maxI; ++i)
            {
                float value = i * aStep;
                float x = aRect.Left + ((value - aMin) * dx);
                aG.DrawLine(Pens.Gray, x, aRect.Top, x, aRect.Bottom);

                if (dx * aStep > 40)
                {
                    for (int j = 1; j < 5; ++j)
                    {
                        float v = value + j * aStep * 0.2f;
                        float l = aRect.Left + ((v - aMin) * dx);
                        aG.DrawLine(Pens.Silver, l, aRect.Top, l, aRect.Bottom);
                    }
                }

                TimeSpan span = new TimeSpan((long)(value * 10000000.0));
                string time = string.Format("{0}:{1:D2}.{2:D2}", span.Minutes, span.Seconds, span.Milliseconds / 10);
                float width = aG.MeasureString(time, mFont).Width;

                aG.TranslateTransform(x + 4, aRect.Bottom);
                aG.RotateTransform(45);
                aG.DrawString(time, mFont, Brushes.Black, 0, 0);
                aG.ResetTransform();
            }
        }

        private void DrawGraph(Graphics aG, Rectangle aRect, float aMinX, float aMaxX, float aMinY, float aMaxY)
        {
            Region reg = aG.Clip;
            aG.Clip = new Region(aRect);

            PointF[] points = new PointF[aRect.Width];

            float dy = aRect.Height / (aMaxY - aMinY);
            float dx = aRect.Width / (aMaxX - aMinX);

            for (int i = 0; i < aRect.Width; ++i)
            {
                float time = (i / dx) + aMinX;
                float value = GetValueAt(time);

                float y = aRect.Bottom - ((value - aMinY) * dy);

                points[i] = new PointF(aRect.Left + i, y);
            }

            aG.DrawLines(mPen, points);

            if (mDisplayValue)
            {
                float x = aRect.Left + ((mCurrentValue.X - aMinX) * dx);
                float y = aRect.Bottom - ((mCurrentValue.Y - aMinY) * dy);
                aG.FillEllipse(mPurpleBrush, x - 3, y - 3, 6, 6);

                string text = string.Empty;
                float value = mCurrentValue.Y;
                switch (mMode)
                {
                    case DisplayMode.Altitude:
                        text = (value * 1000.0f).ToString("F0") + (mUnits == Units.Imperial ? "ft" : "m");
                        break;
                    case DisplayMode.GlideRatio:
                        text = value.ToString("F2");
                        break;
                    case DisplayMode.HorizontalVelocity:
                        text = value.ToString("F1") + (mUnits == Units.Imperial ? "MPH" : "Km/h");
                        break;
                    case DisplayMode.VerticalVelocity:
                        text = value.ToString("F1") + (mUnits == Units.Imperial ? "MPH" : "Km/h");
                        break;
                }

                SizeF size = aG.MeasureString(text, mFont);
                RectangleF rect = new RectangleF(aRect.Right - (size.Width + 3), aRect.Top + 3, size.Width, size.Height);
                aG.FillRectangle(mBackground, rect);
                aG.DrawString(text, mFont, Brushes.Black, rect);
            }

            aG.Clip = reg;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (mValues != null)
            {
                // draw background.
                g.FillRectangle(mBackground, mGraphRect);

                // draw grid.
                Region reg = g.Clip;
                g.Clip = new Region(new Rectangle(mGraphRect.Left, 0, mGraphRect.Width + 1, Height));
                DrawGridX(g, mGraphRect, mMinZoomValueX, mMaxZoomValueX, mZoomStepX);
                g.Clip = new Region(new Rectangle(0, mGraphRect.Top, Width, mGraphRect.Height + 1));
                DrawGridY(g, mGraphRect, mMinZoomValueY, mMaxZoomValueY, mZoomStepY);
                g.Clip = reg;

                g.DrawLine(Pens.Black, mGraphRect.Left, mGraphRect.Top, mGraphRect.Left, mGraphRect.Bottom);
                g.DrawLine(Pens.Black, mGraphRect.Left, mGraphRect.Bottom, mGraphRect.Right, mGraphRect.Bottom);

                // draw graph using smooth quality.
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                DrawGraph(g, mGraphRect, mMinZoomValueX, mMaxZoomValueX, mMinZoomValueY, mMaxZoomValueY);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

                if (mDragMode == DragMode.SelectRange)
                {
                    g.FillRectangle(mBrush, mSelectRect);
                }
            }
        }

        #endregion

        #region -- Event handling ---------------------------------------------

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (mGraphRect.Contains(new Point(e.X, e.Y)))
            {
                mMouseDownX = e.X;
                mMouseDownY = e.Y;
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    mSelectRect = new Rectangle(e.X, e.Y, 0, 0);
                    mDragMode = DragMode.SelectRange;
                }
                else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    mDragMode = DragMode.GraphDrag;
                }
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            mDisplayValue = false;
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (mDragMode == DragMode.SelectRange)
                {
                    int x1 = Math.Min(mMouseDownX, e.X);
                    int x2 = Math.Max(mMouseDownX, e.X);
                    int y1 = Math.Min(mMouseDownY, e.Y);
                    int y2 = Math.Max(mMouseDownY, e.Y);
                    mSelectRect = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                    Invalidate();
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (mDragMode == DragMode.GraphDrag)
                {
                    if (e.Y != mMouseDownY)
                    {
                        float dx = mGraphRect.Width / (mMaxZoomValueX - mMinZoomValueX);
                        float dy = mGraphRect.Height / (mMaxZoomValueY - mMinZoomValueY);

                        float diffX = (mMouseDownX - e.X) / dx;
                        float diffY = (e.Y - mMouseDownY) / dy;

                        bool changed = false;

                        if (mMaxZoomValueX + diffX < mMaxValueX && mMinZoomValueX + diffX > mMinValueX)
                        {
                            mMaxZoomValueX += diffX;
                            mMinZoomValueX += diffX;
                            changed = true;
                        }

                        if (mMaxZoomValueY + diffY < mMaxValueY && mMinZoomValueY + diffY > mMinValueY)
                        {
                            mMaxZoomValueY += diffY;
                            mMinZoomValueY += diffY;
                            changed = true;
                        }

                        if (changed)
                        {
                            Invalidate();
                        }

                        mMouseDownX = e.X;
                        mMouseDownY = e.Y;
                    }
                }
            }
            else if (mGraphRect.Contains(e.X, e.Y) && mValues != null)
            {
                float time = GetValueX(e.X);
                mCurrentValue.X = time;
                mCurrentValue.Y = GetValueAt(time);
                mDisplayValue = true;
                Invalidate();
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (mDragMode == DragMode.SelectRange)
            {
                int area = mSelectRect.Width * mSelectRect.Height;
                if (area > 10)
                {
                    float minX = GetValueX(mSelectRect.Left);
                    float maxX = GetValueX(mSelectRect.Right);
                    float minY = GetValueY(mSelectRect.Top);
                    float maxY = GetValueY(mSelectRect.Bottom);
                    SetZoom(minX, minY, maxX, maxY);
                }
            }

            mDragMode = DragMode.None;
            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                float diff;
                if (e.Delta > 0)
                {
                    diff = e.Delta / 140.0f;
                }
                else
                {
                    diff = 140.0f / -e.Delta;
                }

                float centerY = GetValueY(e.Y);
                float minY = ((mMinZoomValueY - centerY) * diff) + centerY;
                float maxY = ((mMaxZoomValueY - centerY) * diff) + centerY;

                float centerX = GetValueX(e.X);
                float minX = ((mMinZoomValueX - centerX) * diff) + centerX;
                float maxX = ((mMaxZoomValueX - centerX) * diff) + centerX;

                SetZoom(minX, minY, maxX, maxY);
            }
            base.OnMouseWheel(e);
        }

        protected override void OnResize(EventArgs e)
        {
            UpdateGradient();
            Invalidate();
            base.OnResize(e);
        }

        #endregion

        private void SetZoom(float minX, float minY, float maxX, float maxY)
        {
            minX = Clamp(minX, mMinValueX, mMaxValueX);
            maxX = Clamp(maxX, mMinValueX, mMaxValueX);
            minY = Clamp(minY, mMinValueY, mMaxValueY);
            maxY = Clamp(maxY, mMinValueY, mMaxValueY);

            mMinZoomValueX = Math.Min(minX, maxX);
            mMaxZoomValueX = Math.Max(minX, maxX);
            mMinZoomValueY = Math.Min(minY, maxY);
            mMaxZoomValueY = Math.Max(minY, maxY);

            UpdateZoomStep();
            Invalidate();
        }

        private float GetValueX(float aX)
        {
            float dx = mGraphRect.Width / (mMaxZoomValueX - mMinZoomValueX);
            return ((aX - mGraphRect.Left) / dx) + mMinZoomValueX;
        }

        private float GetValueY(float aY)
        {
            float dy = mGraphRect.Height / (mMaxZoomValueY - mMinZoomValueY);
            return ((mGraphRect.Bottom - aY) / dy) + mMinZoomValueY;
        }

        private float GetValueAt(float aTime)
        {
            int low = 0;
            int high = mValues.Length;
            while (low <= high)
            {
                int middle = low + (high - low) / 2;
                if (aTime < mValues[middle].X)
                {
                    high = middle - 1;
                }
                else if (aTime > mValues[middle].X)
                {
                    low = middle + 1;
                }
                else
                {
                    return mValues[middle].Y;
                }
            }

            float t = (aTime - mValues[high].X) / (mValues[low].X - mValues[high].X);
            return ((mValues[low].Y - mValues[high].Y) * t) + mValues[high].Y;
        }

        private int GetIndexAt(float aTime)
        {
            int low = 0;
            int high = mValues.Length;
            while (low <= high)
            {
                int middle = low + (high - low) / 2;
                if (aTime < mValues[middle].X)
                {
                    high = middle - 1;
                }
                else if (aTime > mValues[middle].X)
                {
                    low = middle + 1;
                }
                else
                {
                    return middle;
                }
            }
            return high;
        }

        private void UpdateGradient()
        {
            mGraphRect = new Rectangle(5, 0, Width - 5, Height - 30);
            mBackground = new LinearGradientBrush(ClientRectangle, Color.White, Color.Lavender, 90.0f);
        }

        private void UpdateZoomStep()
        {
            float dx = (mMaxZoomValueX - mMinZoomValueX) / mGraphRect.Width;
            mZoomStepX = ((int)(dx * 5000)) / 100.0f;

            int log = (int)Math.Log10((mMaxZoomValueY - mMinZoomValueY) * 0.5f);
            mZoomStepY = (float)Math.Pow(10, log);
        }

        private static float Clamp(float aValue, float aMin, float aMax)
        {
            return Math.Min(aMax, Math.Max(aValue, aMin));
        }

        #region -- Calculations -----------------------------------------------

        private void CalcValues(bool aResetZoom)
        {
            int idx = 0;

            DateTime startTime = mEntry.DateTime;
            if (mUnits == Units.Metric)
            {
                if (mMode == DisplayMode.HorizontalVelocity)
                {
                    foreach (Record rec in mEntry.Records)
                    {
                        float mps = (float)Math.Sqrt(rec.VelocityEast * rec.VelocityEast + rec.VelocityNorth * rec.VelocityNorth);
                        mValues[idx].X = (float)(rec.Time - startTime).TotalSeconds;
                        mValues[idx].Y = mps * MeterPerSecondToKilometerPerHour;
                        idx++;
                    }
                }
                else if (mMode == DisplayMode.VerticalVelocity)
                {
                    foreach (Record rec in mEntry.Records)
                    {
                        mValues[idx].X = (float)(rec.Time - startTime).TotalSeconds;
                        mValues[idx].Y = rec.VelocityDown * MeterPerSecondToKilometerPerHour;
                        idx++;
                    }
                }
                else if (mMode == DisplayMode.GlideRatio)
                {
                    foreach (Record rec in mEntry.Records)
                    {
                        float mps = (float)Math.Sqrt(rec.VelocityEast * rec.VelocityEast + rec.VelocityNorth * rec.VelocityNorth);
                        float velh = mps * MeterPerSecondToKilometerPerHour;
                        float velv = rec.VelocityDown * MeterPerSecondToKilometerPerHour;
                        mValues[idx].X = (float)(rec.Time - startTime).TotalSeconds;
                        mValues[idx].Y = (velv != 0) ? (velh / velv) : 0;
                        idx++;
                    }
                }
                else if (mMode == DisplayMode.Altitude)
                {
                    foreach (Record rec in mEntry.Records)
                    {
                        mValues[idx].X = (float)(rec.Time - startTime).TotalSeconds;
                        mValues[idx].Y = rec.Altitude / 1000.0f;
                        idx++;
                    }
                }
            }
            else
            {
                if (mMode == DisplayMode.HorizontalVelocity)
                {
                    foreach (Record rec in mEntry.Records)
                    {
                        float mps = (float)Math.Sqrt(rec.VelocityEast * rec.VelocityEast + rec.VelocityNorth * rec.VelocityNorth);
                        mValues[idx].X = (float)(rec.Time - startTime).TotalSeconds;
                        mValues[idx].Y = mps * MeterPerSecondToMilesPerHour;
                        idx++;
                    }
                }
                else if (mMode == DisplayMode.VerticalVelocity)
                {
                    foreach (Record rec in mEntry.Records)
                    {
                        mValues[idx].X = (float)(rec.Time - startTime).TotalSeconds;
                        mValues[idx].Y = rec.VelocityDown * MeterPerSecondToMilesPerHour;
                        idx++;
                    }
                }
                else if (mMode == DisplayMode.GlideRatio)
                {
                    foreach (Record rec in mEntry.Records)
                    {
                        float mps = (float)Math.Sqrt(rec.VelocityEast * rec.VelocityEast + rec.VelocityNorth * rec.VelocityNorth);
                        float velh = mps * MeterPerSecondToMilesPerHour;
                        float velv = rec.VelocityDown * MeterPerSecondToMilesPerHour;
                        mValues[idx].X = (float)(rec.Time - startTime).TotalSeconds;
                        mValues[idx].Y = (velv != 0) ? (velh / velv) : 0;
                        idx++;
                    }
                }
                else if (mMode == DisplayMode.Altitude)
                {
                    foreach (Record rec in mEntry.Records)
                    {
                        mValues[idx].X = (float)(rec.Time - startTime).TotalSeconds;
                        mValues[idx].Y = (rec.Altitude * FeetPerMeter) / 1000.0f;
                        idx++;
                    }
                }
            }

            LowPass();
            LowPass();
            LowPass();
            CalcMinMax(aResetZoom);
        }

        private void LowPass()
        {
            int num = mValues.Length;
            float[] values = new float[num];
            for (int i = 1; i < num - 1; i++)
            {
                values[i] = (mValues[i - 1].Y + (mValues[i].Y * 2) + mValues[i + 1].Y) / 4;
            }
            for (int i = 1; i < num - 1; i++)
            {
                mValues[i].Y = values[i];
            }
        }

        private void CalcMinMax(bool aResetZoom)
        {
            // calculate min/max.
            float minValue = float.MaxValue;
            float maxValue = float.MinValue;
            foreach (Value value in mValues)
            {
                minValue = Math.Min(minValue, value.Y);
                maxValue = Math.Max(maxValue, value.Y);
            }
            mMinValueY = minValue * 1.05f;
            mMaxValueY = maxValue * 1.05f;

            // calculate min/max time.
            mMinValueX = 0.0f;
            mMaxValueX = (float)mEntry.Duration.TotalSeconds;

            // copy into zoom values.
            if (aResetZoom)
            {
                mMinZoomValueX = mMinValueX;
                mMaxZoomValueX = mMaxValueX;
            }

            // rezoom on the selected area.
            int minI = GetIndexAt(mMinZoomValueX);
            int maxI = GetIndexAt(mMaxZoomValueX);
            
            minValue = float.MaxValue;
            maxValue = float.MinValue;
            for (int i=minI; i<maxI; ++i)
            {
                minValue = Math.Min(minValue, mValues[i].Y);
                maxValue = Math.Max(maxValue, mValues[i].Y);
            }
            mMinZoomValueY = minValue * 1.05f;
            mMaxZoomValueY = maxValue * 1.05f;

            UpdateZoomStep();
        }

        private void Setup(bool aResetZoom)
        {
            if (mEntry != null && mEntry.Records.Count > 0)
            {
                mValues = new Value[mEntry.Records.Count];
                CalcValues(aResetZoom);
            }
            else
            {
                mValues = null;
            }
            mDisplayValue = false;
            Invalidate();
        }

        #endregion
    }
}
