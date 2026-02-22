# Guida Modifiche - PeopleExercise.Web

## Obiettivo
Questa guida riassume passo passo le modifiche fatte per:

1. Rendere `codiceFiscale` visibile ma non editabile nel form.
2. Gestire correttamente i messaggi di errore custom dei campi required.
3. Spiegare dove sono state fatte le modifiche e come riprodurle.

## File coinvolti

- `src/PeopleExercise.Web/Services/PersonPropertyMetadata.cs`
- `src/PeopleExercise.Web/Services/ReflectionPersonSchemaService.cs`
- `src/PeopleExercise.Web/Components/Pages/People.razor.cs`
- `src/PeopleExercise.Web/Components/DynamicField.razor`
- `src/PeopleExercise.Web/Components/Pages/People.razor`
- `src/PeopleExercise.Web/Models/Persona.cs`

## Step 1 - Separare visibilita ed editabilita nel metadata

### File: `src/PeopleExercise.Web/Services/PersonPropertyMetadata.cs`

Sono stati aggiunti/tenuti i flag per distinguere comportamento UI:

- `IsVisible`: decide se il campo deve apparire nel form.
- `IsEditable`: decide se il campo e modificabile o in sola lettura.
- `RequiredErrorMessage`: contiene il testo custom del `[Required]`.

Esempio attuale:

```csharp
public required bool IsEditable { get; init; }
public required bool IsVisible { get; init; }
public required bool IsRequired { get; init; }
public string? RequiredErrorMessage { get; init; }
```

## Step 2 - Popolare i metadata via reflection

### File: `src/PeopleExercise.Web/Services/ReflectionPersonSchemaService.cs`

Nel `BuildSchema()` sono stati impostati i campi chiave:

- `IsVisible = property.Name != nameof(Persona.Id)`
- `IsEditable = property.Name != nameof(Persona.Id) && property.Name != nameof(Persona.codiceFiscale)`
- `IsRequired = property.GetCustomAttribute<RequiredAttribute>() is not null`
- `RequiredErrorMessage = property.GetCustomAttribute<RequiredAttribute>()?.ErrorMessage`

Con questa logica:

- `Id` non viene mostrato nel form.
- `codiceFiscale` viene mostrato ma non editato.
- il messaggio custom del required viene letto dal modello.

## Step 3 - Mostrare nel form i campi visibili (non solo editabili)

### File: `src/PeopleExercise.Web/Components/Pages/People.razor.cs`

In inizializzazione:

```csharp
_visibleProperties = properties.Where(propertyMetadata => propertyMetadata.IsVisible).ToArray();
```

Questa modifica e fondamentale per vedere `codiceFiscale` nel form:

- se filtrassi con `IsEditable`, `codiceFiscale` sparirebbe.
- filtrando con `IsVisible`, il campo resta visibile.

### File: `src/PeopleExercise.Web/Components/Pages/People.razor`

Nel form si itera su `_visibleProperties`:

```razor
@foreach (var propertyMetadata in _visibleProperties)
{
    <DynamicField Model="_currentPerson" PropertyMetadata="propertyMetadata" />
}
```

## Step 4 - Rendere `codiceFiscale` non editabile nel controllo

### File: `src/PeopleExercise.Web/Components/DynamicField.razor`

Nel controllo stringa:

```razor
ReadOnly="@(!PropertyMetadata.IsEditable)"
```

Quindi quando `IsEditable == false` (come per `codiceFiscale`) il campo e in sola lettura.

## Step 5 - Messaggi required custom: perche compariva "Required"

### Problema visto
Anche avendo `ErrorMessage` nel modello (`Persona.cs`), la UI mostrava "Required".

### Motivo
`MudTextField` usa il parametro `RequiredError` per la validazione interna `Required`, non `ErrorText`.

### Fix applicato
In `DynamicField.razor`:

```razor
Required="@PropertyMetadata.IsRequired"
RequiredError="@PropertyMetadata.RequiredErrorMessage"
Error="@HasErrors"
ErrorText="@FirstError"
```

Ruoli:

- `RequiredError`: testo della validazione built-in Mud (`Required`).
- `ErrorText`: testo da DataAnnotations letto tramite `EditContext` (`FirstError`).

## Step 6 - Riferimento ai messaggi nel modello

### File: `src/PeopleExercise.Web/Models/Persona.cs`

I testi custom sono sugli attributi, per esempio:

```csharp
[Required(ErrorMessage = "Il Cognome e obbligatorio.")]
public string Cognome { get; set; } = string.Empty;
```

Stesso principio per `Nome`, `LuogoDiNascita`, `sesso`.

## Step 7 - Checklist rapida per riprodurre da zero

1. Aggiungi in `PersonPropertyMetadata` i campi: `IsVisible`, `IsEditable`, `IsRequired`, `RequiredErrorMessage`.
2. In `ReflectionPersonSchemaService` valorizza:
   - `IsVisible` escludendo `Id`.
   - `IsEditable` escludendo `codiceFiscale`.
   - `RequiredErrorMessage` da `RequiredAttribute.ErrorMessage`.
3. In `People.razor.cs` costruisci `_visibleProperties` filtrando con `IsVisible`.
4. In `People.razor` usa `_visibleProperties` nel `foreach` del form.
5. In `DynamicField.razor` imposta:
   - `ReadOnly="@(!PropertyMetadata.IsEditable)"`
   - `RequiredError="@PropertyMetadata.RequiredErrorMessage"`
6. Verifica in UI:
   - `codiceFiscale` visibile nel form.
   - `codiceFiscale` non editabile.
   - messaggi required personalizzati (non solo "Required").

## Nota tecnica
Nel metadata esiste ancora questa property:

```csharp
public Type NotEditable => ReadOnlyAttribute.Yes.GetType();
```

Non e usata nel flusso attuale. Se vuoi evitare ambiguita, puoi rimuoverla e usare solo `IsEditable`.
