using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using AzureFuncSendEmail.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzureFuncSendEmail.Functions;

public class SendEmail(
    ILogger<SendEmail> logger,
    EmailClient emailClient,
    IConfiguration config)
{
    private readonly ILogger<SendEmail> _logger = logger;
    private readonly EmailClient _emailClient = emailClient;
    private readonly IConfiguration _config = config;


    [Function(nameof(SendEmail))]
    public async Task Run(
        [ServiceBusTrigger("SendEmail", Connection = "EsbConnection")]
        ServiceBusReceivedMessage receivedMessage,
        ServiceBusMessageActions messageActions)
    {

        if (receivedMessage.Subject == null || receivedMessage.Body == null)
        {
            _logger.LogWarning($"Message ID: {receivedMessage.MessageId}");
            _logger.LogWarning("Message is empty, can not send email");
            return;
        }

        try
        {
            var sender = _config["AcsSenderAddress"];
            var emailRequest = JsonSerializer.Deserialize<EmailRequest>(receivedMessage.Body);

            var emailMessage = new EmailMessage(
                senderAddress: sender,
                recipientAddress: emailRequest.To,
                content: new EmailContent(receivedMessage.Subject)
                {
                    PlainText = emailRequest.TextContent,
                    Html = emailRequest.HtmlContent
                }
            );

            await _emailClient.SendAsync(WaitUntil.Started, emailMessage);
            _logger.LogInformation($"Email was sent successfully to {emailRequest.To}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Message ID: {receivedMessage.MessageId}");
            _logger.LogWarning($"Fail to send Email: {ex}");
            return;
        }

        await messageActions.CompleteMessageAsync(receivedMessage);
    }
}