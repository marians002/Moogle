using System.Diagnostics;

namespace Program
{
    public class Program
    {
    
        public static void Main(string[] args)
        {
            Stopwatch crono = new Stopwatch();

            crono.Start();
            //Almacena la lista de listas
            List<List<string>> list_of_lists = TxtReader();

            

            
            //Diccionario de IDF
            Dictionary<string, double> IDF = new();
            //Lista de diccionarios de TF
            List<Dictionary<string, double>> TF_list = new();


                for(int i = 0; i<list_of_lists.Count; i++)
                {
                    Dictionary<string, double> TF = new();


                    for(int j = 0; j<list_of_lists[i].Count; j++)
                    {                       
                    
                        if(!TF.ContainsKey(list_of_lists[i][j]))
                        {
                            TF.Add(list_of_lists[i][j], 1);
                        }
                        else
                            TF[list_of_lists[i][j]]++;

                        if(!IDF.ContainsKey(list_of_lists[i][j]))
                        {
                            IDF.Add(list_of_lists[i][j], 1);
                        }
                        else if(TF[list_of_lists[i][j]]==1)
                        {
                            IDF[list_of_lists[i][j]]++;
                        }

                    }

                        TF_list.Add(TF);

                }

                //Termina de calcular el valor del TF
                for(int i = 0; i<TF_list.Count; i++)
                {
                    foreach(KeyValuePair<string, double> element in TF_list[i])
                    {             
                    TF_list[i][element.Key] /= TF_list[i].Count;
                    }

                }


                //Termina de calcular el valor del IDF
                foreach(KeyValuePair<string, double> element in IDF)
                {                    
                    if(IDF[element.Key] != 0)
                    IDF[element.Key] = Math.Log2(list_of_lists.Count/IDF[element.Key]);
                    else
                    IDF[element.Key] = 0;                    

                }


            //Calcula TFIDF de todos los docs
            List<Dictionary<string, double>> tfidf_list = TFIDF(IDF, TF_list);
                
            //Calcula la magnitud de los documentos.
            int[] docs_magnitude = Documents_Magnitude(tfidf_list);
            
                  crono.Stop();
                  Console.WriteLine(crono.ElapsedMilliseconds + " cargar");
                  crono.Restart();

                  crono.Start();


               //simulando query

            string query = "pergamino";

            char[] query_separators = {' ', '=', '`', ';', '\'', '\t', '.', ',', ':', '-', '_', '/','+', '%','?', '[', ']', '(', ')', '{', '}', '|', 'ª' , 'º', '<', '>' , '¡', '¿', '»', '«', '…', '‥' , '&', '#' , '@', '՛', '՝'} ;
            // Los caracteres !, ^, ~ y * no han sido incluidos porque son para los operadores de busqueda.
            //En el query no deben eliminarse, pero en los txt si.


            List<string> query_list = Words_Separator(query, query_separators);

            (bool[], bool[], int[], bool, string, string) operators_in_words = Operators_Used(query_list);


            //Calcula TFIDF del query
            Dictionary<string, double> query_tfidf = TFIDF(query_list, IDF);
            double query_magnitude = Query_Magnitude(query_tfidf);


            //Calcula la similitud de cosenos y devuelve cual es el doc mas relevante.
            
            double[] cosine_similarity = Cosine_Similarity(query_tfidf, tfidf_list, query_magnitude, docs_magnitude);
                

            Operators(cosine_similarity, operators_in_words, query_list, tfidf_list, list_of_lists);




            //Comprobar que existan resultados:

            bool results_exists = false;
            for(int i = 0; i<cosine_similarity.Length && !results_exists; i++)
            {
                if(cosine_similarity[i]!=0)
                {
                    results_exists = true;
                }
            }

            if(results_exists)
            {
            int[] docs_positions = GetArrayLongestPositions(cosine_similarity);
            string[] docs_path = Highest_TFIDF_Docs(cosine_similarity, docs_positions);
            string[] snippets = new string[docs_path.Length];
            
            for(int i = 0; i<docs_path.Length; i++)
            {
                snippets[i] = GetSnippet(docs_path[i], query_tfidf, list_of_lists[docs_positions[i]]);           
            }


            Console.WriteLine(String.Join('\n', snippets));
            
            }
            else
            {
                Console.WriteLine("NO results found.");

            }

            crono.Stop();
            Console.WriteLine(crono.ElapsedMilliseconds + " query");
              
      
        }
    

        
        //Separa un string en una lista de palabras.
        public static List<string> Words_Separator(string s, char[] separators)
        {
            s = Normalize(s);

            return s.Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList();           
        
        }

        public static string Normalize(string s)
        {
            //Elimina los break line y convierte a minusculas.
            s = s.Replace('\n' , ' ').Replace('\r' , ' ').ToLower();
            
            //Elimina los acentos y caracteres especiales.
            return s.Replace('á', 'a').Replace('à', 'a').Replace('ä', 'a').Replace('ã', 'a').Replace('ā', 'a').Replace('é', 'e').Replace('è', 'e').Replace('ë', 'e').Replace('í', 'i').Replace('ì', 'i').Replace('ï', 'i').Replace('ó', 'o').Replace('ò', 'o').Replace('ö', 'o').Replace('ú', 'u').Replace('ù', 'u').Replace('c', 'c');

        }





        public static List<List<string>> TxtReader() //eficiencia de 4 segundos (minimo) para 40mb.
        {
            //Lista de Listas
            List<List<string>> list_of_lists = new();

            //Array con los nombres de los txt:
            string[] files_names = Get_Files_Names();

            //delimitadores para las palabras de los TXT.
            char[] list_separators = {' ', '=', '`', ';', '\'', '\t', '.', ',', ':', '-', '_', '/','+', '%','?', '[', ']', '(', ')', '{', '}', '|', 'ª' , 'º', '<', '>' , '¡', '¿', '»', '«', '…', '‥' , '&', '#' , '@', '՛', '՝', '!', '*', '^', '~'} ;
            // Los caracteres !, ^, ~ y * no han sido incluidos porque son para los operadores de busqueda.
            //En el query no deben eliminarse, pero en los txt si.


            //Añadir a la lista de listas cada lista con las palabras divididas
                for(int i = 0; i<files_names.Length; i++)
                {                                        
                    list_of_lists.Add(Words_Separator(File.ReadAllText(files_names[i]), list_separators));
                }

            return list_of_lists;

        }
        public static string[] Get_Files_Names()
        {
            string path = Directory.GetCurrentDirectory(); 
            path = Path.Join(path, "..", "/moogle-main/Content");

            return Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
        }

    
    
    
        public static List<Dictionary<string, double>> TFIDF(Dictionary<string, double> IDF, List<Dictionary<string, double>> TF_list)
        {

            List<Dictionary<string, double>> TFIDF = new();                
            
            foreach(Dictionary<string, double> tf_dic in TF_list)
            {                   
                    
                foreach(KeyValuePair<string, double> element in tf_dic)
                {
                    tf_dic[element.Key] *= IDF[element.Key];                        
                }

                TFIDF.Add(tf_dic);
            }
            
            return TFIDF;

        }
    
        static Dictionary<string, double> TFIDF(List<string> query, Dictionary<string, double> idf)
        {

            Dictionary<string, double> tfidf_query = new();

                foreach(string word in query)
                {
                    if(!tfidf_query.ContainsKey(word))
                    {
                        tfidf_query.Add(word, 1);
                    }
                    else
                        tfidf_query[word]++;                    

                }
                
                
                foreach(KeyValuePair<string, double> word in tfidf_query)
                {
                    
                    if(idf.ContainsKey(word.Key))
                    {
                        tfidf_query[word.Key] *= idf[word.Key];
                        //tfidf_query.Add(word.Key, idf[word.Key] * tfidf_query[word.Key]);
                    }
                    else
                    {
                        tfidf_query[word.Key] = 0;
                    }
                    
                }

            return tfidf_query;
        }

        public static int[] Documents_Magnitude(List<Dictionary<string, double>> tfidf_docs)
        {
            double sumatory = 0;
            int[] docs_magnitude = new int[tfidf_docs.Count];
                
            
                for(int i = 0; i < docs_magnitude.Length; i++)
                {
                    foreach(Dictionary<string, double> doc in tfidf_docs)
                    {
                        foreach(KeyValuePair<string, double> element in doc)
                        {
                            sumatory+=element.Value*element.Value;
                        }            
                                                    
                    }
                    
                    docs_magnitude[i] = ((int)Math.Sqrt(sumatory));
                }
            
            return docs_magnitude;
                    
        }

        public static double Query_Magnitude(Dictionary<string, double> tfidf_query)
        {
            double sum_query = 0;

                foreach(KeyValuePair<string, double> element in tfidf_query)
                {
                    sum_query+=element.Value*element.Value;
                }

            return Math.Sqrt(sum_query);
        }       
        
        public static double[] Cosine_Similarity(Dictionary<string, double> tfidf_query, List<Dictionary<string, double>> tfidf_list, double magnitud_query, int[] magnitud_docs)
        {

            double[] suma_punto = new double[tfidf_list.Count];

                for(int i = 0; i<tfidf_list.Count; i++)
                {
                    foreach(KeyValuePair<string, double> element in tfidf_query)
                    {
                        if(tfidf_list[i].ContainsKey(element.Key))
                        {
                            suma_punto[i] += element.Value*tfidf_list[i][element.Key];
                        }
                        
                    }
                }

            //Calcula los cosenos de los angulos

            double[] similitud_de_cosenos = new double[tfidf_list.Count];

                for(int i = 0; i<similitud_de_cosenos.Length; i++)
                {
                    double magnitudes = magnitud_query*magnitud_docs[i];
                    
                    if(magnitudes!=0)
                    similitud_de_cosenos[i] = ((suma_punto[i]/magnitudes));
                    else 
                    similitud_de_cosenos[i] = (0);
                    

                }

            
            return similitud_de_cosenos;
        }

        
        public static void Operators(double[] similitud_de_cosenos, (bool[], bool[], int[], bool, string, string) operators_in_words, List<string> query, List<Dictionary<string, double>> tfidf_list, List<List<string>> lista_de_listas)
        {            

            for(int i = 0 ; i < query.Count; i++)
            {

                if(operators_in_words.Item1[i])
                {
                    for(int j = 0; j<tfidf_list.Count; j++)
                    {
                        if(!tfidf_list[j].ContainsKey(query[i]))
                        {
                            similitud_de_cosenos[j] = 0;
                        }
                    }
                }

                else if(operators_in_words.Item2[i])
                {
                    for(int j = 0; j<tfidf_list.Count; j++)
                    {
                        if(tfidf_list[j].ContainsKey(query[i]))
                        {
                            similitud_de_cosenos[j] = 0;
                        }

                    }
                } 
                else if(operators_in_words.Item3[i] != 0)
                {
                    for(int j = 0; j<tfidf_list.Count; j++)
                    {
                        if(tfidf_list[j].ContainsKey(query[i]))
                        {
                            similitud_de_cosenos[j] *= (operators_in_words.Item3[i] + 2);
                        }
                    }

                }
            }


            if(operators_in_words.Item4)
            {
                Operador_de_Cercania(similitud_de_cosenos, operators_in_words.Item5, operators_in_words.Item6, query, tfidf_list, lista_de_listas);
            }
        }


        public static (bool[], bool[], int[], bool, string word1, string word2) Operators_Used(List<string> query)
        {
            bool[] operador_de_obligatoriedad = new bool[query.Count];
            bool[] operador_de_negacion = new bool[query.Count];
            int[] operador_de_importancia = new int[query.Count];
            bool operador_de_cercania = false;
            string word1 = "";
            string word2 = "";

            for(int i = 0; i<query.Count; i++)
            {
                if(query[i][0] == '^')
                {
                    operador_de_obligatoriedad[i] = true;
                    query[i] = query[i].Remove(0, 1);

                }
                else if(query[i][0] == '!')
                {
                    operador_de_negacion[i] = true;
                    query[i] = query[i].Remove(0, 1);

                }
                else 
                {
                    for (int j = 0; j<query[i].Length; j++)
                    {
                        if(query[i][j] == '*')
                        {
                            operador_de_importancia[i]++;
                        }
                        else break;
                    }

                    query[i] = query[i].Remove(0, operador_de_importancia[i]);
                }

                if(query[i] == "~")
                {
                    operador_de_cercania = true;
                    word1 = query[i-1];
                    word2 = query[i+1];
                    query.Remove(query[i]);
                        

                }

            }

            return (operador_de_obligatoriedad, operador_de_negacion, operador_de_importancia, operador_de_cercania, word1, word2);
        }


        public static void Operador_de_Cercania(double[] similitud_de_cosenos, string word1, string word2, List<string> query, List<Dictionary<string, double>> tfidf_list, List<List<string>> lista_de_listas)
        {
            int[] distancia_por_docs = new int[lista_de_listas.Count];                

                for(int i=0; i<tfidf_list.Count; i++)
                {
                    List<int> word1_positions = new();
                    List<int> word2_positions = new();


                    foreach(KeyValuePair<string, double> word in tfidf_list[i])
                    {
                        if(word.Key == word1 || word.Key == word2)
                        {

                            for(int j = 0; j<lista_de_listas[i].Count; j++)
                            {
                                if(lista_de_listas[i][j] == word1)
                                {
                                    word1_positions.Add(j);
                                }
                                else if(lista_de_listas[i][j] == word2)
                                {
                                    word2_positions.Add(j);
                                }
                            }

                        break;

                        }                          
                        

                    }

                    
                    if((word1_positions.Count != 0) && (word2_positions.Count != 0))
                    {
                        int temporal = 0;
                        int resta = int.MaxValue;
                                                    
                            for(int n = 0; n < word1_positions.Count; n++)
                            {
                                for(int m = 0; m < word2_positions.Count; m++)
                                {
                                    
                                    temporal = (word1_positions[n] - word2_positions[m]);
                                    if(Math.Sign(temporal) == -1)
                                    {
                                        temporal = -temporal;
                                    }
                                    if(resta > temporal)
                                    resta = temporal;

                                }           

                            }
                        distancia_por_docs[i] = resta;                          

                        
                    }
                    else
                    {
                        distancia_por_docs[i] = tfidf_list[i].Count;

                    }
                    similitud_de_cosenos[i] = similitud_de_cosenos[i]/distancia_por_docs[i];


                }
        } 

        static string[] Highest_TFIDF_Docs(double[] cosenos, int[] highest_docs_positions)
        {

            string[] Highest_TFIDF_Docs = new string[3];
            

                for(int i = 0; i<Highest_TFIDF_Docs.Length; i++)
                {
                    Highest_TFIDF_Docs[i] = Get_Files_Names()[highest_docs_positions[i]];
                }

                return Highest_TFIDF_Docs;

        }

        public static int[] GetArrayLongestPositions(double[] cosenos)
        {
            int max = 0;
            int med = 0;
            int low = 0;  

            for(int i = 0; i<cosenos.Length; i++)
            {               
                

                if(cosenos[i]>cosenos[max])
                max = i;
            }
            cosenos[max] = -1;

            
                for(int i = 0; i<cosenos.Length; i++)
                {
                    if(cosenos[i]>cosenos[med])
                    {
                        med = i;
                    }
                }
                cosenos[med] = -1;

                for(int i = 0; i<cosenos.Length; i++)
                {
                    if(cosenos[i]>cosenos[low])
                    {
                        low = i;
                    }
                }
                
            int[] positions = {max, med, low};
            
            return positions;
            
            
            
        }
          
    



        

        //Muestra una parte de los textos donde hay resultados de la query. Arreglar.
         
        public static List<string> Order_query_tfidf(Dictionary<string, double> query_tfidf)
        {
           
            //ordena el tfidf del query.
            var Dictionary_Ordered = from entry in query_tfidf orderby entry.Value descending select entry;;
            
            List<string> list_ordered = new();
            foreach(KeyValuePair<string, double> entry in Dictionary_Ordered)
            {
                list_ordered.Add(entry.Key);
            }
            
            return list_ordered;

        
        }
        
        
        public static string GetSnippet(string doc_path, Dictionary<string, double> query_tfidf, List<string> doc)
        {

            List<string> query_list = Order_query_tfidf(query_tfidf);

            string readed_doc = File.ReadAllText(doc_path).Replace('\n' , ' ').Replace('\r' , ' ');

            char[] separators = {' ', '\n', '\t'};

            string readed_doc_normalized = Normalize(readed_doc);
           
            //string[] readed_doc_splitted = readed_doc.Split(separators , StringSplitOptions.RemoveEmptyEntries);
            string snippet = "";
           
            for(int i = 0; i<query_list.Count; i++)
            {
                if(doc.Contains(query_list[i]))
                {
                    int position = readed_doc_normalized.IndexOf(query_list[i]);

                    if(readed_doc_normalized.Length > position + 100)
                    {
                        snippet = "..." + readed_doc.Substring(position, 100) + "...";
                    }
                    else if(readed_doc_normalized.Length > position + 25)
                    {
                        snippet = "..." + readed_doc.Substring(position, 25) + "...";

                    }
                    else
                    {
                        snippet = "..." + readed_doc.Substring(position, readed_doc_normalized.Length-1) + "...";
                    }
                    break;
                }
            }

            return snippet;
    }

    }
}