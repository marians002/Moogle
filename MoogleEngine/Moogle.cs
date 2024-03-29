﻿//Moogle by Marian S. C121.
namespace MoogleEngine;
public static class Moogle
{
    public static SearchResult Query(string query, int recursive_counter, string suggestions_param) 
    {           

            char[] query_separators = {' ', '=', '`', ';', '\'', '\t', '.', ',', ':', '-', '_', '/','+', '%','?', '[', ']', '(', ')', '{', '}', '|', 'ª' , 'º', '<', '>' , '¡', '¿', '»', '«', '…', '‥' , '&', '#' , '@', '՛', '՝'} ;
            // Los caracteres !, ^, ~ y * no han sido incluidos porque son para los operadores de busqueda.
            //En el query no deben eliminarse, pero en los txt si.

            //Almacena el query en una lista de palabras casi normalizadas
            List<string> query_list = Start.WordsSeparator(query, query_separators);

            //Almacena que operadores fueron usados en el query
            (bool[], bool[], int[], bool, string, string) operators_in_words = SearchQuery.OperatorsUsed(query_list);

            //Calcula TFIDF del query
            Dictionary<string, double> query_tfidf = SearchQuery.TFIDF(Start.IDF, query_list);
           
            //Calcula magnitud del query
            double query_magnitude = SearchQuery.QueryMagnitude(query_tfidf);

            //Calcula la similitud de cosenos y devuelve cuales son los documentos mas relevantes.
            double[] cosine_similarity = SearchQuery.CosineSimilarity(query_tfidf, Start.tfidf_list, query_magnitude, Start.docs_magnitude);
                
            /*Realiza las operaciones pertinentes con los operadores que modificaran
            la relevancia de cada documento.*/
            SearchQuery.Operators(cosine_similarity, operators_in_words, query_list, Start.tfidf_list, Start.list_of_lists);
       
            //Devuelve la posicion que ocupan los documentos mas relevantes.
            //Si ningun documento se corresponde con el query, devuelve -1. 
            int[] docs_positions = Results.GetHighestArrayPositions(cosine_similarity);

            //Si existen resultados, muestra un fragmento de cada documento donde aparezca parte
            //del query. 
            if(docs_positions[0] != -1)
            {            
                //Devuelve la ruta de los documentos más importantes.
                string[] docs_path = Results.MostImportantDocs(cosine_similarity, docs_positions);

                string[] snippets = new string[docs_path.Length];

                SearchItem[] items = new SearchItem[snippets.Length];

                //Almacena un fragmento de texto donde se encuentre parte de la búsqueda realizada
                for(int i = 0; i<items.Length; i++)
                {
                    snippets[i] = Results.GetSnippet(docs_path[i], query_tfidf, Start.list_of_lists[docs_positions[i]]);           
                    items[i] = new SearchItem(Path.GetFileNameWithoutExtension(docs_path[i]), snippets[i], cosine_similarity[i], "file://" + docs_path[i]);
         
                
                }
                
                return new SearchResult(items, suggestions_param);
                
            }
            
            // En caso de no existir resultados, busca sinonimos.
            else if(recursive_counter<1)
            {           
                //Almacena las sugerencias a partir de la busqueda original.
                    suggestions_param = SearchQuery.Suggestions(Start.IDF, query_list);

                //Añade a la busqueda original todos los sinonimos de las palabras del query 
                    foreach(string word in query_list)
                    {
                        try
                        {

                            string[] synonyms = Start.Synonyms[word];

                            for(int i = 0; i<synonyms.Length; i++)  
                            {
                                query+= " " + synonyms[i];
                            }
                         
                        }
                        catch(KeyNotFoundException ex){}
                    }
                
                //Aumenta un contador que representa que ya los sinonimos se agregaron al query
                    recursive_counter++;
                
                return Query(query, recursive_counter, suggestions_param);
            }

            /*Si con los sinonimos tampoco se encontraron documentos relevantes, pasa a decir 
              que no fueron hallados resultados relevantes y muestra una sugerencia de las
              palabras mas cercanas a aquellas introducidas en el query original.
            */
            else
            {  
                SearchItem[] items = new SearchItem[1];
                items[0] = new SearchItem("No se encontraron resultados", "Pruebe a hacer otra búsqueda", 0, "#");
                return new SearchResult(items, suggestions_param);
            }
 
    }
    
}

