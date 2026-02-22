using System.ComponentModel;
using System.Reflection;

namespace PeopleExercise.Web.Services;

public sealed class PersonPropertyMetadata
{
    public required string PropertyName { get; init; }

    public required Type PropertyType { get; init; }

    public required bool IsEditable { get; init; }

    public required bool IsVisible { get; init; }

    public required string DisplayLabel { get; init; }

    public required bool IsRequired { get; init; }

    public string? RequiredErrorMessage {get; init; }
    
    public required PropertyInfo PropertyInfo { get; init; }
    public Type NotEditable => ReadOnlyAttribute.Yes.GetType();

    public bool IsNullable => Nullable.GetUnderlyingType(PropertyType) is not null;

    public Type EffectiveType => Nullable.GetUnderlyingType(PropertyType) ?? PropertyType;
}
