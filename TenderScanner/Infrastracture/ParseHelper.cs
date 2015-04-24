using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TenderScanner.Infrastracture
{
    public static class ParseHelper
    {
        private static Func<string, DateTime, DateTime, int, string> urlDynamic = (url, startDate, endDate, pageNumber) => String.Format(url, startDate, endDate, pageNumber);

        public static string GetUrlWithPage(this string url, DateTime startDate, DateTime endDate, int page)
        {
            return urlDynamic(url, startDate, endDate, page);
        }

        public static Task<HtmlDocument> LoadAsync(this string url)
        {
            var htmlWeb = new HtmlWeb();

            return htmlWeb.LoadFromWebAsync(url);
        }
    }
}
