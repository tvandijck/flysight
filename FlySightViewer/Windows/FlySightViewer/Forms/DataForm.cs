using System.Windows.Forms;
using FlySightViewer.WinFormsUI.Docking;
using System;
using System.ComponentModel;

namespace FlySightViewer.Forms
{
    public partial class DataForm : DockContent
    {
        private Range mSelectRange = Range.Invalid;
        private int mSuspendRangeEvent = 0;

        public event EventHandler RowsDeleted;
        public event EventHandler SelectRangeChanged;

        public DataForm()
        {
            InitializeComponent();
        }

        public LogEntry SelectedEntry
        {
            set
            {
                if (value != null)
                {
                    mSuspendRangeEvent++;
                    mRawData.DataSource = new BindingList<Record>(value.Records);
                    mRawData.Columns[0].DefaultCellStyle.Format = "hh:mm:ss.ff";
                    mRawData.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
                    mSuspendRangeEvent--;
                }
                else
                {
                    mRawData.DataSource = null;
                }
            }
        }

        public Range SelectRange
        {
            get { return mSelectRange; }
            set
            {
                Range range;
                if (value.Width > 10)
                {
                    range.Min = Math.Max(0, value.Min);
                    range.Max = Math.Min(mRawData.RowCount, value.Max);
                }
                else
                {
                    range = Range.Invalid;
                }

                if (range != mSelectRange)
                {
                    mRawData.SuspendLayout();
                    mSuspendRangeEvent++;
                    for (int i = mSelectRange.Min; i < mSelectRange.Max; ++i)
                    {
                        mRawData.Rows[i].Selected = range.IsInRange(i);
                    }
                    for (int i = range.Min; i < range.Max; ++i)
                    {
                        mRawData.Rows[i].Selected = true;
                    }
                    mSuspendRangeEvent--;
                    mRawData.ResumeLayout();

                    mSelectRange = range;
                }
            }
        }

        private void mRawData_RowsDeleted(object sender, EventArgs e)
        {
            if (RowsDeleted != null)
            {
                mSelectRange = Range.Invalid;
                RowsDeleted(this, EventArgs.Empty);
            }
        }

        private void mRawData_SelectionChanged(object sender, EventArgs e)
        {
            if (mSuspendRangeEvent == 0)
            {
                mSuspendRangeEvent++;

                int min = -1;
                int max = -1;
                if (mRawData.SelectedRows.Count > 0)
                {
                    min = mRawData.SelectedRows[0].Index;
                    max = mRawData.SelectedRows[0].Index;

                    foreach (DataGridViewBand row in mRawData.SelectedRows)
                    {
                        min = Math.Min(min, row.Index);
                        max = Math.Max(max, row.Index);
                    }

                    mSelectRange = new Range(min, max);
                    for (int i = mSelectRange.Min; i < mSelectRange.Max; ++i)
                    {
                        mRawData.Rows[i].Selected = true;
                    }
                }

                if (SelectRangeChanged != null)
                {
                    SelectRangeChanged(this, EventArgs.Empty);
                }

                mSuspendRangeEvent--;
            }
        }
    }
}
