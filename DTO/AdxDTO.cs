using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnTraid.DTO
{
    /// <summary>
    /// Результат вычисления направленных движений
    /// </summary>
    public class DirectionalMovementResult
    {
        public double[] PlusDM { get; set; }    // +DM значения
        public double[] MinusDM { get; set; }   // -DM значения
    }

    /// <summary>
    /// Результат вычисления ADX индикатора
    /// </summary>
    public class ADXResult
    {
        public double[] ADX { get; set; }       // Основная ADX линия
        public double[] PlusDI { get; set; }    // +DI линия (бычье направление)
        public double[] MinusDI { get; set; }   // -DI линия (медвежье направление)
    }
}
