using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnTraid.DTO
{
    /// <summary>
    /// Класс для хранения фундаментальных показателей компании
    /// </summary>
    public class FinancialData
    {
        public string Ticker { get; set; }
        public string CompanyName { get; set; }
        public DateTime ParseDate { get; set; }
        public List<FinancialMetric> Metrics { get; set; }

        public FinancialData()
        {
            Metrics = new List<FinancialMetric>();
            ParseDate = DateTime.Now;
        }
    }

    /// <summary>
    /// Класс для хранения отдельной финансовой метрики
    /// </summary>
    public class FinancialMetric
    {
        public string Name { get; set; }
        public List<string> Values { get; set; }
        public List<int> Years { get; set; }

        public FinancialMetric()
        {
            Values = new List<string>();
            Years = new List<int>();
        }

        public FinancialMetric(string name, List<string> values) : this()
        {
            Name = name;
            Values = values ?? new List<string>();
        }
    }
}
