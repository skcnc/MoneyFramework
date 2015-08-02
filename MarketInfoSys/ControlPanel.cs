using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace MarketInfoSys
{
    public partial class ControlPanel : Form
    {
        webservice service = new webservice();
        
        private delegate void ProxyClient();

        public ControlPanel()
        {
            InitializeComponent();
            btnSubmit.Click += btnSubmit_Click;
            this.FormClosed += ControlPanel_FormClosed;
            this.rtbSubscribe.Text = "600030.sh\n600036.sh\nIF1508.cf\n000300.sh";
            updateCount.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ControlPanel_FormClosed(object sender, FormClosedEventArgs e)
        {
            webservice.STOP = true;
            //throw new NotImplementedException();
        }

        /// <summary>
        /// 启动或停止运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnSubmit_Click(object sender, EventArgs e)
        {
            if (this.btnSubmit.Text == "启动运行")
            {
                TDFMain.ip = tbip.Text.Trim();
                TDFMain.port = tbport.Text.Trim();
                TDFMain.userName = tbuserName.Text.Trim();
                TDFMain.password = tbpassword.Text.Trim();
                TDFMain.subscribeList = rtbSubscribe.Text.Trim();

                this.btnSubmit.Text = "停止运行";

                webservice.STOP = false;
                webservice.run();
            }
            else
            {
                webservice.STOP = true;
                this.btnSubmit.Text = "启动运行";
            }
        }

        private void updateCount_Tick(object sender, EventArgs e)
        {
            this.QueueLength.Text = "目前队列长度:" +  webservice.GetLength().ToString();
        }
    }
}
