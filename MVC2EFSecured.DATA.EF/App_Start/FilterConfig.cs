using System.Web;
using System.Web.Mvc;

namespace MVC2EFSecured.DATA.EF
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
