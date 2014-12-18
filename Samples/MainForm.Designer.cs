namespace Samples
{
    partial class MainForm
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.gbFindList = new System.Windows.Forms.GroupBox();
            this.numFLCount = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.numFLMaxLen = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.lxFLSource = new System.Windows.Forms.ListBox();
            this.btnFLTest = new System.Windows.Forms.Button();
            this.tbFLResult = new System.Windows.Forms.TextBox();
            this.lbResult = new System.Windows.Forms.Label();
            this.gbFindList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFLCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFLMaxLen)).BeginInit();
            this.SuspendLayout();
            // 
            // gbFindList
            // 
            this.gbFindList.Controls.Add(this.lbResult);
            this.gbFindList.Controls.Add(this.tbFLResult);
            this.gbFindList.Controls.Add(this.btnFLTest);
            this.gbFindList.Controls.Add(this.lxFLSource);
            this.gbFindList.Controls.Add(this.label2);
            this.gbFindList.Controls.Add(this.label1);
            this.gbFindList.Controls.Add(this.numFLMaxLen);
            this.gbFindList.Controls.Add(this.numFLCount);
            this.gbFindList.Location = new System.Drawing.Point(12, 12);
            this.gbFindList.Name = "gbFindList";
            this.gbFindList.Size = new System.Drawing.Size(479, 346);
            this.gbFindList.TabIndex = 0;
            this.gbFindList.TabStop = false;
            this.gbFindList.Text = "FindList with string elements";
            // 
            // numFLCount
            // 
            this.numFLCount.Location = new System.Drawing.Point(51, 21);
            this.numFLCount.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numFLCount.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numFLCount.Name = "numFLCount";
            this.numFLCount.Size = new System.Drawing.Size(73, 22);
            this.numFLCount.TabIndex = 0;
            this.numFLCount.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Count";
            // 
            // numFLMaxLen
            // 
            this.numFLMaxLen.Location = new System.Drawing.Point(237, 21);
            this.numFLMaxLen.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numFLMaxLen.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numFLMaxLen.Name = "numFLMaxLen";
            this.numFLMaxLen.Size = new System.Drawing.Size(45, 22);
            this.numFLMaxLen.TabIndex = 0;
            this.numFLMaxLen.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(130, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Max. string length";
            // 
            // lxFLSource
            // 
            this.lxFLSource.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lxFLSource.FormattingEnabled = true;
            this.lxFLSource.IntegralHeight = false;
            this.lxFLSource.Location = new System.Drawing.Point(6, 49);
            this.lxFLSource.Name = "lxFLSource";
            this.lxFLSource.Size = new System.Drawing.Size(464, 199);
            this.lxFLSource.TabIndex = 2;
            // 
            // btnFLTest
            // 
            this.btnFLTest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFLTest.Location = new System.Drawing.Point(289, 21);
            this.btnFLTest.Name = "btnFLTest";
            this.btnFLTest.Size = new System.Drawing.Size(181, 23);
            this.btnFLTest.TabIndex = 3;
            this.btnFLTest.Text = "Test!";
            this.btnFLTest.UseVisualStyleBackColor = true;
            this.btnFLTest.Click += new System.EventHandler(this.btnFLTest_Click);
            // 
            // tbFLResult
            // 
            this.tbFLResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFLResult.Location = new System.Drawing.Point(6, 272);
            this.tbFLResult.Multiline = true;
            this.tbFLResult.Name = "tbFLResult";
            this.tbFLResult.Size = new System.Drawing.Size(464, 68);
            this.tbFLResult.TabIndex = 4;
            // 
            // lbResult
            // 
            this.lbResult.AutoSize = true;
            this.lbResult.Location = new System.Drawing.Point(7, 253);
            this.lbResult.Name = "lbResult";
            this.lbResult.Size = new System.Drawing.Size(220, 13);
            this.lbResult.TabIndex = 5;
            this.lbResult.Text = "Results in System.Diagnostics.Watch ticks";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(697, 399);
            this.Controls.Add(this.gbFindList);
            this.Name = "MainForm";
            this.Text = "Примеры";
            this.gbFindList.ResumeLayout(false);
            this.gbFindList.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFLCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFLMaxLen)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbFindList;
        private System.Windows.Forms.Button btnFLTest;
        private System.Windows.Forms.ListBox lxFLSource;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numFLMaxLen;
        private System.Windows.Forms.NumericUpDown numFLCount;
        private System.Windows.Forms.Label lbResult;
        private System.Windows.Forms.TextBox tbFLResult;
    }
}

