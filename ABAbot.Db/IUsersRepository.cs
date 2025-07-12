using ABAbot.Db.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABAbot.Db
{
    public interface IUsersRepository
    {
        Task AddAsync(User user);
        Task<List<User>> GetAllAsync();
        Task RemoveAsync(User user);
        Task<User?> TryGetByIdAsync(long id);
    }
}
