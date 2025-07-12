using ABAbot.Db.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABAbot.Db
{
    public interface IIkigaiesRepository
    {
        Task AddAsync(Ikigai ikigai);
        Task<List<Ikigai>> GetAllAsync();
        Task RemoveAsync(Ikigai ikigai);
        Task<Ikigai?> TryGetByIdAsync(long id);
    }
}
