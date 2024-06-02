using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using RestEase;
using WireMock.Client;
using WireMock.Client.Extensions;

namespace WireMock.Net.Aspire;

internal class WireMockServerMappingBuilderHook : IDistributedApplicationLifecycleHook
{
    public async Task AfterResourcesCreatedAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken)
    {
        var wireMockInstances = appModel.Resources
            .OfType<WireMockServerResource>()
            .Where(i => i.Arguments.ApiMappingBuilder is not null)
            .ToArray();

        if (!wireMockInstances.Any())
        {
            return;
        }

        foreach (var wireMockInstance in wireMockInstances)
        {
            var endpoint = wireMockInstance.GetEndpoint("http");
            if (endpoint.IsAllocated)
            {
                var adminApi = RestClient.For<IWireMockAdminApi>(endpoint.Url);

                var mappingBuilder = adminApi.GetMappingBuilder();
                await wireMockInstance.Arguments.ApiMappingBuilder!.Invoke(mappingBuilder);
            }
        }
    }
}