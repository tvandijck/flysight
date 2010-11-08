using System;
using FlySightViewer.Controls;
using FlySightViewer.WinFormsUI.Docking;

namespace FlySightViewer.Forms
{
    public partial class GraphForm : DockContent
    {
        public event EventHandler DisplayRangeChanged;

        public GraphForm()
        {
            InitializeComponent();

            mGraphMode.Items.AddRange(Enum.GetNames(typeof(Graph.DisplayMode)));
            mGraphMode.SelectedIndex = 1;
        }

        private Range mRange;
        public Range SelectRange
        {
            get { return mRange; }
            set
            {
                if (value != mRange)
                {
                    mRange = value;

                    if (DisplayRangeChanged != null)
                    {
                        DisplayRangeChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        public LogEntry SelectedEntry
        {
            set { mGraph.LogEntry = value; }
        }

        private void OnModeSelected(object sender, EventArgs e)
        {
            int idx = mGraphMode.SelectedIndex;
            Graph.DisplayMode[] values = (Graph.DisplayMode[])Enum.GetValues(typeof(Graph.DisplayMode));
            mGraph.Mode = values[idx];
            UpdateGraphMode();
        }

        private void UpdateGraphMode()
        {
            if (mGraph.Mode == Graph.DisplayMode.GlideRatio)
            {
                mImperial.Hide();
                mMetric.Hide();
            }
            else
            {
                mImperial.Show();
                mMetric.Show();
                switch (mGraph.Mode)
                {
                    case Graph.DisplayMode.HorizontalVelocity:
                    case Graph.DisplayMode.VerticalVelocity:
                        mImperial.Text = "MPH";
                        mMetric.Text = "KMPH";
                        break;
                    case Graph.DisplayMode.Altitude:
                        mImperial.Text = "ft (x1000)";
                        mMetric.Text = "KM";
                        break;
                }
            }
        }

        private void OnUnitCheckedChanged(object sender, EventArgs e)
        {
            if (mImperial.Checked)
            {
                mGraph.Unit = Graph.Units.Imperial;
            }
            else
            {
                mGraph.Unit = Graph.Units.Metric;
            }
        }
    }
}
