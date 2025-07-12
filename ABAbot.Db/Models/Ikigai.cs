using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABAbot.Db.Models
{
    public class Ikigai
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string WhatYouLove { get; set; }
        public string WhatYouAreGoodAt { get; set; }
        public string WhatYouCanBePaidFor { get; set; }
        public string WhatTheWorldNeeds { get; set; }
        public string GptAns { get; set; }
        public DateTime Date { get; set; }
    }
}
