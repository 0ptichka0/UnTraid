using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnTraid.DTO
{
    /// <summary>
    /// Результат вычисления TRIX индикатора
    /// </summary>
    public class TRIXResult
    {
        public double[] TRIX { get; set; }      // Основная TRIX линия
        public double[] Signal { get; set; }    // Сигнальная линия (EMA от TRIX)
    }
}
