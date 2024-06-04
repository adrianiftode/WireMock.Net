using System.Net.Http.Headers;
using System.Text;
using Stef.Validation;

// ReSharper disable once CheckNamespace
namespace WireMock.Client.Extensions;

/// <summary>
/// Some extensions for <see cref="IWireMockAdminApi"/>.
/// </summary>
internal static class WireMockAdminApiExtensions
{
    private const int MaxRetries = 5;
    private const int InitialWaitingTimeInMilliSeconds = 500;
    private const string HealthStatusHealthy = "Healthy";

    /// <summary>
    /// Set basic authentication to access the <see cref="IWireMockAdminApi"/>.
    /// </summary>
    /// <param name="adminApi">See <see cref="IWireMockAdminApi"/>.</param>
    /// <param name="username">The admin username.</param>
    /// <param name="password">The admin password.</param>
    /// <returns><see cref="IWireMockAdminApi"/></returns>
    public static IWireMockAdminApi WithAuthorization(this IWireMockAdminApi adminApi, string username, string password)
    {
        Guard.NotNull(adminApi);
        Guard.NotNullOrEmpty(username);
        Guard.NotNullOrEmpty(password);

        adminApi.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));
        return adminApi;
    }

    /// <summary>
    /// Wait for the WireMock.Net server to be healthy. (The "/__admin/health" returns "Healthy").
    /// </summary>
    /// <param name="adminApi">See <see cref="IWireMockAdminApi"/>.</param>
    /// <param name="maxRetries">The maximum number of retries. Default is <c>5</c>.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <returns>A completed Task in case the health endpoint is available, else throws a <see cref="InvalidOperationException"/>.</returns>
    public static async Task WaitForHealthAsync(this IWireMockAdminApi adminApi, int maxRetries = MaxRetries, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(adminApi);

        var retries = 0;
        var waitTime = TimeSpan.FromMilliseconds(InitialWaitingTimeInMilliSeconds);
        var isHealthy = await GetHealthAsync(adminApi, cancellationToken);
        while (!isHealthy && retries < MaxRetries && !cancellationToken.IsCancellationRequested)
        {
            waitTime = TimeSpan.FromMilliseconds((int)(InitialWaitingTimeInMilliSeconds * Math.Pow(2, retries)));
            await Task.Delay(waitTime, cancellationToken);
            isHealthy = await GetHealthAsync(adminApi, cancellationToken);
            retries++;
        }

        if (retries >= MaxRetries)
        {
            throw new InvalidOperationException($"The /__admin/health endpoint did not return 'Healthy' after {MaxRetries} retries and {waitTime.TotalSeconds:0.0} seconds.");
        }
    }

    private static async Task<bool> GetHealthAsync(IWireMockAdminApi adminApi, CancellationToken cancellationToken)
    {
        try
        {
            var status = await adminApi.GetHealthAsync(cancellationToken);
            return string.Equals(status, HealthStatusHealthy, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}