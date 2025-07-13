using ABAbot.Db.Models;

namespace ABAbot.Db
{
    public interface IUserGptRequestLogsDbRepository
    {
        Task AddAsync(UserGptRequestLog userRequest);
        Task<List<UserGptRequestLog>> GetAllAsync();
        Task RemoveAsync(UserGptRequestLog user);
        Task<UserGptRequestLog?> TryGetByIdAsync(long id);
    }
}