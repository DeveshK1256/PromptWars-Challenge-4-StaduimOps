using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Options;
using StadiumOps.Application.Abstractions;

namespace StadiumOps.Infrastructure.Integrations;

public sealed class FirebaseNotificationGateway(
    IOptions<FirebaseOptions> options,
    FirebaseApp? firebaseApp = null)
    : INotificationGateway
{
    public async Task<NotificationDispatchResult> SendAsync(
        NotificationDispatchRequest request,
        CancellationToken cancellationToken)
    {
        var config = options.Value;
        if (string.IsNullOrWhiteSpace(request.DeviceToken))
        {
            return new NotificationDispatchResult(
                false,
                false,
                "FirebaseCloudMessaging",
                "No device token was supplied, so only the in-app notification record was created.");
        }

        if (firebaseApp is null)
        {
            throw new ExternalIntegrationNotConfiguredException(
                "FirebaseCloudMessaging",
                "Firebase Cloud Messaging is not configured. Set Firebase:ServiceAccountPath to a valid service account JSON file.");
        }

        var message = new Message
        {
            Token = request.DeviceToken,
            Notification = new Notification
            {
                Title = request.Title,
                Body = request.Body
            },
            Data = new Dictionary<string, string>
            {
                ["priority"] = request.Priority
            }
        };

        var messageId = await FirebaseMessaging.GetMessaging(firebaseApp).SendAsync(message, cancellationToken);
        return new NotificationDispatchResult(
            true,
            true,
            "FirebaseCloudMessaging",
            $"Delivered with Firebase message id {messageId}.");
    }
}
