using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using StadiumOps.Application.Abstractions;

namespace StadiumOps.Infrastructure.Integrations;

public sealed class FirebaseNotificationGateway(IOptions<FirebaseOptions> options)
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

        if (string.IsNullOrWhiteSpace(config.ServiceAccountPath) || !File.Exists(config.ServiceAccountPath))
        {
            throw new ExternalIntegrationNotConfiguredException(
                "FirebaseCloudMessaging",
                "Firebase Cloud Messaging is not configured. Set Firebase:ServiceAccountPath to a valid service account JSON file.");
        }

        var credential = CredentialFactory
            .FromFile(config.ServiceAccountPath, JsonCredentialParameters.ServiceAccountCredentialType);

        var app = FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions
        {
            Credential = credential,
            ProjectId = config.ProjectId
        });

        var message = new Message
        {
            Fid = request.DeviceToken,
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

        var messageId = await FirebaseMessaging.GetMessaging(app).SendAsync(message, cancellationToken);
        return new NotificationDispatchResult(
            true,
            true,
            "FirebaseCloudMessaging",
            $"Delivered with Firebase message id {messageId}.");
    }
}
