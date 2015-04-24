using TenderScanner.Data;
using TenderScanner.Infrastracture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Diagnostics;

namespace TenderScanner.Parsers
{
    public class B2BParser : IParser<TenderData>
    {
        private const string url = "http://www.b2b-center.ru/market/?f_keyword=&organizer=&organizer_prev=&date=1&date_start_dmy={0:dd.MM.yyyy}&date_end_dmy={1:dd.MM.yyyy}&price_start=&price_end=&price_currency=0&with_map=0&map_country=643_77&trade=buy&advanced=on&from={2}";

        private int GetPageCount(HtmlNode htmlNode)
        {
            int count = 0;
            if (htmlNode != null)
            {
                var lastLink = htmlNode.Descendants("a")
                    .Where(p => p.Attributes.Contains("title") && !p.Attributes.Contains("rel"))
                    .LastOrDefault();

                if (lastLink != null)
                {
                    int.TryParse(lastLink.InnerText, out count);
                }
            }

            return count;
        }

        private IEnumerable<TenderData> Mapping(IEnumerable<HtmlNode> htmlNodes)
        {
            foreach (var node in htmlNodes)
            {
                var tender = new TenderData();

                try
                {                   
                    var aTag = node.Descendants("a").FirstOrDefault();
                    if (aTag != null)
                    {                        
                        var bTag = aTag.Descendants("b").FirstOrDefault();
                        if (bTag != null)
                        {
                            var lines = bTag.InnerText.Split(' ');
                            tender.Number = lines.LastOrDefault();
                        }

                        var text = aTag.InnerHtml.Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
                        if (text.Length >= 2)
                        {
                            tender.Title = text[1];
                        }

                        if (String.IsNullOrEmpty(tender.Title))
                        {
                            throw new Exception(tender.Number);
                        }

                        //tender.FullUrl = new Uri(aTag.Attributes.Single(p => p.Value.Equals("href")).Value);
                    }
                }
                catch
                {
                    Debug.WriteLine(node);

                    tender.Title = "Ошибка";
                }

                yield return tender;
            }
        }

        public async Task<IEnumerable<TenderData>> GetAsync()
        {
            var tenderList = new List<TenderData>();

            int count = 0;
            for (int pageIndex = 0, index = 0; ; pageIndex += 20, index++)
            {
                string fullUrl = url.GetUrlWithPage(DateTime.Today.Date, DateTime.Today.Date, pageIndex);
                var htmlDocument = await ParseHelper.LoadAsync(fullUrl);
                var divTags  = htmlDocument
                    .DocumentNode
                    .Descendants("div")
                    .SingleOrDefault(p => p.Id.Contains("searchresult"));

                if (index == 0)
                {
                    count = GetPageCount(divTags);
                }

                var trTags = divTags
                    .Descendants("tr")
                    .Where(p => p.Attributes.Contains("class") &&  p.Attributes["class"].Value.Equals("c1"));

                tenderList.AddRange(Mapping(trTags));

                if (index == count) break;
            }

            return tenderList;
        }
    }
}
