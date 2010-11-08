namespace FlySightViewer.Forms
{
    partial class GraphForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mImperial = new System.Windows.Forms.RadioButton();
            this.mMetric = new System.Windows.Forms.RadioButton();
            this.mGraphMode = new System.Windows.Forms.ComboBox();
            this.mGraph = new FlySightViewer.Controls.Graph();
            this.SuspendLayout();
            // 
            // mImperial
            // 
            this.mImperial.AutoSize = true;
            this.mImperial.Location = new System.Drawing.Point(217, 5);
            this.mImperial.Name = "mImperial";
            this.mImperial.Size = new System.Drawing.Size(61, 17);
            this.mImperial.TabIndex = 9;
            this.mImperial.Text = "Imperial";
            this.mImperial.UseVisualStyleBackColor = true;
            this.mImperial.CheckedChanged += new System.EventHandler(this.OnUnitCheckedChanged);
            // 
            // mMetric
            // 
            this.mMetric.AutoSize = true;
            this.mMetric.Checked = true;
            this.mMetric.Location = new System.Drawing.Point(157, 5);
            this.mMetric.Name = "mMetric";
            this.mMetric.Size = new System.Drawing.Size(54, 17);
            this.mMetric.TabIndex = 8;
            this.mMetric.TabStop = true;
            this.mMetric.Text = "Metric";
            this.mMetric.UseVisualStyleBackColor = true;
            this.mMetric.CheckedChanged += new System.EventHandler(this.OnUnitCheckedChanged);
            // 
            // mGraphMode
            // 
            this.mGraphMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mGraphMode.FormattingEnabled = true;
            this.mGraphMode.Location = new System.Drawing.Point(6, 4);
            this.mGraphMode.Name = "mGraphMode";
            this.mGraphMode.Size = new System.Drawing.Size(121, 21);
            this.mGraphMode.TabIndex = 6;
            this.mGraphMode.SelectedIndexChanged += new System.EventHandler(this.OnModeSelected);
            // 
            // mGraph
            // 
            this.mGraph.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mGraph.BackColor = System.Drawing.Color.Transparent;
            this.mGraph.Location = new System.Drawing.Point(0, 30);
            this.mGraph.LogEntry = null;
            this.mGraph.Mode = FlySightViewer.Controls.Graph.DisplayMode.Altitude;
            this.mGraph.Name = "mGraph";
            this.mGraph.Size = new System.Drawing.Size(629, 368);
            this.mGraph.TabIndex = 5;
            this.mGraph.Unit = FlySightViewer.Controls.Graph.Units.Metric;
            // 
            // GraphForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(629, 399);
            this.Controls.Add(this.mImperial);
            this.Controls.Add(this.mMetric);
            this.Controls.Add(this.mGraphMode);
            this.Controls.Add(this.mGraph);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HideOnClose = true;
            this.Name = "GraphForm";
            this.Text = "Graphs";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton mImperial;
        private System.Windows.Forms.RadioButton mMetric;
        private System.Windows.Forms.ComboBox mGraphMode;
        private FlySightViewer.Controls.Graph mGraph;
    }
}