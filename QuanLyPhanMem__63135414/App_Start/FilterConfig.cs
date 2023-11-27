using System.Web;
using System.Web.Mvc;

namespace QuanLyPhanMem__63135414
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
