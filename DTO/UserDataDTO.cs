using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnTraid.DTO
{

    public class UserDataDTO
    {
        public UserJsonDataDTO User { get; set; }
        public TelegramJsonDataDTO TelegramId { get; set; }
        public TInvestTokenDTO TInvestToken { get; set; }
    }

    public class UserJsonDataDTO
    {
        public string UserName { get; set; }
        public string Defalt { get; set; }
    }
    public class TelegramJsonDataDTO
    {
        public bool Status { get; set; }
        public string Id { get; set; }
        public string Defalt { get; set; }
    }
    public class TInvestTokenDTO
    {
        public bool Status { get; set; }
        public string Id { get; set; }
        public string Defalt { get; set; }
    }
}
    

