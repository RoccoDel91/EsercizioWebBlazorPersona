using System.ComponentModel.DataAnnotations;
using System.Reflection;
using PeopleExercise.Web.Models;

namespace PeopleExercise.Web.Services;

public sealed class ReflectionPersonSchemaService : IPersonSchemaService
{
    private static readonly HashSet<Type> SupportedPropertyTypes =
    [
        typeof(string),
        typeof(int),
        typeof(decimal),
        typeof(DateTime),
        typeof(bool)
    ];

    public IReadOnlyList<PersonPropertyMetadata> GetPersonProperties() => BuildSchema();

    private static IReadOnlyList<PersonPropertyMetadata> BuildSchema()
    {
        return typeof(Persona)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.CanRead && property.CanWrite)
            .Where(property => SupportedPropertyTypes.Contains(Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType))
            .Select(property => new PersonPropertyMetadata
            {
                PropertyName = property.Name,
                PropertyType = property.PropertyType,
                IsEditable = property.Name != nameof(Persona.Id),
                DisplayLabel = ResolveDisplayLabel(property),
                IsRequired = property.GetCustomAttribute<RequiredAttribute>() is not null,
                PropertyInfo = property
            })
            .ToArray();
    }

    private static string ResolveDisplayLabel(PropertyInfo propertyInfo)
    {
        var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();
        if (!string.IsNullOrWhiteSpace(displayAttribute?.Name))
        {
            return displayAttribute.Name;
        }

        return propertyInfo.Name;
    }
}
