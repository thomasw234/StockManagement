namespace BackOffice
{
    public partial class frmOffersReceptDesigner
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnUnderline = new System.Windows.Forms.Button();
            this.btnHighlight = new System.Windows.Forms.Button();
            this.btnEmphasised = new System.Windows.Forms.Button();
            this.btnBarcode = new System.Windows.Forms.Button();
            this.btnDoubleWidth = new System.Windows.Forms.Button();
            this.btnDoubleHeight = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCentre = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(12, 113);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(562, 362);
            this.textBox1.TabIndex = 0;
            // 
            // btnUnderline
            // 
            this.btnUnderline.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnUnderline.Location = new System.Drawing.Point(12, 69);
            this.btnUnderline.Name = "btnUnderline";
            this.btnUnderline.Size = new System.Drawing.Size(75, 38);
            this.btnUnderline.TabIndex = 1;
            this.btnUnderline.Text = "Underline";
            this.btnUnderline.UseVisualStyleBackColor = true;
            this.btnUnderline.Click += new System.EventHandler(this.btnUnderline_Click);
            // 
            // btnHighlight
            // 
            this.btnHighlight.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHighlight.Location = new System.Drawing.Point(93, 69);
            this.btnHighlight.Name = "btnHighlight";
            this.btnHighlight.Size = new System.Drawing.Size(75, 38);
            this.btnHighlight.TabIndex = 2;
            this.btnHighlight.Text = "Highlight";
            this.btnHighlight.UseVisualStyleBackColor = true;
            this.btnHighlight.Click += new System.EventHandler(this.btnHighlight_Click);
            // 
            // btnEmphasised
            // 
            this.btnEmphasised.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEmphasised.Location = new System.Drawing.Point(174, 69);
            this.btnEmphasised.Name = "btnEmphasised";
            this.btnEmphasised.Size = new System.Drawing.Size(75, 38);
            this.btnEmphasised.TabIndex = 3;
            this.btnEmphasised.Text = "Emphasised";
            this.btnEmphasised.UseVisualStyleBackColor = true;
            this.btnEmphasised.Click += new System.EventHandler(this.btnEmphasised_Click);
            // 
            // btnBarcode
            // 
            this.btnBarcode.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBarcode.Location = new System.Drawing.Point(255, 69);
            this.btnBarcode.Name = "btnBarcode";
            this.btnBarcode.Size = new System.Drawing.Size(75, 38);
            this.btnBarcode.TabIndex = 4;
            this.btnBarcode.Text = "Barcode";
            this.btnBarcode.UseVisualStyleBackColor = true;
            this.btnBarcode.Click += new System.EventHandler(this.btnBarcode_Click);
            // 
            // btnDoubleWidth
            // 
            this.btnDoubleWidth.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDoubleWidth.Location = new System.Drawing.Point(336, 69);
            this.btnDoubleWidth.Name = "btnDoubleWidth";
            this.btnDoubleWidth.Size = new System.Drawing.Size(75, 38);
            this.btnDoubleWidth.TabIndex = 5;
            this.btnDoubleWidth.Text = "Double Width";
            this.btnDoubleWidth.UseVisualStyleBackColor = true;
            this.btnDoubleWidth.Click += new System.EventHandler(this.btnDoubleWidth_Click);
            // 
            // btnDoubleHeight
            // 
            this.btnDoubleHeight.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDoubleHeight.Location = new System.Drawing.Point(418, 69);
            this.btnDoubleHeight.Name = "btnDoubleHeight";
            this.btnDoubleHeight.Size = new System.Drawing.Size(75, 38);
            this.btnDoubleHeight.TabIndex = 6;
            this.btnDoubleHeight.Text = "Double Height";
            this.btnDoubleHeight.UseVisualStyleBackColor = true;
            this.btnDoubleHeight.Click += new System.EventHandler(this.btnDoubleHeight_Click);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(481, 56);
            this.label1.TabIndex = 7;
            this.label1.Text = "Design how the voucher will print on this screen. The buttons below add tags to t" +
                "he text box. These won\'t show up on the voucher, but text enclosed within them w" +
                "ill be. You can have tags within tags.";
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(488, 481);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(88, 44);
            this.btnSave.TabIndex = 8;
            this.btnSave.Text = "Save && Exit";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCentre
            // 
            this.btnCentre.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCentre.Location = new System.Drawing.Point(499, 69);
            this.btnCentre.Name = "btnCentre";
            this.btnCentre.Size = new System.Drawing.Size(75, 38);
            this.btnCentre.TabIndex = 9;
            this.btnCentre.Text = "Centralise";
            this.btnCentre.UseVisualStyleBackColor = true;
            this.btnCentre.Click += new System.EventHandler(this.btnCentre_Click);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(430, 52);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(144, 14);
            this.linkLabel1.TabIndex = 10;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Click to see a sample receipt";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // frmOffersReceptDesigner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(588, 530);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.btnCentre);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnDoubleHeight);
            this.Controls.Add(this.btnDoubleWidth);
            this.Controls.Add(this.btnBarcode);
            this.Controls.Add(this.btnEmphasised);
            this.Controls.Add(this.btnHighlight);
            this.Controls.Add(this.btnUnderline);
            this.Controls.Add(this.textBox1);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmOffersReceptDesigner";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Receipt Designer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnUnderline;
        private System.Windows.Forms.Button btnHighlight;
        private System.Windows.Forms.Button btnEmphasised;
        private System.Windows.Forms.Button btnBarcode;
        private System.Windows.Forms.Button btnDoubleWidth;
        private System.Windows.Forms.Button btnDoubleHeight;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCentre;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}