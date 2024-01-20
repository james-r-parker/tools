namespace DotnetHelp.DevTools.Shared;

public record IncomingEmail(
    IReadOnlyCollection<EmailAddress> To,
    IReadOnlyCollection<EmailAddress> From,
    IReadOnlyCollection<EmailHeader> Headers,
    IReadOnlyCollection<EmailContent> Content,
    string Subject,
    DateTimeOffset Created);

public record EmailAddress(string Name, string Address, string Domain);

public record EmailHeader(string Name, string Value);

public record EmailContent(string Id, string Type, string Content);