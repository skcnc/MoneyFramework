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
            this.rtbSubscribe.Text = "600030.sh\n600036.sh\n600048.sh\n600050.sh\n600089.sh\n600104.sh\n600109.sh\n600111.sh\n600150.sh\n600256.sh\n600332.sh\n600372.sh\n600406.sh\n600518.sh\n600519.sh\n600585.sh\n600637.sh\n600690.sh\n600703.sh\n600832.sh\n600837.sh\n600887.sh\n600999.sh\n601006.sh\n601088.sh\n601118.sh\n601166.sh\n601169.sh\n601288.sh\n601299.sh\n601318.sh\n601328.sh\n601398.sh\n601601.sh\n601628.sh\n601668.sh\n601688.sh\n601766.sh\n601818.sh\n601857.sh\n601901.sh\n601989.sh\n601998.sh\n600196.sh\n";
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
