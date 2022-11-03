namespace MoogleEngine
{

    internal static class Results
    {

        //Devuelve la posicion que ocupan los documentos mas importantes.
        public static int[] GetHighestArrayPositions(double[] cosine_similarity)
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

        //Devuelve la ruta de los documentos con mayor TFIDF
        public static string[] MostImportantDocs(double[] cosines, int[] highest_docs_positions)
        {

            string[] highest_TFIDF_docs = new string[highest_docs_positions.Length];
            

                for(int i = 0; i<highest_TFIDF_docs.Length; i++)
                {
                    highest_TFIDF_docs[i] = Start.GetFilesNames()[highest_docs_positions[i]];
                }

                return highest_TFIDF_docs;

        }

        //Devuelve una lista ordenada del TFIDF de los documentos de mayor a menor
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
        
        //Devuelve un fragmento del documento original donde se encontro el query (o parte de el).
       public static string GetSnippet(string doc_path, Dictionary<string, double> query_tfidf, List<string> doc)
        {

            List<string> query_list = OrderQueryTFIDF(query_tfidf);

            string readed_doc = File.ReadAllText(doc_path).Replace('\n' , ' ').Replace('\r' , ' ');

            char[] separators = {' ', '\n', '\t'};

            string readed_doc_normalized = Start.Normalize(readed_doc);
           
            string snippet = "";
           
            
            for(int i = 0; i<query_list.Count; i++)
            {

                //Va recorriendo el documento hasta que encuentre una de las palabras del query.
                if(doc.Contains(query_list[i]))
                {
                    for(int m = 0; m<doc.Count; m++)
                    {
                        if(doc[m] == query_list[i])
                        break;
                    }
                    
                    int position = readed_doc_normalized.IndexOf(" " + query_list[i] + " ");
                    int last_position = 0;


                        while(position == -1)
                        {
                    
                            if(!Char.IsLetter(readed_doc_normalized, readed_doc_normalized.IndexOf(query_list[i], last_position) + query_list[i].Length) && !Char.IsLetter(readed_doc_normalized, readed_doc_normalized.IndexOf(query_list[i], last_position) - 1)) 
                            {
                                position = readed_doc_normalized.IndexOf(query_list[i], last_position);

                            }
                            else
                            {
                                last_position = readed_doc_normalized.IndexOf(query_list[i], last_position + query_list[i].Length);
                            }

                        }
                        


                    if(readed_doc_normalized.Length > position + 200)
                    {
                        snippet = "..." + readed_doc.Substring(position, 200);
                        for(int k = position+200 ; k<readed_doc_normalized.Length && Char.IsLetter(readed_doc[k]); k++)
                        {
                            snippet += readed_doc[k];
                        }
                        snippet += "...";
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