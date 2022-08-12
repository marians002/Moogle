using System;
using System.Diagnostics;
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
                
                
                
                
                List<List<string>> list = txtReader();
                Dictionary<string, double> idf = IDF(list);
                List<Dictionary<string, double>> tfidf_dicc_list = new();

                
                for(int i = 0; i < list.Count; i++)
                {                    
                    tfidf_dicc_list.Add(TFIDF(TF(list[i]), idf));                    

                }
                
                 
              
              //simulando query
               List<string> lista = new List<string>();
               lista.Add("perro");
               lista.Add("epistemologia");
               lista.Add("la");

               Dictionary<string, double> tfidf_query = query_tfidf(lista, idf);

                 
               
                
               
            Cosine_Similarity(tfidf_query, tfidf_dicc_list);
           
      
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
            //En el query no deben eliminarse, pero en los txt si.
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

            return Directory.GetFiles(ruta, "*.txt", SearchOption.AllDirectories);


            }
            

            public static List<List<string>> txtReader() //eficiencia de 12 segundos para 42mb
            {
            //Lista de Listas
            List<List<string>> Conjunto_de_Listas = new();

            //Almacen para el array devuelto por getFilesNames:
            string[] filesNames = getFilesNames();

            for(int i = 0; i<filesNames.Length; i++)
            {

                List<string> lista = Separador_Palabras(File.ReadAllText(filesNames[i])).ToList();
                
                Conjunto_de_Listas.Add(lista);

            }

                return Conjunto_de_Listas;

        }

        //Metodo que calcula el TF.
        //14 segundos para 42mb sumando txtReader()
        public static Dictionary<string, double> TF(List<string> lista) 
        {

            Dictionary<string, double> Dicc = new();


            for(int i = 0; i < lista.Count; i++)
            {

                if(!Dicc.ContainsKey(lista[i]))
                
                    Dicc.Add(lista[i], 1);
                
                else
                
                    Dicc[lista[i]]++;                 
                   
            }  
            foreach(KeyValuePair<string, double> element in Dicc)
            {
                Dicc[element.Key] = Dicc[element.Key]/Dicc.Count; 
            }

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
                        if(!Dict.ContainsKey(word))
                        {
                            //cantidad de docs donde aparece la palabra
                            int docs = Docs_True(lista, word);

                            if(docs != 0)
                            {
                                double div = lista.Count/docs;

                                if(div != 1)
                                { 
                                Dict.Add(word, Math.Log2(div));
                                } 
                                else Dict.Add(word, 0);
                            }
                            
                            else
                            {
                                Dict.Add(word, 0);
                            }

                        }
                        
                        
                    }

                }
            

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


        public static Dictionary<string, double> TFIDF(Dictionary<string, double> Dicc, Dictionary<string, double> Dict){


            Dictionary<string, double> TFIDF = new();
            
            foreach(KeyValuePair<string, double> element in Dicc)
            {

                if(!TFIDF.ContainsKey(element.Key))
                    TFIDF.Add(element.Key, element.Value * Dict[element.Key]);
            }
            
            
            return TFIDF;

        }


        //Metodos para usar con la query:

        //Normalize y Separador_Palabras_Query

        static string[] SearchResult(string s)
        {

            //Almacenar query en una lista
            List<string> query = Separador_Palabras_Query(s).ToList();

            //Almacenar la lista de listas:
            List<List<string>> lista_de_listas = txtReader();

            //Almacenar idf en un diccionario
            Dictionary <string, double> dic = IDF(lista_de_listas);


             //Almacenar tfidf en una lista de diccionarios:
            List<Dictionary<string, double>> tfidf_list = new List<Dictionary<string, double>>(lista_de_listas.Count);

                for(int i = 0; i<lista_de_listas.Count; i++)
                {
                tfidf_list.Add(TFIDF(TF(lista_de_listas[i]), dic));
                }
            

                static double[,] Matrix(List<List<string>> conj_de_listas, List<string> lista_strings, List<Dictionary<string, double>> lista_de_diccionarios)
                {
                    //Matriz con las palabras y su tfidf por documentos
                    double[,] matrix = new double[conj_de_listas.Count, lista_strings.Count];

                    for(int i = 0; i < matrix.GetLength(0); i++)
                    {
                        for(int j = 0; j < matrix.GetLength(1); j++)
                        {
                            if(lista_de_diccionarios[i].ContainsKey(lista_strings[j]))

                            matrix[i,j] = lista_de_diccionarios[i][lista_strings[j]];

                            else matrix[i,j] = 0;
                            
                        }


                    }

                return matrix;
                }
                //Almacena lo devuelto por el metodo Matrix en un array bidimensional
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
            /*
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
            */
            public static Dictionary<string, double> query_tfidf(List<string> query, Dictionary<string, double> idf)
            {
                Dictionary<string, double> tfidf_query = new();
                Dictionary<string, double> tf_query = new();

                tf_query = TF(query);
                
                foreach(string word in query)
                {
                    if(idf.ContainsKey(word))
                    {
                        tfidf_query.Add(word, idf[word] * tf_query[word]);
                    }
                    else
                    {
                        tfidf_query.Add(word, 0);
                    }
                }

                return tfidf_query;


            }
            
            public static double[] Cosine_Similarity(Dictionary<string, double> tfidf_query, List<Dictionary<string, double>> tfidf_docs)
            {
                //Calcular la magnitud del query una sola vez.
                double magnitud_query = 0;
                double suma_query = 0;

                    foreach(KeyValuePair<string, double> element in tfidf_query)
                    {
                        suma_query+=element.Value*element.Value;
                    }

                magnitud_query = Math.Sqrt(suma_query);

                //Calcular la magnitud de cada documento.

                double suma_doc = 0;
                double[] magnitud_doc = new double[tfidf_docs.Count];

                    foreach(Dictionary<string, double> doc in tfidf_docs)
                    {
                        foreach(KeyValuePair<string, double> element in doc)
                        {
                            suma_doc+=element.Value*element.Value;
                        }

                        for(int i = 0; i< magnitud_doc.Length; i++)
                        {
                            magnitud_doc[i]= Math.Sqrt(suma_doc);
                        }
                        
                    }

                //Calcula la suma punto

                double[] suma_punto = new double[tfidf_docs.Count];

                    for(int i = 0; i<tfidf_docs.Count; i++)
                    {
                        foreach(KeyValuePair<string, double> element in tfidf_query)
                        {
                            if(tfidf_docs[i].ContainsKey(element.Key))
                            {
                                suma_punto[i] += element.Value*tfidf_docs[i][element.Key];
                            }
                        }
                    }

                //Calcula los cosenos de los angulos

                double[] similitud_de_cosenos = new double[tfidf_docs.Count];

                    for(int i = 0; i<similitud_de_cosenos.Length; i++)
                    {
                        similitud_de_cosenos[i] = Math.Cos(suma_punto[i]/(magnitud_query*magnitud_doc[i]));
                    }

                
                return similitud_de_cosenos;
            }
            

            


           
    }
}

//TODO:  Revisar tfidf del query con el tema de la lista. Calcula el tf de la lista dell query mal
//Revisar el coseno porque no devuelve lo esperado.