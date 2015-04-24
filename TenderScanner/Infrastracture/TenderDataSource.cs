using TenderScanner.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenderScanner.Infrastracture
{
    public class TenderDataSource
    {
        private ObservableCollection<TenderData> tenders = new ObservableCollection<TenderData>();
        public ObservableCollection<TenderData> Tenders
        {
            get
            {
                return this.tenders;
            }
        }

        public async Task<ObservableCollection<TenderData>> GetDataAsync(params IParser<TenderData>[] parsers)
        {
            var list = new List<TenderData>();
            foreach (var parser in parsers)
            {
                list.AddRange(await parser.GetAsync());

                await Task.Yield();
            }

            return new ObservableCollection<TenderData>(list);
        }
    }
}
