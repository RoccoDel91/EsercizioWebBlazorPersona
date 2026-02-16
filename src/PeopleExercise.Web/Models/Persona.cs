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

    // 
}
