using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stork_Future_TaoLi
{
    /// <summary>
    /// 用户行为： 创建新实例
    /// </summary>
    public class CSI
    {
        /// <summary>
        /// 权重文件
        /// </summary>
        public Dictionary<String, float> weight = new Dictionary<string, float>();

        /// <summary>
        /// 交易计划表
        /// </summary>
        public Dictionary<String, int> order = new Dictionary<string, int>(); 

        /// <summary>
        /// 期货合约
        /// </summary>
        public String CT = String.Empty;

        /// <summary>
        /// 购买手数
        /// </summary>
        public int HD = 1;

        /// <summary>
        /// 开仓点位
        /// </summary>
        public float OP = 1;

        /// <summary>
        /// 指数
        /// </summary>
        public String Index = String.Empty;

        /// <summary>
        /// 发起操作用户名
        /// </summary>
        public String UserName = String.Empty;

        public CSI(Dictionary<String,float> _weight, Dictionary<String,int> _order , String _CT , int _HD , float _OP,String _userName)
        {
            this.weight = _weight;
            this.order = _order;
            this.CT = _CT;
            this.HD = _HD;
            this.OP = _OP;
            this.UserName = _userName;
        }
    }

    /// <summary>
    /// 用户行为： 修改现有实例
    /// </summary>
    public class USI
    {
        /// <summary>
        /// 被修改的实例编号
        /// </summary>
        public Guid id = new Guid();

        /// <summary>
        /// 交易计划表
        /// </summary>
        public Dictionary<String, int> order = new Dictionary<string, int>();

        /// <summary>
        /// 期货合约
        /// </summary>
        public String CT = String.Empty;

        /// <summary>
        /// 购买手数
        /// </summary>
        public int HD = 1;

        /// <summary>
        /// 开仓点位
        /// </summary>
        public float OP = 1;

        /// <summary>
        /// 指数
        /// </summary>
        public String Index = String.Empty;

         /// <summary>
        /// 发起操作用户名
        /// </summary>
        public String UserName = String.Empty;

        public USI(Guid _id, Dictionary<String, int> _order, String _CT, int _HD, float _OP,String _UserName )
        {
            this.id = _id;
            this.order = _order;
            this.CT = _CT;
            this.HD = _HD;
            this.OP = _OP;
            this.UserName =_UserName;
        }
    }

    /// <summary>
    /// 用户行为： 删除现有实例
    /// </summary>
    public class DSI
    {
        public Guid id = new Guid();

        /// <summary>
        /// 发起操作用户名
        /// </summary>
        public String UserName = String.Empty;

        public DSI(Guid _id,String _UserName)
        {
            this.id = _id;
            this.UserName = _UserName;
        }
    }

    /// <summary>
    /// 用户行为： 修改交易执行允许标志
    /// </summary>
    public class ASI
    {
        public Guid id = new Guid();

        public bool brun = false;

        /// <summary>
        /// 发起操作用户名
        /// </summary>
        public String UserName = String.Empty;

        public ASI(Guid _id,bool _brun,String _userName)
        {
            this.id = _id;
            this.brun = _brun;
            this.UserName = _userName;
        }
    }

    /// <summary>
    /// 用户行为： 修改实例执行允许标志
    /// </summary>
    public class RSI
    {
        public Guid id = new Guid();

        public bool bRun = false;

           /// <summary>
        /// 发起操作用户名
        /// </summary>
        public String UserName = String.Empty;

        public RSI(Guid _id, bool _bRun, String _userName)
        {
            this.id = _id;
            this.bRun = _bRun;
            this.UserName = _userName;
        }
    }
}