using HtmlAgilityPack;
using TenderScanner.Data;
using TenderScanner.Infrastracture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenderScanner.Parsers
{
    public class TradeParser : IParser<TenderData>
    {
        private const string url = "http://www.trade.su/search/page{0}?find_data_dat=on&startdate=2014-1-1&stopdate=2014-6-11&dat_startdate={0:yyyy-MM-dd}&dat_stopdate={1:yyyy-MM-dd}&ext=1&and_or=1&sort_type=up";

        private int GetPageCount(HtmlNode htmlNode)
        {
            int count = 0;
            if (htmlNode != null)
            {
                var firstTable = htmlNode
                    .Descendants("table")
                    .FirstOrDefault(p => p.Attributes.Contains("class") && p.Attributes["class"].Value.Equals("sortquery"));

                if (firstTable != null)
                {
                    var searchCount = firstTable.Descendants("td").FirstOrDefault();
                    if (searchCount != null)
                    {
                        var line = searchCount.InnerText.Split(':');
                        if (line.Length == 2)
                        {
                            var number = line[1].Split('\n');
                            if (number != null)
                            {
                                int.TryParse(number[0], out count);
                            }
                        }
                    }
                }
            }

            return count;
        }

        private IEnumerable<TenderData> Mapping(IEnumerable<HtmlNode> htmlNodes)
        {
            foreach (var node in htmlNodes)
            {
                var tender = new TenderData
                {
                    Title = node.InnerText
                    //FullUrl = new Uri(node.Descendants("a").Attributes.Single(p => p.Value.Equals("href")).Value)
                };

                yield return tender;
            }
        }

        public async Task<IEnumerable<TenderData>> GetAsync()
        {
            var tenderList = new List<TenderData>();

            int count = 0;
            for (int index = 0;; index += 20)
            {
                string fullUrl = url.GetUrlWithPage(DateTime.Today.Date, DateTime.Today.Date, index);
                var htmlDocument = await ParseHelper.LoadAsync(fullUrl);
                var divTags = htmlDocument
                    .DocumentNode
                    .Descendants("div")
                    .SingleOrDefault(p => p.Id.Contains("results"));

                if (index == 0)
                {
                    count = GetPageCount(divTags);
                }

                var trTags = divTags
                    .Descendants("tr");

                tenderList.AddRange(Mapping(trTags));

                if (index == count) break;
            }

            return tenderList;
        }
    }
}
