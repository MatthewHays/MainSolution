namespace AviFileRename
{
    partial class MainForm
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
            this._renameButton = new System.Windows.Forms.Button();
            this._sourceTextBox = new System.Windows.Forms.TextBox();
            this._sourceButton = new System.Windows.Forms.Button();
            this._collapseFlattenButton = new System.Windows.Forms.Button();
            this._collapseToDestButton = new System.Windows.Forms.Button();
            this._destButton = new System.Windows.Forms.Button();
            this._destTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // _renameButton
            // 
            this._renameButton.Location = new System.Drawing.Point(12, 71);
            this._renameButton.Name = "_renameButton";
            this._renameButton.Size = new System.Drawing.Size(159, 23);
            this._renameButton.TabIndex = 0;
            this._renameButton.Text = "RenameAll";
            this._renameButton.UseVisualStyleBackColor = true;
            this._renameButton.Click += new System.EventHandler(this.rename_Click);
            // 
            // _sourceTextBox
            // 
            this._sourceTextBox.Location = new System.Drawing.Point(12, 12);
            this._sourceTextBox.Name = "_sourceTextBox";
            this._sourceTextBox.Size = new System.Drawing.Size(204, 20);
            this._sourceTextBox.TabIndex = 1;
            this._sourceTextBox.Text = "E:\\Downloads";
            // 
            // _sourceButton
            // 
            this._sourceButton.Location = new System.Drawing.Point(229, 12);
            this._sourceButton.Name = "_sourceButton";
            this._sourceButton.Size = new System.Drawing.Size(34, 23);
            this._sourceButton.TabIndex = 2;
            this._sourceButton.Text = "...";
            this._sourceButton.UseVisualStyleBackColor = true;
            this._sourceButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // _collapseFlattenButton
            // 
            this._collapseFlattenButton.Location = new System.Drawing.Point(12, 100);
            this._collapseFlattenButton.Name = "_collapseFlattenButton";
            this._collapseFlattenButton.Size = new System.Drawing.Size(159, 23);
            this._collapseFlattenButton.TabIndex = 3;
            this._collapseFlattenButton.Text = "CollapseFlatten";
            this._collapseFlattenButton.UseVisualStyleBackColor = true;
            this._collapseFlattenButton.Click += new System.EventHandler(this.collapseFlatten_Click);
            // 
            // _collapseToDestButton
            // 
            this._collapseToDestButton.Location = new System.Drawing.Point(12, 129);
            this._collapseToDestButton.Name = "_collapseToDestButton";
            this._collapseToDestButton.Size = new System.Drawing.Size(159, 23);
            this._collapseToDestButton.TabIndex = 4;
            this._collapseToDestButton.Text = "CollapseToDest";
            this._collapseToDestButton.UseVisualStyleBackColor = true;
            this._collapseToDestButton.Click += new System.EventHandler(this.collapseToDest_Click);
            // 
            // _destButton
            // 
            this._destButton.Location = new System.Drawing.Point(229, 38);
            this._destButton.Name = "_destButton";
            this._destButton.Size = new System.Drawing.Size(34, 23);
            this._destButton.TabIndex = 6;
            this._destButton.Text = "...";
            this._destButton.UseVisualStyleBackColor = true;
            this._destButton.Click += new System.EventHandler(this._destButton_Click);
            // 
            // _destTextBox
            // 
            this._destTextBox.Location = new System.Drawing.Point(12, 38);
            this._destTextBox.Name = "_destTextBox";
            this._destTextBox.Size = new System.Drawing.Size(204, 20);
            this._destTextBox.TabIndex = 5;
            this._destTextBox.Text = "E:\\Videos\\Movies";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(275, 159);
            this.Controls.Add(this._destButton);
            this.Controls.Add(this._destTextBox);
            this.Controls.Add(this._collapseToDestButton);
            this.Controls.Add(this._collapseFlattenButton);
            this.Controls.Add(this._sourceButton);
            this.Controls.Add(this._sourceTextBox);
            this.Controls.Add(this._renameButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MainForm";
            this.Text = "Movie Rename";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _renameButton;
        private System.Windows.Forms.TextBox _sourceTextBox;
        private System.Windows.Forms.Button _sourceButton;
        private System.Windows.Forms.Button _collapseFlattenButton;
        private System.Windows.Forms.Button _collapseToDestButton;
        private System.Windows.Forms.Button _destButton;
        private System.Windows.Forms.TextBox _destTextBox;
    }
}

