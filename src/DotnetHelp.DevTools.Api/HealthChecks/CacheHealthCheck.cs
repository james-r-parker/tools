using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DotnetHelp.DevTools.Api.HealthChecks;

public class CacheHealthCheck(IDistributedCache cache) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            var value = Guid.NewGuid().ToString();
            await cache.SetStringAsync("health", value, cancellationToken);
            string? result = await cache.GetStringAsync("health", cancellationToken);
            await cache.RemoveAsync("health", cancellationToken);

            if (result is null)
            {
                return HealthCheckResult.Unhealthy();
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}