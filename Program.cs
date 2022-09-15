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


            /*Añadir a la lista de diccionarios de TF la cantidad de veces
              que aparece una palabra.
              Y a la lista de diccionarios de IDF la cantidad de documentos donde aparece
              la palabra.
            */

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


            //Calcula TFIDF de todos los documetnos y almacena en una lista de diccionarios
            List<Dictionary<string, double>> tfidf_list = TFIDF(IDF, TF_list);
                
            //Calcula la magnitud de los documentos.
            double[] docs_magnitude = DocumentsMagnitude(tfidf_list);

           
            
                  crono.Stop();
                  Console.WriteLine(crono.ElapsedMilliseconds + " cargar");
                  crono.Restart();

                  crono.Start();


               //simulando query

            string query = "Orient Express";

            char[] query_separators = {' ', '=', '`', ';', '\'', '\t', '.', ',', ':', '-', '_', '/','+', '%','?', '[', ']', '(', ')', '{', '}', '|', 'ª' , 'º', '<', '>' , '¡', '¿', '»', '«', '…', '‥' , '&', '#' , '@', '՛', '՝'} ;
            // Los caracteres !, ^, ~ y * no han sido incluidos porque son para los operadores de busqueda.
            //En el query no deben eliminarse, pero en los txt si.

            //Almacena el query en una lista de palabras casi normalizadas
            List<string> query_list = WordsSeparator(query, query_separators);

            //Almacena que operadores fueron usados en el query
            (bool[], bool[], int[], bool, string, string) operators_in_words = OperatorsUsed(query_list);


            //Calcula TFIDF del query
            Dictionary<string, double> query_tfidf = TFIDF(IDF, query_list);
            //Calcula magnitud del query
            double query_magnitude = QueryMagnitude(query_tfidf);


            //Calcula la similitud de cosenos y devuelve cual es el doc mas relevante.
            
            double[] cosine_similarity = CosineSimilarity(query_tfidf, tfidf_list, query_magnitude, docs_magnitude);
                
            //Realiza las operaciones pertinentes con los operadores que modificaran
            //la relevancia de cada documento.
            Operators(cosine_similarity, operators_in_words, query_list, tfidf_list, list_of_lists);
       
            //Devuelve la posicion que ocupan los documentos mas relevantes.
            //Si ningun documento se corresponde con el query, devuelve -1. 
            int[] docs_positions = GetArrayLongestPositions(cosine_similarity);

            //Si existen resultados, muestra un fragmento de cada documento donde aparezca parte
            //del query. En caso de no existir resultados, imprime que no se encontraron resultados.
            if(docs_positions[0] != -1)
            {
                string[] docs_path = MostImportantDocs(cosine_similarity, docs_positions);
                string[] snippets = new string[docs_path.Length];
                
                for(int i = 0; i<docs_path.Length; i++)
                {
                    snippets[i] = GetSnippet(docs_path[i], query_tfidf, list_of_lists[docs_positions[i]]);           
                }


                Console.WriteLine(String.Join('\n', snippets));
                Console.WriteLine(String.Join('\n', docs_path));

                
            }
            
            else
            {
                Console.WriteLine("NO results found.");

            }

            crono.Stop();
            Console.WriteLine(crono.ElapsedMilliseconds + " query");
              
      
        }
    

        
        //Separa un string en una lista de palabras.
        public static List<string> WordsSeparator(string s, char[] separators)
        {
            s = Normalize(s);

            return s.Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList();           
        
        }

        //Elimina los caracteres especiales de un string y convierte a minusculas.
        public static string Normalize(string s)
        {
            //Elimina los break line y convierte a minusculas.
            s = s.Replace('\n' , ' ').Replace('\r' , ' ').ToLower();
            
            //Elimina los acentos y caracteres especiales.
            return s.Replace('á', 'a').Replace('à', 'a').Replace('ä', 'a').Replace('ã', 'a').Replace('ā', 'a').Replace('é', 'e').Replace('è', 'e').Replace('ë', 'e').Replace('í', 'i').Replace('ì', 'i').Replace('ï', 'i').Replace('ó', 'o').Replace('ò', 'o').Replace('ö', 'o').Replace('ú', 'u').Replace('ù', 'u').Replace('c', 'c');

        }

        //Devuelve una lista de los documentos que contiene, a su vez, una lista de cada palabra
        //en el documento. 
        public static List<List<string>> TxtReader()
        {
            //Lista de Listas
            List<List<string>> list_of_lists = new();

            //Array con los nombres de los txt:
            string[] files_names = GetFilesNames();

            //delimitadores para las palabras de los TXT.
            char[] list_separators = {' ', '=', '`', ';', '\'', '\t', '.', ',', ':', '-', '_', '/','+', '%','?', '[', ']', '(', ')', '{', '}', '|', 'ª' , 'º', '<', '>' , '¡', '¿', '»', '«', '…', '‥' , '&', '#' , '@', '՛', '՝', '!', '*', '^', '~'} ;
            // Los caracteres !, ^, ~ y * no han sido incluidos porque son para los operadores de busqueda.
            //En el query no deben eliminarse, pero en los txt si.


            //Añadir a la lista de listas cada lista con las palabras divididas
                for(int i = 0; i<files_names.Length; i++)
                {                                        
                    list_of_lists.Add(WordsSeparator(File.ReadAllText(files_names[i]), list_separators));
                }

            return list_of_lists;

        }

        //Devuelve la ruta de todos los documentos que se encuentran en la carpeta Content.
        public static string[] GetFilesNames()
        {
            string path = Directory.GetCurrentDirectory(); 
            path = Path.Join(path, "..", "/moogle-main/Content");

            return Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
        }

    
    
        //Multiplica el TF de una palabra en un documento por su IDF y devuelve este valor
        //almacenado en una lista de diccionarios con cada palabra y su tfidf en el documento.
        public static List<Dictionary<string, double>> TFIDF(Dictionary<string, double> idf, List<Dictionary<string, double>> tf_list)
        {

            List<Dictionary<string, double>> TFIDF = new();                
            
            foreach(Dictionary<string, double> tf_dic in tf_list)
            {                   
                    
                foreach(KeyValuePair<string, double> element in tf_dic)
                {
                    tf_dic[element.Key] *= idf[element.Key];                        
                }

                TFIDF.Add(tf_dic);
            }
            
            return TFIDF;

        }
    
        //Multiplica cada palabra del query por su frecuencia en el propio query y su idf
        //Almacena el resultado en un diccionario de TFIDF.
        static Dictionary<string, double> TFIDF(Dictionary<string, double> idf, List<string> query)
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
                    }
                    else
                    {
                        tfidf_query[word.Key] = 0;
                    }
                    
                }

            return tfidf_query;
        }

        public static double[] DocumentsMagnitude(List<Dictionary<string, double>> docs_tfidf)
        {
            double[] docs_magnitude = new double[docs_tfidf.Count];
                
            
                for(int i = 0; i < docs_magnitude.Length; i++)
                {            
                    double sumatory = 0;

                    
                        foreach(KeyValuePair<string, double> element in docs_tfidf[i])
                        {
                            sumatory+=element.Value*element.Value;
                        }     
                        
                        docs_magnitude[i] = Math.Sqrt(sumatory);
       
                }
            
            return docs_magnitude;
                    
        }

        public static double QueryMagnitude(Dictionary<string, double> tfidf_query)
        {
            double sum_query = 0;

                foreach(KeyValuePair<string, double> element in tfidf_query)
                {
                    sum_query+=element.Value*element.Value;
                }

            return Math.Sqrt(sum_query);
        }       
        
        public static double[] CosineSimilarity(Dictionary<string, double> query_tfidf, List<Dictionary<string, double>> tfidf_list, double query_magnitude, double[] docs_magnitude)
        {

            double[] dot_sum = new double[tfidf_list.Count];

                for(int i = 0; i<tfidf_list.Count; i++)
                {
                    foreach(KeyValuePair<string, double> element in query_tfidf)
                    {
                        if(tfidf_list[i].ContainsKey(element.Key))
                        {
                            dot_sum[i] += element.Value*tfidf_list[i][element.Key];
                        }
                        
                    }
                }

            //Calcula los cosenos de los angulos

            double[] cosine_similarity = new double[tfidf_list.Count];

                for(int i = 0; i<cosine_similarity.Length; i++)
                {
                    double magnitudes = query_magnitude*docs_magnitude[i];
                    
                    if(magnitudes!=0)
                    cosine_similarity[i] = ((dot_sum[i]/magnitudes));
                    else 
                    cosine_similarity[i] = (0);
                    
                }

            
            return cosine_similarity;
        }

        
        public static void Operators(double[] cosine_similarity, (bool[], bool[], int[], bool, string, string) operators_in_words, List<string> query, List<Dictionary<string, double>> tfidf_list, List<List<string>> list_of_lists)
        {            

            for(int i = 0 ; i < query.Count; i++)
            {

                if(operators_in_words.Item1[i])
                {
                    for(int j = 0; j<tfidf_list.Count; j++)
                    {
                        if(!tfidf_list[j].ContainsKey(query[i]))
                        {
                            cosine_similarity[j] = 0;
                        }
                    }
                }

                else if(operators_in_words.Item2[i])
                {
                    for(int j = 0; j<tfidf_list.Count; j++)
                    {
                        if(tfidf_list[j].ContainsKey(query[i]))
                        {
                            cosine_similarity[j] = 0;
                        }

                    }
                } 
                else if(operators_in_words.Item3[i] != 0)
                {
                    for(int j = 0; j<tfidf_list.Count; j++)
                    {
                        if(tfidf_list[j].ContainsKey(query[i]))
                        {
                            cosine_similarity[j] *= (operators_in_words.Item3[i] + 2);
                        }
                    }

                }
            }


            if(operators_in_words.Item4)
            {
                Proximity_Operator(cosine_similarity, operators_in_words.Item5, operators_in_words.Item6, query, tfidf_list, list_of_lists);
            }
        }


        public static (bool[], bool[], int[], bool, string word1, string word2) OperatorsUsed(List<string> query)
        {
            bool[] most_appear_operator = new bool[query.Count];
            bool[] cant_appear_operator = new bool[query.Count];
            int[] importance_operator = new int[query.Count];
            bool proximity_operator = false;
            string word1 = "";
            string word2 = "";

            for(int i = 0; i<query.Count; i++)
            {
                if(query[i][0] == '^')
                {
                    most_appear_operator[i] = true;
                    query[i] = query[i].Remove(0, 1);

                }
                else if(query[i][0] == '!')
                {
                    cant_appear_operator[i] = true;
                    query[i] = query[i].Remove(0, 1);

                }
                else 
                {
                    for (int j = 0; j<query[i].Length; j++)
                    {
                        if(query[i][j] == '*')
                        {
                            importance_operator[i]++;
                        }
                        else break;
                    }

                    query[i] = query[i].Remove(0, importance_operator[i]);
                }

                if(query[i] == "~")
                {
                    proximity_operator = true;
                    word1 = query[i-1];
                    word2 = query[i+1];
                    query.Remove(query[i]);
                        
                }

            }

            return (most_appear_operator, cant_appear_operator, importance_operator, proximity_operator, word1, word2);
        }


        public static void Proximity_Operator(double[] cosine_similarity, string word1, string word2, List<string> query, List<Dictionary<string, double>> tfidf_list, List<List<string>> list_of_lists)
        {
                for(int i=0; i<tfidf_list.Count; i++)
                {
                    List<int> word1_positions = new();
                    List<int> word2_positions = new();


                    foreach(KeyValuePair<string, double> word in tfidf_list[i])
                    {
                        if(word.Key == word1 || word.Key == word2)
                        {

                            for(int j = 0; j<list_of_lists[i].Count; j++)
                            {
                                if(list_of_lists[i][j] == word1)
                                {
                                    word1_positions.Add(j);
                                }
                                else if(list_of_lists[i][j] == word2)
                                {
                                    word2_positions.Add(j);
                                }
                            }

                        break;

                        }                          
                        
                    }

                    
                    if((word1_positions.Count != 0) && (word2_positions.Count != 0))
                    {
                        int temporary = 0;
                        int difference = int.MaxValue;
                                                    
                            for(int n = 0; n < word1_positions.Count; n++)
                            {
                                for(int m = 0; m < word2_positions.Count; m++)
                                {
                                    
                                    temporary = (word1_positions[n] - word2_positions[m]);
                                    if(Math.Sign(temporary) == -1)
                                    {
                                        temporary = -temporary;
                                    }
                                    if(difference > temporary)
                                    difference = temporary;

                                }           

                            }
                        cosine_similarity[i] = cosine_similarity[i] * (5 + 1/difference);
  
                    }
                    
                }
        } 

        static string[] MostImportantDocs(double[] cosines, int[] highest_docs_positions)
        {

            string[] highest_TFIDF_docs = new string[highest_docs_positions.Length];
            

                for(int i = 0; i<highest_TFIDF_docs.Length; i++)
                {
                    highest_TFIDF_docs[i] = GetFilesNames()[highest_docs_positions[i]];
                }

                return highest_TFIDF_docs;

        }

        public static int[] GetArrayLongestPositions(double[] cosine_similarity)
        {

            List<int> longest_positions = new();

            for(int i = 0; i<5; i++)
            {
                double temporary = 0;
                int position = -1;

                for(int j = 0; j<cosine_similarity.Length; j++)
                {

                    if(cosine_similarity[j] > temporary)
                    {
                        temporary = cosine_similarity[j];
                        position = j;
                    }
                }
                if(position != -1)
                {
                    longest_positions.Add(position);
                    cosine_similarity[position] = -1;
                }
                else
                    break;
                
            }

            if (longest_positions.Count != 0)
            {
                return longest_positions.ToArray();

            }            
            else
            {
                longest_positions.Add(-1);
                return longest_positions.ToArray();
            }
        }
          
    

        //Muestra una parte de los textos donde hay resultados de la query. Arreglar.
         
        public static List<string> OrderQueryTFIDF(Dictionary<string, double> query_tfidf)
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

            List<string> query_list = OrderQueryTFIDF(query_tfidf);

            string readed_doc = File.ReadAllText(doc_path).Replace('\n' , ' ').Replace('\r' , ' ');

            char[] separators = {' ', '\n', '\t'};

            string readed_doc_normalized = Normalize(readed_doc);
           
            string snippet = "";
           
            for(int i = 0; i<query_list.Count; i++)
            {
                if(doc.Contains(query_list[i]))
                {
                    int position = readed_doc_normalized.IndexOf(query_list[i] + " ");
                    int last_position = 0;

                    while(position == -1)
                    {
                        
                     if(!Char.IsLetter(readed_doc_normalized, readed_doc_normalized.IndexOf(query_list[i], last_position) + query_list[i].Length + 1))
                     {
                        position = readed_doc_normalized.IndexOf(query_list[i], last_position);
                        
                     }
                     else
                     {
                        last_position = readed_doc_normalized.IndexOf(query_list[i], last_position + query_list[i].Length);
                     }

                    }




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
                        snippet = "..." + readed_doc.Substring(position, readed_doc.Length-position);
                    }
                    break;
                }
            }

            return snippet;
    }

    }
}