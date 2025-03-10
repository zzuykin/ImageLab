namespace Laba1Form
{
    partial class Form1
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
            label1 = new Label();
            mainPBox = new PictureBox();
            bOpen = new Button();
            bSave = new Button();
            resultLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)mainPBox).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Cursor = Cursors.No;
            label1.Font = new Font("Segoe UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label1.Location = new Point(530, 9);
            label1.Name = "label1";
            label1.Size = new Size(185, 35);
            label1.TabIndex = 0;
            label1.Text = "Супер-фотошоп";
            // 
            // mainPBox
            // 
            mainPBox.BorderStyle = BorderStyle.FixedSingle;
            mainPBox.Location = new Point(12, 53);
            mainPBox.Name = "mainPBox";
            mainPBox.Size = new Size(872, 662);
            mainPBox.SizeMode = PictureBoxSizeMode.Zoom;
            mainPBox.TabIndex = 1;
            mainPBox.TabStop = false;
            mainPBox.Click += mainPBox_Click;
            // 
            // bOpen
            // 
            bOpen.Location = new Point(969, 52);
            bOpen.Name = "bOpen";
            bOpen.Size = new Size(152, 29);
            bOpen.TabIndex = 2;
            bOpen.Text = "Добавить картинку";
            bOpen.UseVisualStyleBackColor = true;
            bOpen.Click += bOpen_Click;
            // 
            // bSave
            // 
            bSave.Location = new Point(969, 88);
            bSave.Name = "bSave";
            bSave.Size = new Size(152, 29);
            bSave.TabIndex = 3;
            bSave.Text = "Сохранить";
            bSave.UseVisualStyleBackColor = true;
            bSave.Click += bSave_Click;
            // 
            // resultLabel
            // 
            resultLabel.AutoSize = true;
            resultLabel.Location = new Point(920, 24);
            resultLabel.Name = "resultLabel";
            resultLabel.Size = new Size(251, 20);
            resultLabel.TabIndex = 4;
            resultLabel.Text = "Изображение успешно сохранено";
            resultLabel.Visible = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(1689, 736);
            Controls.Add(resultLabel);
            Controls.Add(bSave);
            Controls.Add(bOpen);
            Controls.Add(mainPBox);
            Controls.Add(label1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)mainPBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private PictureBox mainPBox;
        private Button bOpen;
        private Button bSave;
        private Label resultLabel;
    }
}
