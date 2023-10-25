
using Notify.Models.Responses;

namespace FFC.Payment.Integrations.Function.Services;

/// <summary>
/// Interface for Notrify Service
/// </summary>
public interface INotifyService
{
    /// <summary>
    /// Send an email via GovNotify
    /// </summary>
    /// <param name="email"></param>
    /// <param name="templateId"></param>
    /// <param name="messagePersonalisation"></param>
    /// <returns></returns>
    EmailNotificationResponse SendEmail(string email, string templateId, dynamic messagePersonalisation);
}
