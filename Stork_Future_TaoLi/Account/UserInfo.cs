using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stork_Future_TaoLi.Account
{
    public class userOper
    {
        /// <summary>
        /// 注册新用户
        /// </summary>
        /// <param name="InputJson"></param>
        /// <returns></returns>
        public static string register(String InputJson)
        {
            registerType para = JsonConvert.DeserializeObject<registerType>(InputJson);

            if(para.Password.Length < 16)
            {
                for(int i=0;i<16-para.Password.Length;i++)
                {
                    para.Password += "A";
                }
            }

            return DBAccessLayer.InsertUser(para);
        }

        public static int? login(String InputJson)
        {
            loginType para = JsonConvert.DeserializeObject<loginType>(InputJson);
            if (para.password.Length < 16)
            {
                for (int i = 0; i < 16 - para.password.Length; i++)
                {
                    para.password += "A";
                }
            }

            return DBAccessLayer.Login(para);

        }

        public static bool ChangePassword(String InputJson)
        {
            if (InputJson == null) return false;
            ChangePasswordType para = JsonConvert.DeserializeObject<ChangePasswordType>(InputJson);

            if(para.op.Length <  16)
            {
                for (int i = 0; i < 16 - para.op.Length; i++)
                {
                    para.op += "A";
                }
            }

            if (para.np.Length < 16)
            {
                for (int i = 0; i < 16 - para.np.Length; i++)
                {
                    para.np += "A";
                }
            }

            return DBAccessLayer.ChangePassword(para);
        }
    }

    /// <summary>
    /// 注册账户类型
    /// </summary>
    public class registerType
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 真实名
        /// </summary>
        public string Realname { get; set; }

        /// <summary>
        /// 股票可用资金
        /// </summary>
        public string StockAccount { get; set; }

        /// <summary>
        /// 期货可用资金
        /// </summary>
        public string FutureAccount { get; set; }

        /// <summary>
        /// 用户类型
        /// </summary>
        public string right { get; set; }
    }

    /// <summary>
    /// 登录类型
    /// </summary>
    public class loginType
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string password { get; set; }
    }

    /// <summary>
    /// 修改密码数据类型
    /// </summary>
    public class ChangePasswordType
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 旧密码
        /// </summary>
        public string op { get; set;}

        /// <summary>
        /// 新密码
        /// </summary>
        public string np { get; set; }
    }
}