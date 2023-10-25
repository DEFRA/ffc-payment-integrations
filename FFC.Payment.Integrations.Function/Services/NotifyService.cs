using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Notify.Interfaces;
using Notify.Models.Responses;

namespace FFC.Payment.Integrations.Function.Services;

/// <summary>
/// Notify Service for sending emails
/// </summary>
public class NotifyService : INotifyService
{
    private readonly INotificationClient _notifyServiceClient;

    /// <summary>
    /// Constructor for NotifyService
    /// </summary>
    /// <param name="notifyServiceClient"></param>
    public NotifyService(INotificationClient notifyServiceClient)
    {
        _notifyServiceClient = notifyServiceClient;
    }

    /// <inheritdoc />
    public EmailNotificationResponse SendEmail(string email, string templateId, dynamic messagePersonalisation)
    {
        Dictionary<string, dynamic> personalisation = new();

        foreach (var j in messagePersonalisation)
        {
            var jp = (JProperty)j;
            var j1 = jp.Value;

            personalisation.Add(jp.Name, j1.ToString());
        }

        return _notifyServiceClient.SendEmail(
                    email,
                    templateId,
                    personalisation
                );
    }
}
