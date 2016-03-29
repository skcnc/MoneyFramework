using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IronPython.Hosting;

namespace Stork_Future_TaoLi
{
    public class pythonOper
    {
        private static pythonOper instance = new pythonOper();

        public static pythonOper GetInstance()
        {
            return instance;
        }

        /// <summary>
        /// 从配置文件中获取批量交易
        /// </summary>
        /// <returns>批量交易内容</returns>
        public List<string> GetBatchTradeList()
        {
            var engin = Python.CreateEngine();
            dynamic py = engin.ExecuteFile(@"F:\workbench\suncheng\pythonOpers.py");

            dynamic calc = py.ConfigureContext();

            List<string> rets = new List<string>();

            var strs = calc.GetConfigBatchTradeList();

            if (strs.Count <= 1) return rets;

            for (int i = 1; i < strs.Count; i++)
            {
                string item = strs[i];
                rets.Add(item);
            }

            return rets;
        }
    }
}