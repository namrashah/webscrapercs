using System;
using System.Collections.Generic;
using System.Text;

namespace WebScraperModularized.data
{
    public class NTPICategory
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public List<NTPI> NtpiList { get; set; }
    }
}
