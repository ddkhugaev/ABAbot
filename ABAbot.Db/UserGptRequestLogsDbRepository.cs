using ABAbot.Db.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABAbot.Db
{
    public class UserGptRequestLogsDbRepository : IUserGptRequestLogsDbRepository
    {
        readonly DatabaseContext databaseContext;

        public UserGptRequestLogsDbRepository(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task AddAsync(UserGptRequestLog userRequest)
        {
            await databaseContext.UserGptRequestLogs.AddAsync(userRequest);
            await databaseContext.SaveChangesAsync();
        }

        public async Task<List<UserGptRequestLog>> GetAllAsync()
        {
            return await databaseContext.UserGptRequestLogs
                .ToListAsync();
        }

        public async Task RemoveAsync(UserGptRequestLog user)
        {
            databaseContext.UserGptRequestLogs.Remove(user);
            await databaseContext.SaveChangesAsync();
        }

        public async Task<UserGptRequestLog?> TryGetByIdAsync(long id)
        {
            return await databaseContext.UserGptRequestLogs
                .FirstOrDefaultAsync(user => user.Id == id);
        }
    }
}
