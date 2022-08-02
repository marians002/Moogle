using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Program
{
    public class Program
    {

    
        public static void Main(string[] args)
        {
            txtReader(); 
            
            
      
        }
    

            //Metodo que separa un string en un array de strings por palabras
        public static string[] Separador_Palabras(string s)
        {

            char[] delimitadores = {' ', '=', '`', ';', '\'', '\t', '.', ',', ':', '-', '_', '/','+', '%','?', '[', ']', '(', ')', '{', '}', '|'} ;
            // Los caracteres !, ^, ~ y * no han sido incluidos porque son para los operadores de busqueda.
            s = s.Replace('\n' , ' ').Replace('\r',' ');
            //Elimina los break line.

            return s.Split(delimitadores, StringSplitOptions.RemoveEmptyEntries);
            
        
        }
         


        public static void txtReader()
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

            
            for(int i = 0; i<path.Length; i++){
                
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

                Diccionario(Conjunto_de_Listas);

        }

        public static int WordCount(string word, List<string> lista){
             
             int count = 0;
            
            for(int i=0; i<lista.Count; i++){

                if(lista[i] == word)
                count++;
            }

            return count;
            
        }

        public static Dictionary<string, int> Diccionario(List<List<string>> lista){

            Dictionary<string, int> Dicc = new();


            for(int i = 0; i < lista.Count; i++){

                foreach(string palabra in lista[i]){
                    Dicc[palabra] = WordCount(palabra, lista[i]);

                    
                }
                
            }       

            foreach(KeyValuePair<string, int> value in Dicc){
                    Console.WriteLine(value);
            }
            


            return Dicc;
        }        


           
    }
}