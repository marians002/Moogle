using System.Text.Json;
namespace MoogleEngine

{



    public static class Start
    {

        //Crea la lista de listas.
        internal static List<List<string>> list_of_lists = new();
        //Crea el diccionario de IDF
        internal static Dictionary<string, double> IDF = new();
        //Crea la lista de diccionarios de TF
        internal static List<Dictionary<string, double>> TF_list = new();
        //Crea la lista de diccionarios de TFIDF.
        internal static List<Dictionary<string, double>> tfidf_list = new();
        //Crea el array para la magnitud de los documentos
        internal static double[]? docs_magnitude;
        //Crea y almacena el diccionario para los sinonimos:
        internal static Dictionary<string, string[]> Synonyms = JsonSerializer.Deserialize<Dictionary<string, string[]>>(File.ReadAllText(Directory.GetCurrentDirectory() + "/synonyms.json"));

            
        //Realiza todo el preprocesamiento necesario
        public static void LoadFiles()
        {
             
            //Almacena la lista de listas.
            list_of_lists = Start.TxtReader();
            //Comienza a calcular tfidf.
            Start.TFIDF_Calculator(IDF, TF_list, list_of_lists);
            //Almacena tfidf en una lista de diccionarios.
            tfidf_list = Start.TFIDF(IDF, TF_list);
            //Calcula la magnitud de los documentos.
            docs_magnitude = Start.DocumentsMagnitude(tfidf_list);

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
            path = Path.Join(path, "..", "/Content");
            

            return Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
        }

        //Devuelve una lista de las palabras separadas
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

        //Calcula TF e IDF de los documentos.
        public static void TFIDF_Calculator(Dictionary<string, double> IDF, List<Dictionary<string, double>> TF_list, List<List<string>> list_of_lists)
        {
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

        }
        
        //Devuelve el TFIDF final de todos los documentos.
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

        //Calcula la magnitud de los documentos.
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
            

    }
}