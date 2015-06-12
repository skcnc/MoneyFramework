using System.Web;
using System.Web.Mvc;

namespace Stork_Future_TaoLi
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}