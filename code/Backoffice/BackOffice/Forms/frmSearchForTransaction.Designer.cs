namespace BackOffice
{
    partial class frmSearchForTransaction
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbDateFrom = new System.Windows.Forms.ComboBox();
            this.cmbMonthFrom = new System.Windows.Forms.ComboBox();
            this.cmbYearFrom = new System.Windows.Forms.ComboBox();
            this.cmbYearTo = new System.Windows.Forms.ComboBox();
            this.cmbMonthTo = new System.Windows.Forms.ComboBox();
            this.cmbDayTo = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Franklin Gothic Medium", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(8, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(416, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Enter the details of the transaction that you want to search for:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Franklin Gothic Medium", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(8, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(147, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "Transaction between:";
            // 
            // cmbDateFrom
            // 
            this.cmbDateFrom.FormattingEnabled = true;
            this.cmbDateFrom.Location = new System.Drawing.Point(12, 73);
            this.cmbDateFrom.Name = "cmbDateFrom";
            this.cmbDateFrom.Size = new System.Drawing.Size(121, 21);
            this.cmbDateFrom.TabIndex = 2;
            // 
            // cmbMonthFrom
            // 
            this.cmbMonthFrom.FormattingEnabled = true;
            this.cmbMonthFrom.Location = new System.Drawing.Point(139, 73);
            this.cmbMonthFrom.Name = "cmbMonthFrom";
            this.cmbMonthFrom.Size = new System.Drawing.Size(121, 21);
            this.cmbMonthFrom.TabIndex = 3;
            // 
            // cmbYearFrom
            // 
            this.cmbYearFrom.FormattingEnabled = true;
            this.cmbYearFrom.Location = new System.Drawing.Point(266, 73);
            this.cmbYearFrom.Name = "cmbYearFrom";
            this.cmbYearFrom.Size = new System.Drawing.Size(121, 21);
            this.cmbYearFrom.TabIndex = 4;
            // 
            // cmbYearTo
            // 
            this.cmbYearTo.FormattingEnabled = true;
            this.cmbYearTo.Location = new System.Drawing.Point(266, 133);
            this.cmbYearTo.Name = "cmbYearTo";
            this.cmbYearTo.Size = new System.Drawing.Size(121, 21);
            this.cmbYearTo.TabIndex = 8;
            // 
            // cmbMonthTo
            // 
            this.cmbMonthTo.FormattingEnabled = true;
            this.cmbMonthTo.Location = new System.Drawing.Point(139, 133);
            this.cmbMonthTo.Name = "cmbMonthTo";
            this.cmbMonthTo.Size = new System.Drawing.Size(121, 21);
            this.cmbMonthTo.TabIndex = 7;
            // 
            // cmbDayTo
            // 
            this.cmbDayTo.FormattingEnabled = true;
            this.cmbDayTo.Location = new System.Drawing.Point(12, 133);
            this.cmbDayTo.Name = "cmbDayTo";
            this.cmbDayTo.Size = new System.Drawing.Size(121, 21);
            this.cmbDayTo.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Franklin Gothic Medium", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(8, 110);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "and:";
            // 
            // frmSearchForTransaction
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 488);
            this.Controls.Add(this.cmbYearTo);
            this.Controls.Add(this.cmbMonthTo);
            this.Controls.Add(this.cmbDayTo);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbYearFrom);
            this.Controls.Add(this.cmbMonthFrom);
            this.Controls.Add(this.cmbDateFrom);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmSearchForTransaction";
            this.Text = "frmSearchForTransaction";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbDateFrom;
        private System.Windows.Forms.ComboBox cmbMonthFrom;
        private System.Windows.Forms.ComboBox cmbYearFrom;
        private System.Windows.Forms.ComboBox cmbYearTo;
        private System.Windows.Forms.ComboBox cmbMonthTo;
        private System.Windows.Forms.ComboBox cmbDayTo;
        private System.Windows.Forms.Label label3;
    }
}