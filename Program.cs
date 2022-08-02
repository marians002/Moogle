using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Program
{
    public class Program
    {

    
        public static void Main(string[] args)
        {
            TFIDF(TF(txtReader()[3]), IDF(txtReader()));
      
        }
    

        //Normaliza las palabras de los textos.
        static string Normalize(string s)
        {
            //Elimina los break line y convierte a minusculas.
            s = s.Replace('\n' , ' ').Replace('\r',' ').ToLower();
            
            //Elimina los acentos y caracteres especiales.

                Regex replace_a_Accents = new Regex("[á|à|ä|â]", RegexOptions.Compiled);
                Regex replace_e_Accents = new Regex("[é|è|ë|ê]", RegexOptions.Compiled);
                Regex replace_i_Accents = new Regex("[í|ì|ï|î]", RegexOptions.Compiled);
                Regex replace_o_Accents = new Regex("[ó|ò|ö|ô]", RegexOptions.Compiled);
                Regex replace_u_Accents = new Regex("[ú|ù|ü|û]", RegexOptions.Compiled);
                s = replace_a_Accents.Replace(s, "a");
                s = replace_e_Accents.Replace(s, "e");
                s = replace_i_Accents.Replace(s, "i");
                s = replace_o_Accents.Replace(s, "o");
                s = replace_u_Accents.Replace(s, "u");
        
        return s;
        }

        
        
        //Separa un string en un array de strings por palabras.
        public static string[] Separador_Palabras(string s)
        {

            char[] delimitadores = {' ', '=', '`', ';', '\'', '\t', '.', ',', ':', '-', '_', '/','+', '%','?', '[', ']', '(', ')', '{', '}', '|'} ;
            // Los caracteres !, ^, ~ y * no han sido incluidos porque son para los operadores de busqueda.
            
            s = Normalize(s);

            return s.Split(delimitadores, StringSplitOptions.RemoveEmptyEntries);
            
        
        }

        public static List<List<string>> txtReader()
        {
            //string ruta = Directory.GetCurrentDirectory() + "..moogle-main/Content/prueba";
            string ruta = "/home/marian_susana/Documents/Moogle/moogle-main/Content/prueba";
            DirectoryInfo directorio = new DirectoryInfo(ruta);

            FileInfo[] archivos = directorio.GetFiles("*.txt", SearchOption.AllDirectories);


            string[] path = new string[archivos.Length];
            
            for (int i=0; i<archivos.Length; i++)
            {
                //Convierte la informacion devuelta por archivos a un array de strings.
                path[i] = archivos[i].ToString();
            }

            
            StreamReader[] files = new StreamReader[path.Length];
            string[] leidos = new string[files.Length];

            
            for(int i = 0; i<path.Length; i++)
            {                
                files[i] = new StreamReader(path[i]);
                
                leidos[i] = files[i].ReadToEnd();
            }


            //Lista de listas: cada elemento es una lista de strings con los txt
            List<List<string>> Conjunto_de_Listas = new();


            for(int i = 0; i<path.Length; i++)
            {

                List<string> textos = new();

                foreach(string palabras in leidos)
                {                    
                    textos = Separador_Palabras(leidos[i]).ToList();
                }

                Conjunto_de_Listas.Add(textos);
            }

                    

                //Para imprimir palabra por palabra
                /*
                foreach(List<string> items in Conjunto_de_Listas)
                {
                    foreach(string elements in items){
                        
                        Console.WriteLine(elements);
                    }
                }
                */

                
                return Conjunto_de_Listas;

        }

        //Metodo que calcula el TF.
        public static Dictionary<string, int> TF(List<string> lista)
        {

            Dictionary<string, int> Dicc = new();


            for(int i = 0; i < lista.Count; i++)
            {

                    if(Dicc.ContainsKey(lista[i])){

                        Dicc[lista[i]]++;
                    }
                    else{
                        Dicc.Add(lista[i], 1);
                    }
                    
            }  

            
           // Para imprimir el diccionario
            /*
            foreach(KeyValuePair<string, int> value in Dicc){
                    Console.WriteLine(value);
            }
            
            */
            
            return Dicc;
        }  


        //Metodo que calcula el IDF
        public static Dictionary<string, double> IDF(List<List<string>> lista)
        {


            int cant_docs = lista.Count;

            Dictionary<string, double> Dict = new();

                for(int i = 0; i<lista.Count; i++)
                {

                    foreach(string word in lista[i])
                    {
                        double idf;
                        if(Docs_True(lista, word) != 0)
                        {
                            idf = Math.Log(cant_docs/Docs_True(lista, word));
                        }
                        
                        else
                        {
                            idf = 0;
                        }

                        if(!Dict.ContainsKey(word))
                            Dict.Add(word, idf);
                        
                    }

                }

            // Para imprimir el diccionario
            /*
            foreach(KeyValuePair<string, double> value in Dict){
                    Console.WriteLine(value);
            }
            */
            return Dict;
            
            
        } 

        public static int Docs_True(List<List<string>> conj_de_listas, string s)
        {
            int total = 0;

            for (int i = 0; i<conj_de_listas.Count; i++)
            {
                if(conj_de_listas[i].Contains(s))
                {
                    total++;
                }
            }

            return total;
        }


        public static Dictionary<string, double> TFIDF(Dictionary<string, int> Dicc, Dictionary<string, double> Dict){


            Dictionary<string, double> TFIDF = new();
            
            foreach(KeyValuePair<string, int> value in Dicc){
                TFIDF.Add(value.Key, value.Value * Dict[value.Key]);
            }
            
            // Para imprimir el diccionario
            /*
            foreach(KeyValuePair<string, double> value in TFIDF){
                    Console.WriteLine(value);
            }
            */
            
            return TFIDF;

        }

        


           
    }
}