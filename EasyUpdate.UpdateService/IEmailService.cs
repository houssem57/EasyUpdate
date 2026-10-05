using EasyUpdate.Data.Entities;

namespace EasyUpdate.UpdateService
{
    public interface IEmailService
    {
        Task SendUpdateFailureEmail(ScheduledUpdate update);
    }
}