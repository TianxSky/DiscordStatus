namespace DiscordStatus
{
    using CounterStrikeSharp.API.Core;
    using Microsoft.Extensions.DependencyInjection;

    public class PluginServices : IPluginServiceCollection<DiscordStatus>
    {
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Mark DiscordWebhook as transient
            serviceCollection.AddSingleton<Globals>();
            serviceCollection.AddTransient<IWebhook, Webhook>();
            serviceCollection.AddSingleton<IQuery, Query>();
            serviceCollection.AddSingleton<IChores, Chores>();
        }
    }
}