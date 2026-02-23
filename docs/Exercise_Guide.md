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

<!-- HOOK:todo.comuni.istanze.audit:START -->
TASK 3 - Audit istanziazioni `Comuni` (superflue + errori di compilazione)

Obiettivo finale:
1. Trovare tutte le `new Comuni()` nel progetto.
2. Capire quali sono superflue.
3. Sostituire gli usi errati con il servizio già iniettato (`ComuniStorageService`).
4. Tornare a compilazione pulita sugli errori bloccanti.

Contesto importante:
- La classe `Comuni` ha costruttore con parametro obbligatorio:
  - `public Comuni(IWebHostEnvironment env)`
- Quindi `new Comuni()` senza argomenti produce errore.

PASSO 1 - Trova tutte le istanziazioni
Apri terminale nella root del progetto ed esegui:

```powershell
rg -n "new\\s+Comuni\\(" src/PeopleExercise.Web
```

Devi trovare (al momento):
1. `src/PeopleExercise.Web/Funzioni/Utility.cs`
2. `src/PeopleExercise.Web/Components/Pages/People.razor.cs` (più di un punto)

PASSO 2 - Rimuovi l'istanza superflua in `Utility`
File: `src/PeopleExercise.Web/Funzioni/Utility.cs`

Rimuovi:

```csharp
Comuni comuni = new Comuni();
```

Perché è superflua:
- `Utility` non deve possedere dipendenze file/hosting.
- la funzione `test()` chiama `comuni.CalcoloComuneAsync()` ma non restituisce nulla utile.

PASSO 3 - Rimuovi o commenta il metodo `test()` non usato
Stesso file (`Utility.cs`), rimuovi:

```csharp
public async Task test()
{
    comuni.CalcoloComuneAsync();
}
```

Se vuoi tenerlo come promemoria studente, commentalo invece di eliminarlo.

PASSO 4 - Correggi `People.razor.cs` (prima istanza)
File: `src/PeopleExercise.Web/Components/Pages/People.razor.cs`

Nel metodo `CalcoloCodiceFiscale(Persona persona)`:
- elimina `Comuni comuni = new();`
- elimina `myUtility.test();`
- sostituisci il caricamento comuni con il servizio:

```csharp
var dizionarioComuni = await ComuniStorageService.GetComuniAsync();
if (!dizionarioComuni.TryGetValue(persona.LuogoDiNascita, out var codiceComune))
{
    Snackbar.Add("Comune non trovato nel file comuni.", Severity.Warning);
    return;
}
```

e usa `codiceComune` al posto di `dizionarioComuni["Arezzo"]`.

PASSO 5 - Correggi `People.razor.cs` (seconda istanza)
Nel metodo `OnDatiCambiati()`:
- elimina:

```csharp
var comuniSvc = new Comuni();
var dizionario = comuniSvc.CalcoloComuneAsync();
```

- sostituisci con accesso a `ComuniStorageService` (stesso schema del passo 4).

Nota:
- `OnDatiCambiati()` usa dati esterni, quindi conviene portarlo ad `async Task` e gestire chiamate async.

PASSO 6 - Allinea firma async dove necessario
Se `CalcoloCodiceFiscale` chiama `ComuniStorageService.GetComuniAsync()`, aggiorna la firma:

```csharp
public async Task CalcoloCodiceFiscale(Persona persona)
{
    // logica
}
```

Se è richiamato da UI (`OnClick`), usa:

```razor
OnClick="@(() => CalcoloCodiceFiscale(context))"
```

MudBlazor gestisce `Task` come callback async.

PASSO 7 - Verifica compilazione
Esegui:

```powershell
dotnet msbuild src/PeopleExercise.Web/PeopleExercise.Web.csproj /t:Compile /nologo
```

Risultato atteso:
- spariscono gli errori `CS7036` legati a `Comuni.Comuni(IWebHostEnvironment)`.

CHECK FINALE
1. Nessuna `new Comuni()` rimasta.
2. Recupero comuni solo via `ComuniStorageService`.
3. Nessun hardcode `"Arezzo"` nel calcolo codice fiscale.
4. Build senza errori bloccanti su `Comuni`.
<!-- HOOK:todo.comuni.istanze.audit:END -->

<!-- HOOK:todo.comuni.render.performance:START -->
TASK 4 - Perché `i` cresce sempre e perché la pagina rallenta con i comuni

Obiettivo:
1. Capire il problema reale del rallentamento.
2. Capire perché `i` sembra crescere all'infinito.
3. Applicare la soluzione corretta (`MudAutocomplete` con ricerca filtrata).

Spiegazione chiara del problema:
1. Nel codice c'era una `MudSelect` con un ciclo su `_nomiComuni`:
   - i comuni sono circa 7900+ elementi;
   - ogni elemento genera un `MudSelectItem`.
2. Blazor ricalcola il markup ad ogni render (input, validazione, cambi stato, ecc.).
3. Quindi il ciclo viene rieseguito spesso e il contatore `i` viene incrementato tante volte:
   - non è uno stato globale che esplode;
   - è il risultato di molti render + ciclo molto grande.
4. Cambiare il testo da `@nomeComune` a `@i` NON risolve:
   - il costo resta il render di migliaia di item.
5. Il vero fix è ridurre i nodi renderizzati:
   - usare `MudAutocomplete`;
   - mostrare solo risultati filtrati (es. massimo 50).

PASSO 1 - Sostituisci `MudSelect` con `MudAutocomplete`
File: `src/PeopleExercise.Web/Components/Pages/People.razor`

Nel ramo `LuogoDiNascita`, usa questo blocco:

```razor
<MudAutocomplete T="string"
                 Label="@propertyMetadata.DisplayLabel"
                 Value="@_currentPerson.LuogoDiNascita"
                 ValueChanged="OnLuogoDiNascitaChanged"
                 SearchFunc="SearchComuniAsync"
                 MinCharacters="2"
                 MaxItems="50"
                 DebounceInterval="250"
                 Clearable="true"
                 ResetValueOnEmptyText="true"
                 Required="true" />
```

Rimuovi:
- `int i = 0;`
- il `foreach` interno con `MudSelectItem`.

PASSO 2 - Aggiungi limite suggerimenti
File: `src/PeopleExercise.Web/Components/Pages/People.razor.cs`

Aggiungi nel blocco campi:

```csharp
private const int MaxComuniSuggestions = 50;
```

PASSO 3 - Aggiungi metodo filtro comuni
Stesso file (`People.razor.cs`), aggiungi:

```csharp
private Task<IEnumerable<string>> SearchComuniAsync(string value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        IEnumerable<string> defaultResults = _nomiComuni.Take(MaxComuniSuggestions);
        return Task.FromResult(defaultResults);
    }

    IEnumerable<string> filteredResults = _nomiComuni
        .Where(nome => nome.Contains(value, StringComparison.OrdinalIgnoreCase))
        .Take(MaxComuniSuggestions);

    return Task.FromResult(filteredResults);
}
```

PASSO 4 - Aggancia il cambio valore del comune
Sempre in `People.razor.cs`, aggiungi:

```csharp
private async Task OnLuogoDiNascitaChanged(string? value)
{
    _currentPerson.LuogoDiNascita = value ?? string.Empty;
    await OnDatiCambiati();
}
```

Questo mantiene il trigger del calcolo codice fiscale quando cambia il comune.

PASSO 5 - Verifica compilazione
Esegui:

```powershell
dotnet msbuild src/PeopleExercise.Web/PeopleExercise.Web.csproj /t:Compile /nologo
```

CHECK FINALE
1. Apri `/people`.
2. Vai su `Nuova Persona`.
3. Campo `LuogoDiNascita` deve essere autocomplete (non tendina enorme).
4. Scrivendo ad esempio `monte` la lista deve essere veloce.
5. Non deve più comparire il comportamento del contatore `i`.
6. Se compili i campi necessari, il calcolo CF deve continuare a funzionare.
<!-- HOOK:todo.comuni.render.performance:END -->
