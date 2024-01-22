namespace AC_fqrand_solver
{
    partial class MapProbabilityForm
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
            this.MapPictureBox = new System.Windows.Forms.PictureBox();
            this.ResetButton = new System.Windows.Forms.Button();
            this.ShineLabel = new System.Windows.Forms.Label();
            this.MoneyRockLabel = new System.Windows.Forms.Label();
            this.showGoldenBox = new System.Windows.Forms.CheckBox();
            this.showRockBox = new System.Windows.Forms.CheckBox();
            this.AcreModeCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.MapPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // MapPictureBox
            // 
            this.MapPictureBox.Location = new System.Drawing.Point(12, 80);
            this.MapPictureBox.Name = "MapPictureBox";
            this.MapPictureBox.Size = new System.Drawing.Size(660, 792);
            this.MapPictureBox.TabIndex = 0;
            this.MapPictureBox.TabStop = false;
            this.MapPictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.MapPictureBox_Paint);
            this.MapPictureBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MapPictureBox_MouseClick);
            this.MapPictureBox.MouseLeave += new System.EventHandler(this.MapPictureBox_MouseLeave);
            this.MapPictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MapPictureBox_MouseMove);
            // 
            // ResetButton
            // 
            this.ResetButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ResetButton.Location = new System.Drawing.Point(12, 908);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(660, 52);
            this.ResetButton.TabIndex = 1;
            this.ResetButton.Text = "Reset All Acres";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // ShineLabel
            // 
            this.ShineLabel.AutoSize = true;
            this.ShineLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ShineLabel.Location = new System.Drawing.Point(12, 9);
            this.ShineLabel.Name = "ShineLabel";
            this.ShineLabel.Size = new System.Drawing.Size(185, 18);
            this.ShineLabel.TabIndex = 2;
            this.ShineLabel.Text = "Best Golden Spot Acre(s): ";
            // 
            // MoneyRockLabel
            // 
            this.MoneyRockLabel.AutoSize = true;
            this.MoneyRockLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MoneyRockLabel.Location = new System.Drawing.Point(12, 39);
            this.MoneyRockLabel.Name = "MoneyRockLabel";
            this.MoneyRockLabel.Size = new System.Drawing.Size(187, 18);
            this.MoneyRockLabel.TabIndex = 3;
            this.MoneyRockLabel.Text = "Best Money Rock Acre(s): ";
            // 
            // showGoldenBox
            // 
            this.showGoldenBox.AutoSize = true;
            this.showGoldenBox.Checked = true;
            this.showGoldenBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showGoldenBox.Location = new System.Drawing.Point(15, 885);
            this.showGoldenBox.Name = "showGoldenBox";
            this.showGoldenBox.Size = new System.Drawing.Size(143, 17);
            this.showGoldenBox.TabIndex = 4;
            this.showGoldenBox.Text = "Show Each Golden Spot";
            this.showGoldenBox.UseVisualStyleBackColor = true;
            this.showGoldenBox.CheckedChanged += new System.EventHandler(this.ShowCheckboxChanged);
            // 
            // showRockBox
            // 
            this.showRockBox.AutoSize = true;
            this.showRockBox.Checked = true;
            this.showRockBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showRockBox.Location = new System.Drawing.Point(165, 885);
            this.showRockBox.Name = "showRockBox";
            this.showRockBox.Size = new System.Drawing.Size(170, 17);
            this.showRockBox.TabIndex = 5;
            this.showRockBox.Text = "Show Each Money Rock Spot";
            this.showRockBox.UseVisualStyleBackColor = true;
            this.showRockBox.CheckedChanged += new System.EventHandler(this.ShowCheckboxChanged);
            // 
            // AcreModeCheckBox
            // 
            this.AcreModeCheckBox.AutoSize = true;
            this.AcreModeCheckBox.Location = new System.Drawing.Point(561, 885);
            this.AcreModeCheckBox.Name = "AcreModeCheckBox";
            this.AcreModeCheckBox.Size = new System.Drawing.Size(111, 17);
            this.AcreModeCheckBox.TabIndex = 6;
            this.AcreModeCheckBox.Text = "Acre Select Mode";
            this.AcreModeCheckBox.UseVisualStyleBackColor = true;
            this.AcreModeCheckBox.CheckedChanged += new System.EventHandler(this.AcreModeCheckBox_CheckedChanged);
            // 
            // MapProbabilityForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(692, 972);
            this.Controls.Add(this.AcreModeCheckBox);
            this.Controls.Add(this.showRockBox);
            this.Controls.Add(this.showGoldenBox);
            this.Controls.Add(this.MoneyRockLabel);
            this.Controls.Add(this.ShineLabel);
            this.Controls.Add(this.ResetButton);
            this.Controls.Add(this.MapPictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MapProbabilityForm";
            this.Text = "Probability Form";
            ((System.ComponentModel.ISupportInitialize)(this.MapPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox MapPictureBox;
        private System.Windows.Forms.Button ResetButton;
        private System.Windows.Forms.Label ShineLabel;
        private System.Windows.Forms.Label MoneyRockLabel;
        private System.Windows.Forms.CheckBox showGoldenBox;
        private System.Windows.Forms.CheckBox showRockBox;
        private System.Windows.Forms.CheckBox AcreModeCheckBox;
    }
}