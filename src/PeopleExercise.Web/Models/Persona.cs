using PeopleExercise.Web.Components.Pages;
using System.ComponentModel.DataAnnotations;

namespace PeopleExercise.Web.Models;

public sealed class Persona
{
    public Guid Id { get; set; } = Guid.NewGuid();


    [Required(ErrorMessage = "Il nome e obbligatorio.")]
    [StringLength(100, ErrorMessage = "Il nome non puo superare i 100 caratteri.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Il Cognome e obbligatorio.")]
    [StringLength(100, ErrorMessage = "Il Cognome non puo superare i 100 caratteri.")]
    public string Cognome { get; set; } = string.Empty;

    [Range(0, 130, ErrorMessage = "L eta deve essere tra 0 e 130.")]
    public int Eta { get; set; }

    [Range(typeof(decimal), "0", "999999999999", ErrorMessage = "Il reddito deve essere positivo.")]
    public decimal Reddito { get; set; }

    [DataType(DataType.Date)]
    public DateTime DataNascita { get; set; } = DateTime.Today;
    
    
    [Required(ErrorMessage = "Il sesso è obbligatorio persona confusa.")]
    [StringLength(1, ErrorMessage = "Il sesso non può superare un'carattere.")]
    [RegularExpression("^[MFmf]$", ErrorMessage = "Inserire solo M o F")]
    public string sesso { get; set; } = "";


    [Required(ErrorMessage = "Il Luogo di Nascita è obbligatorio.")]
    public string LuogoDiNascita { get; set; } = "";
    
    public string codiceFiscale { get; set; } = "";
}
