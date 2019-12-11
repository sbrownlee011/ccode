namespace UR2Lab10
{
    partial class Form1
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
            this.sourcePictureBox = new System.Windows.Forms.PictureBox();
            this.roiPictureBox = new System.Windows.Forms.PictureBox();
            this.contourBox = new System.Windows.Forms.PictureBox();
            this.squareCount = new System.Windows.Forms.Label();
            this.triangleCount = new System.Windows.Forms.Label();
            this.sButton = new System.Windows.Forms.Button();
            this.onOffLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.sourcePictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.roiPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.contourBox)).BeginInit();
            this.SuspendLayout();
            // 
            // sourcePictureBox
            // 
            this.sourcePictureBox.Location = new System.Drawing.Point(42, 46);
            this.sourcePictureBox.Name = "sourcePictureBox";
            this.sourcePictureBox.Size = new System.Drawing.Size(231, 169);
            this.sourcePictureBox.TabIndex = 0;
            this.sourcePictureBox.TabStop = false;
            // 
            // roiPictureBox
            // 
            this.roiPictureBox.Location = new System.Drawing.Point(42, 254);
            this.roiPictureBox.Name = "roiPictureBox";
            this.roiPictureBox.Size = new System.Drawing.Size(231, 169);
            this.roiPictureBox.TabIndex = 1;
            this.roiPictureBox.TabStop = false;
            // 
            // contourBox
            // 
            this.contourBox.Location = new System.Drawing.Point(343, 46);
            this.contourBox.Name = "contourBox";
            this.contourBox.Size = new System.Drawing.Size(231, 169);
            this.contourBox.TabIndex = 2;
            this.contourBox.TabStop = false;
            // 
            // squareCount
            // 
            this.squareCount.AutoSize = true;
            this.squareCount.Location = new System.Drawing.Point(340, 254);
            this.squareCount.Name = "squareCount";
            this.squareCount.Size = new System.Drawing.Size(130, 17);
            this.squareCount.TabIndex = 3;
            this.squareCount.Text = "# squares detected";
            // 
            // triangleCount
            // 
            this.triangleCount.AutoSize = true;
            this.triangleCount.Location = new System.Drawing.Point(340, 294);
            this.triangleCount.Name = "triangleCount";
            this.triangleCount.Size = new System.Drawing.Size(133, 17);
            this.triangleCount.TabIndex = 4;
            this.triangleCount.Text = "# triangles detected";
            // 
            // sButton
            // 
            this.sButton.Location = new System.Drawing.Point(343, 342);
            this.sButton.Name = "sButton";
            this.sButton.Size = new System.Drawing.Size(75, 23);
            this.sButton.TabIndex = 5;
            this.sButton.Text = "Start!";
            this.sButton.UseVisualStyleBackColor = true;
            this.sButton.Click += new System.EventHandler(this.sButton_Click);
            // 
            // onOffLabel
            // 
            this.onOffLabel.AutoSize = true;
            this.onOffLabel.Location = new System.Drawing.Point(340, 378);
            this.onOffLabel.Name = "onOffLabel";
            this.onOffLabel.Size = new System.Drawing.Size(27, 17);
            this.onOffLabel.TabIndex = 6;
            this.onOffLabel.Text = "Off";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.onOffLabel);
            this.Controls.Add(this.sButton);
            this.Controls.Add(this.triangleCount);
            this.Controls.Add(this.squareCount);
            this.Controls.Add(this.contourBox);
            this.Controls.Add(this.roiPictureBox);
            this.Controls.Add(this.sourcePictureBox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.sourcePictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.roiPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.contourBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox sourcePictureBox;
        private System.Windows.Forms.PictureBox roiPictureBox;
        private System.Windows.Forms.PictureBox contourBox;
        private System.Windows.Forms.Label squareCount;
        private System.Windows.Forms.Label triangleCount;
        private System.Windows.Forms.Button sButton;
        private System.Windows.Forms.Label onOffLabel;
    }
}

