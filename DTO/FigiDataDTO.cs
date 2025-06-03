using System.Collections.Generic;

namespace UnTraid.DTO
{
    public class FigiDataDTO
    {
        public List<FigiData> Data { get; set; }
    }

    public class FigiData
    {
        public string Ticker { get; set; }
        public string Name { get; set; }
    }
}
