# Guida Esercizio - C# + MudBlazor (Principiante)

## Obiettivo
Costruire un CRUD dinamico per `Persona` dove:
- i campi del form sono generati via reflection;
- le colonne della griglia sono generate via reflection;
- ogni persona e salvata come file JSON in una cartella locale.

## Struttura del Progetto
- `src/PeopleExercise.Web/Models/Persona.cs`
- `src/PeopleExercise.Web/Configuration/StorageOptions.cs`
- `src/PeopleExercise.Web/Services/IPersonStorageService.cs`
- `src/PeopleExercise.Web/Services/FilePersonStorageService.cs`
- `src/PeopleExercise.Web/Services/IPersonSchemaService.cs`
- `src/PeopleExercise.Web/Services/ReflectionPersonSchemaService.cs`
- `src/PeopleExercise.Web/Components/DynamicField.razor`
- `src/PeopleExercise.Web/Components/Pages/People.razor`

## Punti di Controllo
- [ ] Crea una nuova persona da UI e verifica che esista il file `{Id}.json`.
- [ ] Riavvia l'app e verifica che le persone vengano caricate da disco.
- [ ] Modifica una persona e verifica che il JSON venga aggiornato.
- [ ] Elimina una persona e verifica che il file JSON venga rimosso.
- [ ] Aggiungi una nuova proprieta in `Persona` (tipo supportato) e verifica che compaiano automaticamente nuovo campo e nuova colonna in griglia.

## Tipi Supportati (v1)
- `string`
- `int`
- `decimal`
- `DateTime`
- `bool`

## TODO per lo Studente
1. `FilePersonStorageService.SaveAsync`
- Tag TODO: `Implementare serializzazione in SaveAsync`.
- Obiettivo: capire il flusso di scrittura JSON e la gestione delle eccezioni.

2. `DynamicField.razor`
- Tag TODO: `Completare mapping type->Mud control`.
- Obiettivo: estendere la generazione UI per tipi aggiuntivi (per esempio `enum`).

3. `People.razor`
- Tag TODO: `Gestire eccezioni I/O con snackbar`.
- Obiettivo: standardizzare il feedback utente e il dettaglio errori.

## Percorso Storage Locale
Impostazione in `appsettings.json`:

```json
"Storage": {
  "PeopleDirectoryPath": "Data/People"
}
```

Se serve, puoi usare un percorso assoluto.

## Test Manuali di Robustezza
1. JSON corrotto:
- crea un file JSON non valido nella cartella people;
- apri `/people`;
- verifica che l'app non vada in crash e che compaia una snackbar di warning.

2. Cartella mancante:
- elimina `Data/People`;
- apri `/people`;
- verifica che la cartella venga ricreata.

3. Permessi:
- imposta il percorso storage su una directory non scrivibile;
- salva/elimina una persona;
- verifica che la UI mostri l'errore storage.

---

## Guida Step-by-Step: Modifiche Fatte (codiceFiscale + errori custom)

Questa sezione documenta gli step effettivamente implementati.

### Obiettivo A: `codiceFiscale` visibile ma non editabile

#### 1) Separare visibilita da editabilita nel metadata
File: `src/PeopleExercise.Web/Services/PersonPropertyMetadata.cs`

Verifica che esistano:

```csharp
public required bool IsVisible { get; init; }
public required bool IsEditable { get; init; }
```

#### 2) Impostare i flag in reflection
File: `src/PeopleExercise.Web/Services/ReflectionPersonSchemaService.cs`

```csharp
IsVisible = property.Name != nameof(Persona.Id),
IsEditable = property.Name != nameof(Persona.Id) && property.Name != nameof(Persona.codiceFiscale),
```

Effetto:
- `Id` resta nascosto nel form.
- `codiceFiscale` entra nel form ma con `IsEditable = false`.

#### 3) Popolare il form con i campi visibili (non con quelli editabili)
File: `src/PeopleExercise.Web/Components/Pages/People.razor.cs`

```csharp
_visibleProperties = properties.Where(propertyMetadata => propertyMetadata.IsVisible).ToArray();
```

File: `src/PeopleExercise.Web/Components/Pages/People.razor`

```razor
@foreach (var propertyMetadata in _visibleProperties)
{
    <DynamicField Model="_currentPerson" PropertyMetadata="propertyMetadata" />
}
```

#### 4) Rendere read-only il campo non editabile
File: `src/PeopleExercise.Web/Components/DynamicField.razor`

```razor
ReadOnly="@(!PropertyMetadata.IsEditable)"
```

Con questa regola, `codiceFiscale` e visibile ma non modificabile.

### Obiettivo B: mostrare testo errore custom invece di `Required`

#### Perche compariva `Required`
`MudTextField` usa il parametro `RequiredError` per il messaggio interno del `Required`.

Se imposti solo:
- `Required="..."`
- `ErrorText="..."`

puoi vedere ancora il default `Required` nei casi in cui sta parlando la validazione interna MudBlazor.

#### Fix applicato
File: `src/PeopleExercise.Web/Services/PersonPropertyMetadata.cs`

```csharp
public string? RequiredErrorMessage { get; init; }
```

File: `src/PeopleExercise.Web/Services/ReflectionPersonSchemaService.cs`

```csharp
RequiredErrorMessage = property.GetCustomAttribute<RequiredAttribute>()?.ErrorMessage,
```

File: `src/PeopleExercise.Web/Components/DynamicField.razor`

```razor
Required="@PropertyMetadata.IsRequired"
RequiredError="@PropertyMetadata.RequiredErrorMessage"
Error="@HasErrors"
ErrorText="@FirstError"
```

### Prima/Dopo (riassunto veloce)

PRIMA:

```razor
Required="@PropertyMetadata.IsRequired"
Error="@HasErrors"
ErrorText="@FirstError"
```

DOPO:

```razor
Required="@PropertyMetadata.IsRequired"
RequiredError="@PropertyMetadata.RequiredErrorMessage"
Error="@HasErrors"
ErrorText="@FirstError"
```

### Checklist di Verifica

1. Apri `/people` e clicca `Nuova Persona`.
2. Verifica che `codiceFiscale` appaia nel form.
3. Verifica che `codiceFiscale` sia read-only.
4. Lascia vuoti `Nome` e `Cognome` e verifica che i messaggi custom siano in italiano.
5. Conferma che `Id` non venga mostrato nel form.

### Nota Tecnica
Nel metadata c'e ancora:

```csharp
public Type NotEditable => ReadOnlyAttribute.Yes.GetType();
```

Non viene usata nel flusso corrente: il comportamento attivo si basa su `IsEditable`.

---

## Hook Task Nuovi ToDo

<!-- HOOK:todo.comuni.storage:START -->
TASK 1 - Comuni con storage service + tendina su LuogoDiNascita

Obiettivo finale:
1. Eliminare path assoluti dal codice.
2. Leggere i comuni con un servizio dedicato.
3. Mostrare `LuogoDiNascita` come lista selezionabile.

Lavora in questo ordine.

PASSO 1 - Crea l'interfaccia del servizio comuni
File nuovo: `src/PeopleExercise.Web/Services/IComuniStorageService.cs`

Copia e incolla:

```csharp
namespace PeopleExercise.Web.Services;

public interface IComuniStorageService
{
    Task<IReadOnlyDictionary<string, string>> GetComuniAsync();
    Task<IReadOnlyList<string>> GetNomiComuniAsync();
}
```

PASSO 2 - Crea il servizio file comuni
File nuovo: `src/PeopleExercise.Web/Services/FileComuniStorageService.cs`

Copia e incolla:

```csharp
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PeopleExercise.Web.Configuration;

namespace PeopleExercise.Web.Services;

public sealed class FileComuniStorageService : IComuniStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _comuniFilePath;

    public FileComuniStorageService(IOptions<StorageOptions> options, IWebHostEnvironment env)
    {
        var configuredPath = options.Value.ComuniDirectoryPath;
        _comuniFilePath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.GetFullPath(Path.Combine(env.ContentRootPath, configuredPath));
    }

    public async Task<IReadOnlyDictionary<string, string>> GetComuniAsync()
    {
        if (!File.Exists(_comuniFilePath))
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var json = await File.ReadAllTextAsync(_comuniFilePath, Encoding.UTF8);
        var comuni = JsonSerializer.Deserialize<List<ComuneJson>>(json, JsonOptions) ?? new List<ComuneJson>();

        return comuni
            .Where(c => !string.IsNullOrWhiteSpace(c.nome) && !string.IsNullOrWhiteSpace(c.codiceCatastale))
            .GroupBy(c => c.nome.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.First().codiceCatastale.Trim(),
                StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyList<string>> GetNomiComuniAsync()
    {
        var map = await GetComuniAsync();
        return map.Keys.OrderBy(k => k).ToList();
    }

    private sealed class ComuneJson
    {
        public string nome { get; set; } = string.Empty;
        public string codiceCatastale { get; set; } = string.Empty;
    }
}
```

PASSO 3 - Registra il servizio in DI
File: `src/PeopleExercise.Web/Program.cs`

Aggiungi:

```csharp
builder.Services.AddScoped<IComuniStorageService, FileComuniStorageService>();
```

PASSO 4 - Inietta il servizio nella pagina People
File: `src/PeopleExercise.Web/Components/Pages/People.razor`

Aggiungi negli `@inject`:

```razor
@inject IComuniStorageService ComuniStorageService
```

PASSO 5 - Carica i comuni in memoria
File: `src/PeopleExercise.Web/Components/Pages/People.razor.cs`

1. Aggiungi campo:

```csharp
private IReadOnlyList<string> _nomiComuni = Array.Empty<string>();
```

2. In `OnInitializedAsync()`, dopo il caricamento dello schema:

```csharp
_nomiComuni = await ComuniStorageService.GetNomiComuniAsync();
```

PASSO 6 - Sostituisci `LuogoDiNascita` con `MudSelect`
File: `src/PeopleExercise.Web/Components/Pages/People.razor`

Nel `foreach` dei campi, usa questa struttura:

```razor
@if (propertyMetadata.PropertyName == nameof(Persona.LuogoDiNascita))
{
    <MudSelect T="string"
               Label="@propertyMetadata.DisplayLabel"
               @bind-Value="_currentPerson.LuogoDiNascita"
               Required="true">
        @foreach (var nomeComune in _nomiComuni)
        {
            <MudSelectItem Value="@nomeComune">@nomeComune</MudSelectItem>
        }
    </MudSelect>
}
else
{
    <DynamicField Model="_currentPerson" PropertyMetadata="propertyMetadata" />
}
```

PASSO 7 - Usa il comune selezionato nel CF
File: `src/PeopleExercise.Web/Components/Pages/People.razor.cs`

Dentro il calcolo CF (dove ora usi `\"Arezzo\"`), metti:

```csharp
var dizionarioComuni = await ComuniStorageService.GetComuniAsync();
if (!dizionarioComuni.TryGetValue(persona.LuogoDiNascita, out var codiceComune))
{
    Snackbar.Add("Comune non trovato nel file comuni.", Severity.Warning);
    return;
}
```

Poi usa `codiceComune` nella composizione del codice fiscale.

CHECK FINALE
1. Avvia app.
2. Apri `/people`.
3. Nuova Persona.
4. `LuogoDiNascita` deve essere una tendina.
5. Nessun `File.ReadAllText(\"C:\\...\")` deve restare nel codice.
<!-- HOOK:todo.comuni.storage:END -->

<!-- HOOK:todo.codicefiscale.auto:START -->
TASK 2 - `codiceFiscale` auto compilato e senza errore required

Obiettivo finale:
1. Cliccando sul campo CF non deve apparire `Required`.
2. Deve apparire: `il codice fiscale si auto compila`.
3. Il CF parte in automatico quando i campi necessari sono compilati.

Campi necessari:
- `Nome`
- `Cognome`
- `DataNascita`
- `Eta`
- `LuogoDiNascita`
- `sesso`

PASSO 1 - Personalizza il rendering del campo CF
File: `src/PeopleExercise.Web/Components/DynamicField.razor`

Nel `MudTextField` dei `string`, aggiungi questi parametri:

```razor
Required="@(IsCodiceFiscaleField ? false : PropertyMetadata.IsRequired)"
RequiredError="@(IsCodiceFiscaleField ? null : PropertyMetadata.RequiredErrorMessage)"
HelperText="@(IsCodiceFiscaleField ? \"il codice fiscale si auto compila\" : null)"
```

Sempre nello stesso file, in `@code`, aggiungi:

```csharp
private bool IsCodiceFiscaleField =>
    PropertyMetadata.PropertyName == nameof(Persona.codiceFiscale);
```

PASSO 2 - Crea regola “campi completi”
File: `src/PeopleExercise.Web/Components/Pages/People.razor.cs`

Aggiungi:

```csharp
private static bool CanAutoCalcolareCodiceFiscale(Persona persona)
{
    return !string.IsNullOrWhiteSpace(persona.Nome)
        && !string.IsNullOrWhiteSpace(persona.Cognome)
        && persona.DataNascita != default
        && persona.Eta > 0
        && !string.IsNullOrWhiteSpace(persona.LuogoDiNascita)
        && !string.IsNullOrWhiteSpace(persona.sesso);
}
```

PASSO 3 - Crea metodo di trigger unico
Sempre in `People.razor.cs`:

```csharp
private async Task TryAutoCalcoloCodiceFiscaleAsync()
{
    if (!CanAutoCalcolareCodiceFiscale(_currentPerson))
        return;

    await CalcoloCodiceFiscale(_currentPerson);
}
```

PASSO 4 - Aggancia il trigger ai cambi campo
Sempre in `People.razor.cs`, aggiungi:

```csharp
private static readonly HashSet<string> TriggerCodiceFiscaleFields = new(StringComparer.Ordinal)
{
    nameof(Persona.Nome),
    nameof(Persona.Cognome),
    nameof(Persona.DataNascita),
    nameof(Persona.Eta),
    nameof(Persona.LuogoDiNascita),
    nameof(Persona.sesso)
};

private async void OnEditFieldChanged(object? sender, FieldChangedEventArgs args)
{
    if (!TriggerCodiceFiscaleFields.Contains(args.FieldIdentifier.FieldName))
        return;

    await TryAutoCalcoloCodiceFiscaleAsync();
    await InvokeAsync(StateHasChanged);
}
```

In `StartCreate()` e `StartEdit(...)`, subito dopo `new EditContext(...)`, aggiungi:

```csharp
_editContext.OnFieldChanged += OnEditFieldChanged;
```

PASSO 5 - Rendi async il metodo CF e salva il risultato nel model
Nel metodo CF, la parte finale deve scrivere:

```csharp
persona.codiceFiscale = codiceFiscale.ToUpperInvariant();
```

Se dentro usi servizi async (es. comuni), rendi il metodo:

```csharp
public async Task CalcoloCodiceFiscale(Persona persona)
{
    // logica
}
```

CHECK FINALE
1. Apri `/people`.
2. Clicca su `codiceFiscale`: deve apparire solo il testo guida.
3. Compila i 6 campi richiesti.
4. Verifica che il CF si auto compili.
<!-- HOOK:todo.codicefiscale.auto:END -->
