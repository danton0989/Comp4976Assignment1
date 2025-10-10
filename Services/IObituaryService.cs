using ObituaryApp.Models;

namespace ObituaryApp.Services;

public interface IObituaryService
{
    Task<Obituary> CreateAsync(Obituary obituary);
    Task<Obituary?> GetByIdAsync(int id);
    Task<List<Obituary>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<bool> UpdateAsync(Obituary obituary);
    Task<bool> DeleteAsync(int id);
}
