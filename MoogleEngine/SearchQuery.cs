namespace MoogleEngine
{

    internal static class SearchQuery
    {

        //Devuelve los operadores empleados y las palabras que los contienen.
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

        //Calcula TFIDF de la query unicamente
        public static Dictionary<string, double> TFIDF(Dictionary<string, double> idf, List<string> query)
        {

            Dictionary<string, double> tfidf_query = new();

                foreach(string word in query)
                {
                    if(!tfidf_query.ContainsKey(word))
                    {
                        tfidf_query.Add(word, 1);
                    }
                    else
                    {
                        tfidf_query[word]++;
                    }
                            
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

        //Calcula la magnitud del query
        public static double QueryMagnitude(Dictionary<string, double> tfidf_query)
        {
            double sum_query = 0;

                foreach(KeyValuePair<string, double> element in tfidf_query)
                {
                    sum_query+=element.Value*element.Value;
                }

            return Math.Sqrt(sum_query);
        }    

        //Calcula los cosenos de los angulos entre los vectores
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

        //Modifica la similitud de cosenos segun los operadores empleados
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
        
        //Modifica la similitud de cosenos por el operador de cercania (si fue usado).
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

        //Devuelve las palabras mas cercanas a la busqueda realizada
        //que esten contenidas dentro de los documentos.
        public static string Suggestions(Dictionary<string, double> IDF, List<string> query_list)
        {
            string[] nearest_words = new string[query_list.Count];
            int distance;

            for(int h = 0; h<query_list.Count; h++)
            {
                int comparer = int.MaxValue;

                if(!IDF.ContainsKey(query_list[h]))
                {
                    foreach(KeyValuePair<string, double> word in IDF)
                    {                            
                        distance = HammilDistance(word.Key, query_list[h]);

                        if(distance<comparer)
                        {
                            comparer = distance;
                            nearest_words[h] = word.Key;
                        }
                    }
                }
                else
                {
                    nearest_words[h] = query_list[h];
                }
            }

            return String.Join(' ' ,nearest_words);


            //Calcula la Distancia Hammil que existe entre las palabras
            static int HammilDistance(string a, string b)
            {
                int distance = 0;

                if(a.Length > b.Length)
                {
                    string c = b;
                    b = a;
                    a = c;
                }
                    for(int i = 0; i<a.Length; i++)
                    {
                        
                            if(a[i] != b[i])
                            distance++;
                        
                
                    }
                    distance += Math.Abs(b.Length - a.Length);
                    
                return distance;
            }
        }

    }
}