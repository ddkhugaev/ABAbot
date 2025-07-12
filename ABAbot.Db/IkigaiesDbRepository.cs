using ABAbot.Db.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABAbot.Db
{
    public class IkigaiesDbRepository : IIkigaiesRepository
    {
        readonly DatabaseContext databaseContext;

        public IkigaiesDbRepository(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task AddAsync(Ikigai ikigai)
        {
            await databaseContext.Ikigaies.AddAsync(ikigai);
            await databaseContext.SaveChangesAsync();
        }

        public async Task<List<Ikigai>> GetAllAsync()
        {
            return await databaseContext.Ikigaies
                .ToListAsync();
        }

        public async Task RemoveAsync(Ikigai ikigai)
        {
            databaseContext.Ikigaies.Remove(ikigai);
            await databaseContext.SaveChangesAsync();
        }

        public async Task<Ikigai?> TryGetByIdAsync(long id)
        {
            return await databaseContext.Ikigaies
                .FirstOrDefaultAsync(ikigai => ikigai.Id == id);
        }
    }
}
