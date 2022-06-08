namespace KeyEncryptorUtil
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxInputKey = new System.Windows.Forms.MaskedTextBox();
            this.textBoxOutputKey = new System.Windows.Forms.TextBox();
            this.buttonEncrypt = new System.Windows.Forms.Button();
            this.buttonCopy = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 25);
            this.label1.TabIndex = 1;
            this.label1.Text = "API Secret Key";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 25);
            this.label2.TabIndex = 2;
            this.label2.Text = "Encrypted Key";
            // 
            // textBoxInputKey
            // 
            this.textBoxInputKey.Location = new System.Drawing.Point(143, 6);
            this.textBoxInputKey.Name = "textBoxInputKey";
            this.textBoxInputKey.Size = new System.Drawing.Size(216, 31);
            this.textBoxInputKey.TabIndex = 3;
            this.textBoxInputKey.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxInputKey_KeyDown);
            // 
            // textBoxOutputKey
            // 
            this.textBoxOutputKey.Location = new System.Drawing.Point(143, 51);
            this.textBoxOutputKey.Multiline = true;
            this.textBoxOutputKey.Name = "textBoxOutputKey";
            this.textBoxOutputKey.ReadOnly = true;
            this.textBoxOutputKey.Size = new System.Drawing.Size(411, 129);
            this.textBoxOutputKey.TabIndex = 4;
            // 
            // buttonEncrypt
            // 
            this.buttonEncrypt.Location = new System.Drawing.Point(375, 3);
            this.buttonEncrypt.Name = "buttonEncrypt";
            this.buttonEncrypt.Size = new System.Drawing.Size(179, 34);
            this.buttonEncrypt.TabIndex = 5;
            this.buttonEncrypt.Text = "Encrypt";
            this.buttonEncrypt.UseVisualStyleBackColor = true;
            this.buttonEncrypt.Click += new System.EventHandler(this.buttonEncrypt_Click);
            // 
            // buttonCopy
            // 
            this.buttonCopy.Location = new System.Drawing.Point(442, 186);
            this.buttonCopy.Name = "buttonCopy";
            this.buttonCopy.Size = new System.Drawing.Size(112, 34);
            this.buttonCopy.TabIndex = 6;
            this.buttonCopy.Text = "Copy";
            this.buttonCopy.UseVisualStyleBackColor = true;
            this.buttonCopy.Click += new System.EventHandler(this.buttonCopy_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.buttonCopy);
            this.Controls.Add(this.buttonEncrypt);
            this.Controls.Add(this.textBoxOutputKey);
            this.Controls.Add(this.textBoxInputKey);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MainForm";
            this.Text = "Key Encryptor Utility";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Label label1;
        private Label label2;
        private MaskedTextBox textBoxInputKey;
        private TextBox textBoxOutputKey;
        private Button buttonEncrypt;
        private Button buttonCopy;
    }
}