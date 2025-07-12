using ABAbot.Db.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABAbot.Db
{
    public class UsersDbRepository : IUsersRepository
    {
        readonly DatabaseContext databaseContext;

        public UsersDbRepository(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task AddAsync(User user)
        {
            await databaseContext.Users.AddAsync(user);
            await databaseContext.SaveChangesAsync();
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await databaseContext.Users
                .ToListAsync();
        }

        public async Task RemoveAsync(User user)
        {
            databaseContext.Users.Remove(user);
            await databaseContext.SaveChangesAsync();
        }

        public async Task<User?> TryGetByIdAsync(long id)
        {
            return await databaseContext.Users
                .FirstOrDefaultAsync(user => user.Id == id);
        }
    }
}
