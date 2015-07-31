namespace MarketInfoSys
{
    partial class ControlPanel
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.tbip = new System.Windows.Forms.TextBox();
            this.tbport = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tbuserName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbpassword = new System.Windows.Forms.TextBox();
            this.rtbSubscribe = new System.Windows.Forms.RichTextBox();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.errTip = new System.Windows.Forms.ToolTip(this.components);
            this.QueueLength = new System.Windows.Forms.Label();
            this.updateCount = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP：";
            // 
            // tbip
            // 
            this.tbip.Location = new System.Drawing.Point(66, 13);
            this.tbip.Name = "tbip";
            this.tbip.Size = new System.Drawing.Size(118, 21);
            this.tbip.TabIndex = 1;
            this.tbip.Text = "114.80.154.34";
            // 
            // tbport
            // 
            this.tbport.Location = new System.Drawing.Point(260, 10);
            this.tbport.Name = "tbport";
            this.tbport.Size = new System.Drawing.Size(100, 21);
            this.tbport.TabIndex = 3;
            this.tbport.Text = "6231";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(214, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "PORT：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(18, 61);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 2;
            this.label5.Text = "用户名：";
            // 
            // tbuserName
            // 
            this.tbuserName.Location = new System.Drawing.Point(66, 58);
            this.tbuserName.Name = "tbuserName";
            this.tbuserName.Size = new System.Drawing.Size(118, 21);
            this.tbuserName.TabIndex = 3;
            this.tbuserName.Text = "TD1033422002";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(214, 61);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 12);
            this.label6.TabIndex = 2;
            this.label6.Text = "密码：";
            // 
            // tbpassword
            // 
            this.tbpassword.Location = new System.Drawing.Point(260, 58);
            this.tbpassword.Name = "tbpassword";
            this.tbpassword.PasswordChar = '*';
            this.tbpassword.Size = new System.Drawing.Size(100, 21);
            this.tbpassword.TabIndex = 3;
            this.tbpassword.Text = "27692616";
            // 
            // rtbSubscribe
            // 
            this.rtbSubscribe.Font = new System.Drawing.Font("SimSun", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rtbSubscribe.Location = new System.Drawing.Point(20, 115);
            this.rtbSubscribe.Name = "rtbSubscribe";
            this.rtbSubscribe.Size = new System.Drawing.Size(347, 432);
            this.rtbSubscribe.TabIndex = 4;
            this.rtbSubscribe.Text = "600030.sh";
            // 
            // btnSubmit
            // 
            this.btnSubmit.Location = new System.Drawing.Point(20, 555);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(347, 23);
            this.btnSubmit.TabIndex = 5;
            this.btnSubmit.Text = "启动运行";
            this.btnSubmit.UseVisualStyleBackColor = true;
            // 
            // QueueLength
            // 
            this.QueueLength.AutoSize = true;
            this.QueueLength.Location = new System.Drawing.Point(20, 97);
            this.QueueLength.Name = "QueueLength";
            this.QueueLength.Size = new System.Drawing.Size(41, 12);
            this.QueueLength.TabIndex = 6;
            this.QueueLength.Text = "label3";
            // 
            // updateCount
            // 
            this.updateCount.Tick += new System.EventHandler(this.updateCount_Tick);
            // 
            // ControlPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(397, 590);
            this.Controls.Add(this.QueueLength);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.rtbSubscribe);
            this.Controls.Add(this.tbpassword);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbuserName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbport);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbip);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ControlPanel";
            this.Text = "行情设置面板";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbip;
        private System.Windows.Forms.TextBox tbport;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbuserName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbpassword;
        private System.Windows.Forms.RichTextBox rtbSubscribe;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.ToolTip errTip;
        private System.Windows.Forms.Label QueueLength;
        private System.Windows.Forms.Timer updateCount;
    }
}