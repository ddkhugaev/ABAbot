using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABAbot.Db.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<Ikigai> Ikigaies { get; set; }
    }
}
