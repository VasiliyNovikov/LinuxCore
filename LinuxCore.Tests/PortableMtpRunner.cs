using System.Threading.Tasks;

using Microsoft.Testing.Platform.Builder;

namespace LinuxCore.Tests;

internal static class PortableMtpRunner
{
    public static async Task<int> Main(string[] args)
    {
        var options = new TestApplicationOptions();
        options.Configuration.ConfigurationSources.RegisterEnvironmentVariablesConfigurationSource = false;

        var builder = await TestApplication.CreateBuilderAsync(args, options);
        SelfRegisteredExtensions.AddSelfRegisteredExtensions(builder, args);

        using var app = await builder.BuildAsync();
        return await app.RunAsync();
    }
}