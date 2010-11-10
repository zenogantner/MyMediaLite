using System.ComponentModel;

namespace MyMediaLite.rating_demo
{
    partial class frmDemo
    {
        /// <summary>
        /// Required designer variable
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbItems = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.cmdRate = new System.Windows.Forms.Button();
            this.lblPrediction = new System.Windows.Forms.Label();
            this.lbFeedback = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbItems
            // 
            this.lbItems.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.lbItems.FormattingEnabled = true;
            this.lbItems.ItemHeight = 20;
            this.lbItems.Location = new System.Drawing.Point(14, 30);
            this.lbItems.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lbItems.Name = "lbItems";
            this.lbItems.Size = new System.Drawing.Size(429, 384);
            this.lbItems.TabIndex = 0;
            this.lbItems.SelectedIndexChanged += new System.EventHandler(this.lbItems_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 5);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(281, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Personalized TopN-Recommendations";
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "5 stars",
            "4 stars",
            "3 stars",
            "2 stars",
            "1 star"});
            this.comboBox1.Location = new System.Drawing.Point(16, 433);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(80, 28);
            this.comboBox1.TabIndex = 2;
            // 
            // cmdRate
            // 
            this.cmdRate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdRate.Location = new System.Drawing.Point(100, 434);
            this.cmdRate.Margin = new System.Windows.Forms.Padding(2);
            this.cmdRate.Name = "cmdRate";
            this.cmdRate.Size = new System.Drawing.Size(100, 25);
            this.cmdRate.TabIndex = 3;
            this.cmdRate.Text = "Rate";
            this.cmdRate.UseVisualStyleBackColor = true;
            this.cmdRate.Click += new System.EventHandler(this.cmdRate_Click);
            // 
            // lblPrediction
            // 
            this.lblPrediction.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblPrediction.Location = new System.Drawing.Point(204, 434);
            this.lblPrediction.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblPrediction.Name = "lblPrediction";
            this.lblPrediction.Size = new System.Drawing.Size(239, 22);
            this.lblPrediction.TabIndex = 4;
            this.lblPrediction.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lbFeedback
            // 
            this.lbFeedback.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lbFeedback.FormattingEnabled = true;
            this.lbFeedback.ItemHeight = 20;
            this.lbFeedback.Location = new System.Drawing.Point(450, 30);
            this.lbFeedback.Name = "lbFeedback";
            this.lbFeedback.Size = new System.Drawing.Size(366, 384);
            this.lbFeedback.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(446, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 20);
            this.label2.TabIndex = 6;
            this.label2.Text = "Your feedback";
            // 
            // frmDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(828, 467);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lbFeedback);
            this.Controls.Add(this.lblPrediction);
            this.Controls.Add(this.cmdRate);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbItems);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "frmDemo";
            this.Text = "Demo Recommender";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lbItems;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button cmdRate;
        private System.Windows.Forms.Label lblPrediction;
        private System.Windows.Forms.ListBox lbFeedback;
        private System.Windows.Forms.Label label2;
    }
}