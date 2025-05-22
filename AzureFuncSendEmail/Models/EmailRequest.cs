

namespace AzureFuncSendEmail.Models;

internal class EmailRequest
{
    public string To { get; set; } = null!;
    public string TextContent { get; set; } = null!;
    public string HtmlContent { get; set; } = null!;
}
