using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnTraid.DTO
{
    public enum RSIState
    {
        Unknown,
        Oversold,    // Перепроданность
        Normal,      // Нормальное состояние
        Overbought   // Перекупленность
    }
}
