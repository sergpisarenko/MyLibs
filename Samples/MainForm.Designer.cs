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
            this.tbExpression = new System.Windows.Forms.TextBox();
            this.lbExpression = new System.Windows.Forms.Label();
            this.tbResult = new System.Windows.Forms.TextBox();
            this.btnEvaluate = new System.Windows.Forms.Button();
            this.lbResult = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbExpression
            // 
            this.tbExpression.Location = new System.Drawing.Point(12, 31);
            this.tbExpression.Name = "tbExpression";
            this.tbExpression.Size = new System.Drawing.Size(673, 22);
            this.tbExpression.TabIndex = 0;
            this.tbExpression.Text = "System.Math.PI < 3.0 ? \"impossible\" : \"correct\"";
            // 
            // lbExpression
            // 
            this.lbExpression.AutoSize = true;
            this.lbExpression.Location = new System.Drawing.Point(9, 15);
            this.lbExpression.Name = "lbExpression";
            this.lbExpression.Size = new System.Drawing.Size(62, 13);
            this.lbExpression.TabIndex = 1;
            this.lbExpression.Text = "Expression";
            // 
            // tbResult
            // 
            this.tbResult.Location = new System.Drawing.Point(12, 102);
            this.tbResult.Name = "tbResult";
            this.tbResult.Size = new System.Drawing.Size(673, 22);
            this.tbResult.TabIndex = 0;
            // 
            // btnEvaluate
            // 
            this.btnEvaluate.Location = new System.Drawing.Point(12, 59);
            this.btnEvaluate.Name = "btnEvaluate";
            this.btnEvaluate.Size = new System.Drawing.Size(75, 23);
            this.btnEvaluate.TabIndex = 2;
            this.btnEvaluate.Text = "Evaluate";
            this.btnEvaluate.UseVisualStyleBackColor = true;
            this.btnEvaluate.Click += new System.EventHandler(this.btnEvaluate_Click);
            // 
            // lbResult
            // 
            this.lbResult.AutoSize = true;
            this.lbResult.Location = new System.Drawing.Point(12, 86);
            this.lbResult.Name = "lbResult";
            this.lbResult.Size = new System.Drawing.Size(39, 13);
            this.lbResult.TabIndex = 1;
            this.lbResult.Text = "Result";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(697, 141);
            this.Controls.Add(this.btnEvaluate);
            this.Controls.Add(this.lbResult);
            this.Controls.Add(this.lbExpression);
            this.Controls.Add(this.tbResult);
            this.Controls.Add(this.tbExpression);
            this.Name = "MainForm";
            this.Text = "Test Form";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbExpression;
        private System.Windows.Forms.Label lbExpression;
        private System.Windows.Forms.TextBox tbResult;
        private System.Windows.Forms.Button btnEvaluate;
        private System.Windows.Forms.Label lbResult;

    }
}

