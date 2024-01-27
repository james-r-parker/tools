namespace DotnetHelp.DevTools.Email.Handler;

public interface IEmailEventHandler
{
    ValueTask ProcessEmailAsync(string bucket, string key, CancellationToken cancellationToken);
}