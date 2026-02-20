using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PeopleExercise.Web.Components.Pages;
using PeopleExercise.Web.Configuration;
using PeopleExercise.Web.Configuration;
using PeopleExercise.Web.Models;
using PeopleExercise.Web.Services;
using System;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;


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
        public string CalcolaDataSesso(DateTime dataNascita, string sesso)
        {
            string cdata = "";
            string dd = dataNascita.Day.ToString();
            int mm = dataNascita.Month;
            string yyyy = dataNascita.Year.ToString();
            cdata = cdata + yyyy.Substring(2);
            Dictionary<int, char> mesiCodiceFiscale = new Dictionary<int, char>
        {
            { 1, 'A' },  // Gennaio
            { 2, 'B' },  // Febbraio
            { 3, 'C' },  // Marzo
            { 4, 'D' },  // Aprile
            { 5, 'E' },  // Maggio
            { 6, 'H' },  // Giugno
            { 7, 'L' },  // Luglio
            { 8, 'M' },  // Agosto
            { 9, 'P' },  // Settembre
            { 10, 'R' }, // Ottobre
            { 11, 'S' }, // Novembre
            { 12, 'T' }  // Dicembre
        };
            cdata = cdata + mesiCodiceFiscale[mm];
            if (sesso == "M")
            {
                cdata = cdata + dd;
            }
            else
            {
                int d = dataNascita.Day;
                d = d + 40;
                dd = d.ToString();
                cdata = cdata + dd;
            }
            return cdata;
        }

        






        public async Task test()
        {

            await Comuni.CalcoloComuneAsync();
        }
           
             public string CalcolaCodComune( string luogoDiNascita)
        {
            string codiceCatastale = "";

            codiceCatastale = Comuni.dizionarioComuni[luogoDiNascita];
            return codiceCatastale;
        }
   
       
    }
}
