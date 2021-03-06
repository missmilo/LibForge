﻿namespace ForgeToolGUI.Inspectors
{
  partial class ConversionInspector
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.pickFileButton = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.selectedFileLabel = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.euCheckBox = new System.Windows.Forms.CheckBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.descriptionBox = new System.Windows.Forms.TextBox();
      this.idBox = new System.Windows.Forms.TextBox();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.label5 = new System.Windows.Forms.Label();
      this.logBox = new System.Windows.Forms.TextBox();
      this.buildButton = new System.Windows.Forms.Button();
      this.contentIdTextBox = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // pickFileButton
      // 
      this.pickFileButton.Location = new System.Drawing.Point(6, 19);
      this.pickFileButton.Name = "pickFileButton";
      this.pickFileButton.Size = new System.Drawing.Size(75, 23);
      this.pickFileButton.TabIndex = 0;
      this.pickFileButton.Text = "Pick a File";
      this.pickFileButton.UseVisualStyleBackColor = true;
      this.pickFileButton.Click += new System.EventHandler(this.pickFileButton_Click);
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.selectedFileLabel);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.pickFileButton);
      this.groupBox1.Location = new System.Drawing.Point(4, 3);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(473, 69);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Step 1: Pick a CON";
      // 
      // selectedFileLabel
      // 
      this.selectedFileLabel.AutoSize = true;
      this.selectedFileLabel.Location = new System.Drawing.Point(83, 45);
      this.selectedFileLabel.Name = "selectedFileLabel";
      this.selectedFileLabel.Size = new System.Drawing.Size(0, 13);
      this.selectedFileLabel.TabIndex = 2;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(6, 45);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(71, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Selected File:";
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.euCheckBox);
      this.groupBox2.Controls.Add(this.label3);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Controls.Add(this.descriptionBox);
      this.groupBox2.Controls.Add(this.idBox);
      this.groupBox2.Enabled = false;
      this.groupBox2.Location = new System.Drawing.Point(4, 79);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(473, 108);
      this.groupBox2.TabIndex = 2;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Step 2: Choose Options";
      // 
      // euCheckBox
      // 
      this.euCheckBox.AutoSize = true;
      this.euCheckBox.Location = new System.Drawing.Point(86, 87);
      this.euCheckBox.Name = "euCheckBox";
      this.euCheckBox.Size = new System.Drawing.Size(101, 17);
      this.euCheckBox.TabIndex = 5;
      this.euCheckBox.Text = "Build for SCEE?";
      this.euCheckBox.UseVisualStyleBackColor = true;
      this.euCheckBox.CheckedChanged += new System.EventHandler(this.euCheckBox_CheckedChanged);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(21, 48);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(63, 13);
      this.label3.TabIndex = 4;
      this.label3.Text = "Description:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(9, 22);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(71, 13);
      this.label2.TabIndex = 3;
      this.label2.Text = "ID (16 chars):";
      // 
      // descriptionBox
      // 
      this.descriptionBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.descriptionBox.Location = new System.Drawing.Point(86, 45);
      this.descriptionBox.Multiline = true;
      this.descriptionBox.Name = "descriptionBox";
      this.descriptionBox.Size = new System.Drawing.Size(381, 36);
      this.descriptionBox.TabIndex = 2;
      // 
      // idBox
      // 
      this.idBox.Location = new System.Drawing.Point(86, 19);
      this.idBox.Name = "idBox";
      this.idBox.Size = new System.Drawing.Size(162, 20);
      this.idBox.TabIndex = 0;
      this.idBox.TextChanged += new System.EventHandler(this.idBox_TextChanged);
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.label5);
      this.groupBox3.Controls.Add(this.logBox);
      this.groupBox3.Controls.Add(this.buildButton);
      this.groupBox3.Controls.Add(this.contentIdTextBox);
      this.groupBox3.Controls.Add(this.label4);
      this.groupBox3.Enabled = false;
      this.groupBox3.Location = new System.Drawing.Point(4, 193);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(473, 210);
      this.groupBox3.TabIndex = 3;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Step 3: Build PKG";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(6, 46);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(28, 13);
      this.label5.TabIndex = 4;
      this.label5.Text = "Log:";
      // 
      // logBox
      // 
      this.logBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.logBox.Location = new System.Drawing.Point(6, 62);
      this.logBox.Multiline = true;
      this.logBox.Name = "logBox";
      this.logBox.ReadOnly = true;
      this.logBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.logBox.Size = new System.Drawing.Size(461, 142);
      this.logBox.TabIndex = 3;
      // 
      // buildButton
      // 
      this.buildButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buildButton.Location = new System.Drawing.Point(392, 11);
      this.buildButton.Name = "buildButton";
      this.buildButton.Size = new System.Drawing.Size(75, 23);
      this.buildButton.TabIndex = 2;
      this.buildButton.Text = "Build";
      this.buildButton.UseVisualStyleBackColor = true;
      this.buildButton.Click += new System.EventHandler(this.buildButton_Click);
      // 
      // contentIdTextBox
      // 
      this.contentIdTextBox.AutoSize = true;
      this.contentIdTextBox.Location = new System.Drawing.Point(98, 16);
      this.contentIdTextBox.Name = "contentIdTextBox";
      this.contentIdTextBox.Size = new System.Drawing.Size(0, 13);
      this.contentIdTextBox.TabIndex = 1;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(6, 16);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(86, 13);
      this.label4.TabIndex = 0;
      this.label4.Text = "PKG Content ID:";
      // 
      // ConversionInspector
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "ConversionInspector";
      this.Size = new System.Drawing.Size(480, 406);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button pickFileButton;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Label selectedFileLabel;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.TextBox idBox;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox descriptionBox;
    private System.Windows.Forms.CheckBox euCheckBox;
    private System.Windows.Forms.Label contentIdTextBox;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox logBox;
    private System.Windows.Forms.Button buildButton;
    private System.Windows.Forms.Label label5;
  }
}
