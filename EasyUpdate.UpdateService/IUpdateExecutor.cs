using EasyUpdate.Data;
using EasyUpdate.Data.Entities;

namespace EasyUpdate.UpdateService
{
    public interface IUpdateExecutor
    {
        Task ExecuteUpdateAsync(ScheduledUpdate update, AppDbContext context, CancellationToken ct);
    }
}