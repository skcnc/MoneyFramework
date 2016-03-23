using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IronPython.Hosting;

namespace Stork_Future_TaoLi.AdditionalModule
{
    public class pythonOper
    {
        /// <summary>
        /// 从配置文件中获取批量交易
        /// </summary>
        /// <returns>批量交易内容</returns>
        public static List<string> GetBatchTradeList()
        {
            var engin = Python.CreateEngine();
            dynamic py = engin.ExecuteFile(@"F:\workbench\suncheng\金融套利系统\Stork_Future_TaoLi\Pythons\pythonOpers.py");

            dynamic calc = py.ConfigureContext();

            List<string> rets = new List<string>();

            string[] strs = calc.GetConfigBatchTradeList();

            if (strs.Count() <= 1) return rets;

            for (int i = 1; i < strs.Count(); i++)
            {
                rets.Add(strs[i]);
            }

            return rets;
        }
    }
}