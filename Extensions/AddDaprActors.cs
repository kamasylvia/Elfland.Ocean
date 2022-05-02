using System.Reflection;
using System.Text.Json;
using Dapr.Actors.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Elfland.Ocean.Extensions;

public static partial class ProgramExtensions
{
    public static void AddDaprActors(this IServiceCollection services)
    {
        services.AddActors(
            options =>
            {
                options.JsonSerializerOptions = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };

                options.Actors.RegisterActors();
            }
        );
    }

    /// <summary>
    /// Registers all actors inherited from Actor type in the collection.
    /// </summary>
    /// <param name="actorRegistrationCollection">A collection of ActorRegistration instances.</param>
    public static void RegisterActors(this ActorRegistrationCollection actorRegistrationCollection)
    {
        var genericMethodInfo = actorRegistrationCollection
            .GetType()
            .GetMethod("RegisterActor", BindingFlags.Public);

        AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(
                implementationType =>
                    implementationType.IsSubclassOf(typeof(Actor))
                    && implementationType.IsClass
                    && !implementationType.IsAbstract
            )
            .ToList()
            .ForEach(
                implementationType =>
                    genericMethodInfo
                        ?.MakeGenericMethod(implementationType)
                        ?.Invoke(actorRegistrationCollection, new object?[] { null })
            );
    }
}
