using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABAbot.Db.Models
{
    public class UserGptRequestLog
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public DateTime Date { get; set; }
        public string UserRequest { get; set; }
        public string GptAnswer { get; set; }
    }
}
