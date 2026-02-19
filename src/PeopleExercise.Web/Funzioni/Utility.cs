using Microsoft.AspNetCore.Mvc;
using PeopleExercise.Web.Components.Pages;
using PeopleExercise.Web.Models;
using System.Reflection.Metadata.Ecma335;

namespace PeopleExercise.Web.Funzioni
{
    public class Utility
    {
        public string calcoloNomeCognomeCodFiscale(string nomeOCongnome, bool isNome)
        {
            string ret = "";
            List<string> consonanti = new List<string> { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "z" };
            List<string> vocali = new List<string> { "a", "e", "i", "o", "u", "y" };

            nomeOCongnome = nomeOCongnome.ToLower();
            int lunghezzaNome = nomeOCongnome.Length;
            int controllo = 0;
            int controllo1;
            int controllo2;
            int controllo3;
            if (isNome)
            {
                controllo1 = 1;
                controllo2 = 3;
                controllo3 = 4;

            }
            else
            {
                controllo1 = 1;
                controllo2 = 2;
                controllo3 = 3;
            }


            for (int i = 0; i < lunghezzaNome; i++)
            {
                string posizioneI = nomeOCongnome.Substring(i, 1);
                if (consonanti.Contains(posizioneI))
                {
                    controllo++;
                    if (controllo == controllo1 || controllo == controllo2 || controllo == controllo3)
                    {
                        ret = ret + posizioneI;

                    }
                    else
                    {
                        continue;
                    }

                }

            }



            return ret;

        }
    }
}
