using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExportDLL;

namespace CSharp_Use_CPP
{
    class Program
    {
        static void Main(string[] args)
        {
            ExportDLL.CLog4C c1 = new CLog4C();
            ExportDLL.CLog4C c2 = new CLog4C();

            int k1 = c1.cal(3, 4);
            int k2 = c2.cal(3, 5);

            Console.WriteLine("k1 = " + k1);
            Console.WriteLine("k2 = " + k2);
            String s = string.Empty;
            

            Console.ReadLine();
        }
    }
}
