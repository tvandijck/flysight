using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FlySightViewer.Controls
{
    public class BatchedDataGridView : DataGridView
    {
        private bool mQueueDeleteEvents = false;
        private int mDeletedRows;

        public event EventHandler RowsDeleted;

        protected override bool ProcessDataGridViewKey(KeyEventArgs e)
        {
            mQueueDeleteEvents = true;
            bool result = base.ProcessDataGridViewKey(e);
            mQueueDeleteEvents = false;
            ProcessDeleteEvent();
            return result;
        }

        protected override void OnUserDeletedRow(DataGridViewRowEventArgs e)
        {
            if (e.Row != null && mQueueDeleteEvents)
            {
                mDeletedRows++;
            }
            base.OnUserDeletedRow(e);
        }

        protected override void OnSelectionChanged(EventArgs e)
        {
            if (!mQueueDeleteEvents)
            {
                base.OnSelectionChanged(e);
            }
        }

        private void ProcessDeleteEvent()
        {
            if (mDeletedRows > 0)
            {
                if (RowsDeleted != null)
                {
                    RowsDeleted(this, EventArgs.Empty);
                }

                mDeletedRows = 0;
            }
        }
    }
}
