using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Take_Home_Tutor_2._0.Helpers
{
    public static class HtmlHelpers
    {
        public static bool IsDebug(this HtmlHelper htmlHelper)
        {
        #if DEBUG
                    return true;
        #else
                        return false;
        #endif
        }
    }
}
