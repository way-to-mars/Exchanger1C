using System;

namespace Exchanger
{
    public class Transaction
    {
        public string DocumentInfo { get; set; }    
        public DateTime? DateOut { get; set; }
        public string Name { get; set; }
        public string InnKpp { get; set; }
        public string Account { get; set; }
        public string Bank { get; set; }
        public double? Debet { get; set; }
        public double? Credit { get; set; }
        public string Description { get; set; }
        public string SourceRaw { get; set; }
    }
}
