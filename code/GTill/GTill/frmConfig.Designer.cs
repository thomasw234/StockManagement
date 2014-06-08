namespace GTill
{
    partial class frmConfig
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
            this.cbAutoLowercase = new System.Windows.Forms.CheckBox();
            this.cbUseVirtualPrinter = new System.Windows.Forms.CheckBox();
            this.cbDoBackups = new System.Windows.Forms.CheckBox();
            this.nLinesAfterRegisterReport = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.nPriceLabelOffset = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.nPrinterCharWidth = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.nLinesBetweenReceipt = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.sPrinterOutputPort = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.sBackupLocation = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.sFloppyDiscLocation = new System.Windows.Forms.TextBox();
            this.btnPresetFloppy = new System.Windows.Forms.Button();
            this.btnPresetDOS = new System.Windows.Forms.Button();
            this.btnPresetNothing = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnPresetDefault = new System.Windows.Forms.Button();
            this.cbFloppy = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this.sFontName = new System.Windows.Forms.TextBox();
            this.cSalesStats = new System.Windows.Forms.CheckBox();
            this.tbNoStats = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.nLinesAfterRegisterReport)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nPriceLabelOffset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nPrinterCharWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nLinesBetweenReceipt)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Franklin Gothic Medium", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(984, 62);
            this.label1.TabIndex = 0;
            this.label1.Text = "GTill Configuration";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // cbAutoLowercase
            // 
            this.cbAutoLowercase.AutoSize = true;
            this.cbAutoLowercase.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbAutoLowercase.Location = new System.Drawing.Point(23, 123);
            this.cbAutoLowercase.Name = "cbAutoLowercase";
            this.cbAutoLowercase.Size = new System.Drawing.Size(608, 29);
            this.cbAutoLowercase.TabIndex = 3;
            this.cbAutoLowercase.Text = "Automatically use lowercase letters on items in a transaction";
            this.cbAutoLowercase.UseVisualStyleBackColor = true;
            // 
            // cbUseVirtualPrinter
            // 
            this.cbUseVirtualPrinter.AutoSize = true;
            this.cbUseVirtualPrinter.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbUseVirtualPrinter.Location = new System.Drawing.Point(23, 263);
            this.cbUseVirtualPrinter.Name = "cbUseVirtualPrinter";
            this.cbUseVirtualPrinter.Size = new System.Drawing.Size(541, 29);
            this.cbUseVirtualPrinter.TabIndex = 7;
            this.cbUseVirtualPrinter.Text = "Use a virtual printer (instead of output to parallel port)";
            this.cbUseVirtualPrinter.UseVisualStyleBackColor = true;
            // 
            // cbDoBackups
            // 
            this.cbDoBackups.AutoSize = true;
            this.cbDoBackups.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbDoBackups.Location = new System.Drawing.Point(23, 228);
            this.cbDoBackups.Name = "cbDoBackups";
            this.cbDoBackups.Size = new System.Drawing.Size(301, 29);
            this.cbDoBackups.TabIndex = 6;
            this.cbDoBackups.Text = "Do backups to another drive";
            this.cbDoBackups.UseVisualStyleBackColor = true;
            // 
            // nLinesAfterRegisterReport
            // 
            this.nLinesAfterRegisterReport.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nLinesAfterRegisterReport.Location = new System.Drawing.Point(451, 326);
            this.nLinesAfterRegisterReport.Name = "nLinesAfterRegisterReport";
            this.nLinesAfterRegisterReport.Size = new System.Drawing.Size(120, 31);
            this.nLinesAfterRegisterReport.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(18, 328);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(427, 25);
            this.label3.TabIndex = 9;
            this.label3.Text = "Number of blank lines after a register report";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(18, 360);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(765, 25);
            this.label4.TabIndex = 10;
            this.label4.Text = "Price Label offset (greater number increases font size of items in a transaction)" +
                "";
            // 
            // nPriceLabelOffset
            // 
            this.nPriceLabelOffset.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nPriceLabelOffset.Location = new System.Drawing.Point(789, 358);
            this.nPriceLabelOffset.Name = "nPriceLabelOffset";
            this.nPriceLabelOffset.Size = new System.Drawing.Size(120, 31);
            this.nPriceLabelOffset.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(18, 398);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(510, 25);
            this.label5.TabIndex = 12;
            this.label5.Text = "Number of characters per line the printer can handle";
            // 
            // nPrinterCharWidth
            // 
            this.nPrinterCharWidth.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nPrinterCharWidth.Location = new System.Drawing.Point(534, 398);
            this.nPrinterCharWidth.Name = "nPrinterCharWidth";
            this.nPrinterCharWidth.Size = new System.Drawing.Size(120, 31);
            this.nPrinterCharWidth.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(18, 437);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(431, 25);
            this.label6.TabIndex = 14;
            this.label6.Text = "Number of blank lines between each receipt";
            // 
            // nLinesBetweenReceipt
            // 
            this.nLinesBetweenReceipt.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nLinesBetweenReceipt.Location = new System.Drawing.Point(455, 435);
            this.nLinesBetweenReceipt.Name = "nLinesBetweenReceipt";
            this.nLinesBetweenReceipt.Size = new System.Drawing.Size(120, 31);
            this.nLinesBetweenReceipt.TabIndex = 12;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(18, 475);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(371, 25);
            this.label7.TabIndex = 16;
            this.label7.Text = "Printer output port (parallel, not USB!)";
            // 
            // sPrinterOutputPort
            // 
            this.sPrinterOutputPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sPrinterOutputPort.Location = new System.Drawing.Point(395, 472);
            this.sPrinterOutputPort.Name = "sPrinterOutputPort";
            this.sPrinterOutputPort.Size = new System.Drawing.Size(100, 31);
            this.sPrinterOutputPort.TabIndex = 13;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(18, 509);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(165, 25);
            this.label8.TabIndex = 18;
            this.label8.Text = "Backup location";
            // 
            // sBackupLocation
            // 
            this.sBackupLocation.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sBackupLocation.Location = new System.Drawing.Point(189, 509);
            this.sBackupLocation.Name = "sBackupLocation";
            this.sBackupLocation.Size = new System.Drawing.Size(198, 31);
            this.sBackupLocation.TabIndex = 14;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(18, 543);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(813, 25);
            this.label9.TabIndex = 20;
            this.label9.Text = "Floppy disc drive location (Set to MS-DOS partition drive if using MS-DOS transfe" +
                "rs)";
            // 
            // sFloppyDiscLocation
            // 
            this.sFloppyDiscLocation.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sFloppyDiscLocation.Location = new System.Drawing.Point(837, 543);
            this.sFloppyDiscLocation.Name = "sFloppyDiscLocation";
            this.sFloppyDiscLocation.Size = new System.Drawing.Size(159, 31);
            this.sFloppyDiscLocation.TabIndex = 15;
            // 
            // btnPresetFloppy
            // 
            this.btnPresetFloppy.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPresetFloppy.Location = new System.Drawing.Point(12, 646);
            this.btnPresetFloppy.Name = "btnPresetFloppy";
            this.btnPresetFloppy.Size = new System.Drawing.Size(221, 37);
            this.btnPresetFloppy.TabIndex = 17;
            this.btnPresetFloppy.Text = "Floppy disc transfers";
            this.btnPresetFloppy.UseVisualStyleBackColor = true;
            this.btnPresetFloppy.Click += new System.EventHandler(this.btnPresetFloppy_Click);
            // 
            // btnPresetDOS
            // 
            this.btnPresetDOS.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPresetDOS.Location = new System.Drawing.Point(239, 646);
            this.btnPresetDOS.Name = "btnPresetDOS";
            this.btnPresetDOS.Size = new System.Drawing.Size(309, 37);
            this.btnPresetDOS.TabIndex = 18;
            this.btnPresetDOS.Text = "Reboot to MS-DOS transfers";
            this.btnPresetDOS.UseVisualStyleBackColor = true;
            this.btnPresetDOS.Click += new System.EventHandler(this.btnPresetDOS_Click);
            // 
            // btnPresetNothing
            // 
            this.btnPresetNothing.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPresetNothing.Location = new System.Drawing.Point(239, 689);
            this.btnPresetNothing.Name = "btnPresetNothing";
            this.btnPresetNothing.Size = new System.Drawing.Size(309, 37);
            this.btnPresetNothing.TabIndex = 20;
            this.btnPresetNothing.Text = "Don\'t do anything to transfer";
            this.btnPresetNothing.UseVisualStyleBackColor = true;
            this.btnPresetNothing.Click += new System.EventHandler(this.btnPresetNothing_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(12, 614);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(102, 29);
            this.label10.TabIndex = 25;
            this.label10.Text = "Presets";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(18, 54);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(108, 29);
            this.label11.TabIndex = 26;
            this.label11.Text = "Settings";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(840, 695);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 21;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(921, 695);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 22;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnPresetDefault
            // 
            this.btnPresetDefault.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPresetDefault.Location = new System.Drawing.Point(12, 689);
            this.btnPresetDefault.Name = "btnPresetDefault";
            this.btnPresetDefault.Size = new System.Drawing.Size(221, 35);
            this.btnPresetDefault.TabIndex = 19;
            this.btnPresetDefault.Text = "Default Settings";
            this.btnPresetDefault.UseVisualStyleBackColor = true;
            this.btnPresetDefault.Click += new System.EventHandler(this.button1_Click);
            // 
            // cbFloppy
            // 
            this.cbFloppy.AutoSize = true;
            this.cbFloppy.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbFloppy.Location = new System.Drawing.Point(23, 158);
            this.cbFloppy.Name = "cbFloppy";
            this.cbFloppy.Size = new System.Drawing.Size(337, 29);
            this.cbFloppy.TabIndex = 4;
            this.cbFloppy.Text = "Transfer data using floppy discs";
            this.cbFloppy.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(18, 579);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(55, 25);
            this.label12.TabIndex = 31;
            this.label12.Text = "Font";
            // 
            // sFontName
            // 
            this.sFontName.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sFontName.Location = new System.Drawing.Point(79, 573);
            this.sFontName.Name = "sFontName";
            this.sFontName.Size = new System.Drawing.Size(308, 31);
            this.sFontName.TabIndex = 16;
            // 
            // cSalesStats
            // 
            this.cSalesStats.AutoSize = true;
            this.cSalesStats.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cSalesStats.Location = new System.Drawing.Point(23, 88);
            this.cSalesStats.Name = "cSalesStats";
            this.cSalesStats.Size = new System.Drawing.Size(833, 29);
            this.cSalesStats.TabIndex = 1;
            this.cSalesStats.Text = "Allow staff to see sales statistics, except about the following comma separated s" +
                "taff:";
            this.cSalesStats.UseVisualStyleBackColor = true;
            // 
            // tbNoStats
            // 
            this.tbNoStats.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbNoStats.Location = new System.Drawing.Point(862, 86);
            this.tbNoStats.Name = "tbNoStats";
            this.tbNoStats.Size = new System.Drawing.Size(100, 31);
            this.tbNoStats.TabIndex = 2;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox1.Location = new System.Drawing.Point(23, 193);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(308, 29);
            this.checkBox1.TabIndex = 32;
            this.checkBox1.Text = "Wait for shut down command";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // frmConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 730);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.tbNoStats);
            this.Controls.Add(this.cSalesStats);
            this.Controls.Add(this.sFontName);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.cbFloppy);
            this.Controls.Add(this.btnPresetDefault);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.btnPresetNothing);
            this.Controls.Add(this.btnPresetDOS);
            this.Controls.Add(this.btnPresetFloppy);
            this.Controls.Add(this.sFloppyDiscLocation);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.sBackupLocation);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.sPrinterOutputPort);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.nLinesBetweenReceipt);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.nPrinterCharWidth);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.nPriceLabelOffset);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.nLinesAfterRegisterReport);
            this.Controls.Add(this.cbDoBackups);
            this.Controls.Add(this.cbUseVirtualPrinter);
            this.Controls.Add(this.cbAutoLowercase);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GTill Configuration";
            this.Load += new System.EventHandler(this.frmConfig_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nLinesAfterRegisterReport)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nPriceLabelOffset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nPrinterCharWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nLinesBetweenReceipt)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbAutoLowercase;
        private System.Windows.Forms.CheckBox cbUseVirtualPrinter;
        private System.Windows.Forms.CheckBox cbDoBackups;
        private System.Windows.Forms.NumericUpDown nLinesAfterRegisterReport;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown nPriceLabelOffset;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown nPrinterCharWidth;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown nLinesBetweenReceipt;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox sPrinterOutputPort;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox sBackupLocation;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox sFloppyDiscLocation;
        private System.Windows.Forms.Button btnPresetFloppy;
        private System.Windows.Forms.Button btnPresetDOS;
        private System.Windows.Forms.Button btnPresetNothing;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnPresetDefault;
        private System.Windows.Forms.CheckBox cbFloppy;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox sFontName;
        private System.Windows.Forms.CheckBox cSalesStats;
        private System.Windows.Forms.TextBox tbNoStats;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}