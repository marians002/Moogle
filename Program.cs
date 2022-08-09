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
            /*
                Dictionary <string, double> dic = IDF(txtReader());
            
                for(int i=0; i<4; i++)
                TFIDF(TF(txtReader()[i]), dic);
                */
                
                //txtReader();
            Snippets(SearchResult("epistemologia"));


            
      
        }
    

        //Normaliza las palabras de los textos.
        static string Normalize(string s)
        {
            //Elimina los break line y convierte a minusculas.
            s = s.Replace('\n' , ' ').Replace('\r',' ').ToLower();
            
            //Elimina los acentos y caracteres especiales.

                Regex replace_a_Accents = new Regex("[á|à|ä|â|ã|å|ā|ă]", RegexOptions.Compiled);
                Regex replace_e_Accents = new Regex("[é|è|ë|ê]", RegexOptions.Compiled);
                Regex replace_i_Accents = new Regex("[í|ì|ï|î]", RegexOptions.Compiled);
                Regex replace_o_Accents = new Regex("[ó|ò|ö|ô|õ]", RegexOptions.Compiled);
                Regex replace_u_Accents = new Regex("[ú|ù|ü|û]", RegexOptions.Compiled);
                Regex replace_c_Accents = new Regex("[ć|ĉ|č|ç]", RegexOptions.Compiled);

                s = replace_a_Accents.Replace(s, "a");
                s = replace_e_Accents.Replace(s, "e");
                s = replace_i_Accents.Replace(s, "i");
                s = replace_o_Accents.Replace(s, "o");
                s = replace_u_Accents.Replace(s, "u");
                s = replace_c_Accents.Replace(s, "c");

        
        return s;
        }

        
        
        //Separa un string en un array de strings por palabras.
        public static string[] Separador_Palabras(string s)
        {

            char[] delimitadores = {' ', '=', '`', ';', '\'', '\t', '.', ',', ':', '-', '_', '/','+', '%','?', '[', ']', '(', ')', '{', '}', '|', 'ª' , 'º', '<', '>' , '¡', '¿', '»', '«', '…', '‥' , '&', '#' , '@', '՛', '՝', '!', '*', '^', '~'} ;
            // Los caracteres !, ^, ~ y * no han sido incluidos porque son para los operadores de busqueda.
            //EN el query no deben eliminarse, pero en los txt si.
            s = Normalize(s);

            return s.Split(delimitadores, StringSplitOptions.RemoveEmptyEntries);
            
        
        }

        public static string[] Separador_Palabras_Query(string s)
        {

            char[] delimitadores = {' ', '=', '`', ';', '\'', '\t', '.', ',', ':', '-', '_', '/','+', '%','?', '[', ']', '(', ')', '{', '}', '|', 'ª' , 'º', '<', '>' , '¡', '¿', '»', '«', '…', '‥' , '&', '#' , '@', '՛', '՝'} ;
            // Los caracteres !, ^, ~ y * no han sido incluidos porque son para los operadores de busqueda.
            //En el query no deben eliminarse, pero en los txt si.
            s = Normalize(s);

            return s.Split(delimitadores, StringSplitOptions.RemoveEmptyEntries);
            
        
        }

            //string ruta = Directory.GetCurrentDirectory() + "..moogle-main/Content/prueba";
            

            public static string[] getFilesNames(){

            string ruta = "/home/marian_susana/Documents/Moogle/moogle-main/Content/prueba";

            StreamReader lector = new StreamReader(ruta);
            string texto = lector.ReadToEnd();

            return Directory.GetFiles(ruta, "*.txt", SearchOption.AllDirectories);


            }
            

            public static List<List<string>> txtReader()
            {
            //Lista de Listas
            List<List<string>> Conjunto_de_Listas = new();

            for(int i = 0; i<getFilesNames().Length; i++)
            {

                List<string> lista = Separador_Palabras(File.ReadAllText(getFilesNames()[i])).ToList();
                
                Conjunto_de_Listas.Add(lista);

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


            Dictionary<string, double> Dict = new();

                for(int i = 0; i<lista.Count; i++)
                {

                    foreach(string word in lista[i])
                    {
                        double idf;
                        if(Docs_True(lista, word) != 0)
                        {
                            idf = Math.Log(lista.Count/Docs_True(lista, word));
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

        //Metodo que cuenta en cuantos docs aparece una palabra
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
            
            foreach(KeyValuePair<string, int> element in Dicc){
                TFIDF.Add(element.Key, element.Value * Dict[element.Key]);
            }
            
            // Para imprimir el diccionario
            /*
            foreach(KeyValuePair<string, double> value in TFIDF){
                    Console.WriteLine(value);
            }
            */
            
            
            return TFIDF;

        }


        //Metodos para usar con la query:

        //Normalize y Separador_Palabras_Query

        static string[] SearchResult(string s){

            //Almacenar query en una lista
            List<string> query = Separador_Palabras_Query(s).ToList();

            //Almacenar idf en un diccionario
            Dictionary <string, double> dic = IDF(txtReader());

             //Almacenar la lista de listas:
            List<List<string>> lista_de_listas = txtReader();

             //Almacenar tfidf en una lista de diccionarios:
            List<Dictionary<string, double>> tfidf_list = new List<Dictionary<string, double>>(lista_de_listas.Count);

                for(int i = 0; i<lista_de_listas.Count; i++)
                {
                tfidf_list.Add(TFIDF(TF(lista_de_listas[i]), dic));
                }
            

                static double[,] Matrix(List<List<string>> list_of_lists, List<string> strings_list, List<Dictionary<string, double>> list_of_dictionaries)
                {
                    //Matriz con las palabras y su tfidf por documentos
                    double[,] matrix = new double[list_of_lists.Count, strings_list.Count];

                    for(int i = 0; i < matrix.GetLength(0); i++)
                    {
                        for(int j = 0; j < matrix.GetLength(1); j++)
                        {
                            if(list_of_dictionaries[i].ContainsKey(strings_list[j]))

                            matrix[i,j] = list_of_dictionaries[i][strings_list[j]];

                            else matrix[i,j] = 0;
                            
                        }


                    }

                return matrix;
                }
                //Almacena lo devuelto por el metodo matriz en un array bidimensional
                double[,] matrix = Matrix(lista_de_listas, query, tfidf_list);

                static double[] Sum(double[,] matrix)
                {

                    double[] sum = new double[matrix.GetLength(0)];

                    for(int i = 0; i<sum.Length; i++)
                    {
                        for(int j=0; j<matrix.GetLength(1); j++)
                        {
                            sum[i] += matrix[i,j];
                        } 

                    }
                    return sum;
                }

                //Almacena lo devuelto por Sum en un array
                double[] sum = Sum(matrix);

            
            

            string[] Highest_TFIDF_Docs = new string[3];

            for(int i = 0; i<Highest_TFIDF_Docs.Length; i++)
            {
                Highest_TFIDF_Docs[i] = getFilesNames()[GetArrayLongestPositions(sum)[i]];
            }

            //Todo eso pincha pero esta regado. JEJE

                return Highest_TFIDF_Docs;
            

        }
        public static int[] GetArrayLongestPositions(double[] array)
            {
                int max = 0;
                int med = 0;
                int low = 0;
                for(int i = 0; i<array.Length; i++)
                {
                    
                    if(array[i]>array[max])
                    max = i;
                }

                for(int i = 0; i<array.Length; i++)
                {

                    if(i!=max && array[i]>array[med])
                    {
                        med = i;
                    }
                }

                for(int i = 0; i<array.Length; i++)
                {

                    if(i!=max && i!=med && array[i]>array[low])
                    {
                        low = i;
                    }
                }
                int[] r = {max, med, low};
                return r;
            }

            //Muestra una parte de los textos donde hay resultados de la query. Arreglar.
            public static List<string>[] Snippets(string[] path)
            {
                List<string> result1 = Separador_Palabras(File.ReadAllText(path[0])).ToList();
                List<string> result2 = Separador_Palabras(File.ReadAllText(path[1])).ToList();
                List<string> result3 = Separador_Palabras(File.ReadAllText(path[2])).ToList();

                result1.RemoveRange(5, result1.Count()-5);
                result2.RemoveRange(6, result2.Count()-6);
                result3.RemoveRange(6, result3.Count()-6);

                                                       



                List<string>[] snippets = {result1, result2, result3};

                foreach(List<string> texto in snippets)
                {
                    foreach(string word in texto)
                    {
                        System.Console.WriteLine(" " + word);
                    }
                }


                
                return snippets;

            
            }

            


           
    }
}