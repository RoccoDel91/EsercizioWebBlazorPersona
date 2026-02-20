using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Linq;
using System.ComponentModel.DataAnnotations;
namespace PeopleExercise.Web.Models

{
    using System.Collections.Immutable;
    using System.Text;
    using System.Text.Json;

    public class Comuni
    {

        
        List <Comune> listaComuni { get; set; }
        public  Dictionary<string, string> dizionarioComuni { get; set; } 
        private  readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
        public class Comune
        {
            public string nome { get; set; }
            public string codiceCatastale { get; set; }

            
        }



        public  Dictionary<string, string> CalcoloComuneAsync()
        {
            var jsonContent =  File.ReadAllText("C:\\Lavoro\\Eserciziopersona\\src\\PeopleExercise.Web\\Data\\Comuni\\comuni_Codici.json",
                Encoding.UTF8);
            
            // Deserializziamo come List perché il tuo JSON inizia con [ (quadra)
            var listaComuni = JsonSerializer.Deserialize<List<Comune>>(jsonContent, JsonSerializerOptions);



            // Trasformazione in Dictionary gestendo i nomi duplicati
            // Se ci sono due "Castro", prenderà il primo incontrato ed eviterà il crash
            dizionarioComuni = listaComuni
                .GroupBy(c => c.nome)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().codiceCatastale
                );

            return dizionarioComuni;
        }

    }
}




