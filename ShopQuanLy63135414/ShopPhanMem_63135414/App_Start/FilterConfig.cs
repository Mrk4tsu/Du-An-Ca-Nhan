using System.Web;
using System.Web.Mvc;

namespace ShopPhanMem_63135414
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
