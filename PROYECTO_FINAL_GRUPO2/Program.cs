using PROYECTO_FINAL_GRUPO2;

//revision No.7
class ProyectoFinal
{   
    static void Main(string[] args){
        bool exit = false;

        while (!exit)
        {
            Console.Clear();
            Console.WriteLine("Seleccione una operación:");
            Console.WriteLine("1. Comprimir");
            Console.WriteLine("2. Encriptar");
            Console.WriteLine("3. Ambos (Comprimir y Encriptar)");
            Console.WriteLine("4. Recuperar archivos");
            Console.WriteLine("5. Salir");

            if (!int.TryParse(Console.ReadLine(), out int option) || option < 1 || option > 5)
            {
                Console.WriteLine("\nOpción no válida. Intente nuevamente.");
                Console.ReadKey();
                continue;
            }

            if (option == 5)
            {
                exit = true;
                break;
            }

            Console.Clear();
            Console.WriteLine("Ingrese el archivo o carpeta:");
            string path = Console.ReadLine();
            Console.Clear();

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Console.WriteLine("\nLa ruta del archivo o carpeta especificada no existe.");
                Console.ReadKey();
                continue;
            }

            Console.Clear();
            Console.WriteLine("Ingrese la ubicación donde se guardará el archivo o carpeta procesada:");
            string outputPath = Console.ReadLine();

            if (!Directory.Exists(outputPath))
            {
                Console.WriteLine("\nLa ruta de salida especificada no existe.");
                Console.ReadKey();
                continue;
            }

            bool encrypt = option == 2 || option == 3;
            bool compress = option == 1 || option == 3;
            bool useLZW = false, useHuffman = false;
            string password = null;

            // Selección de algoritmo de compresión
            if (compress)
            {
                bool validCompressionOption = false;
                while (!validCompressionOption)
                {
                    Console.Clear();
                    Console.WriteLine("Seleccione el algoritmo de compresión:");
                    Console.WriteLine("1. LZW");
                    Console.WriteLine("2. LZW + Huffman");


                    if (int.TryParse(Console.ReadLine(), out int compressionOption) && (compressionOption == 1 || compressionOption == 2))
                    {
                        useLZW = true;
                        useHuffman = compressionOption == 2;
                        validCompressionOption = true;
                        Console.Clear();
                    }
                    else
                    {
                        Console.WriteLine("Opción de compresión no válida. Intente nuevamente.");
                        Console.ReadKey();
                        Console.Clear();
                    }
                }
            }

            // Solicitar contraseña si se selecciona encriptación
            if (encrypt)
            {
                Console.Clear();
                Console.WriteLine("Ingrese la contraseña para encriptar/desencriptar:");
                password = Console.ReadLine();
                Console.Clear();
            }

            try
            {
                if (option == 4) // Recuperación de archivos
                {
                    if (File.Exists(path))
                    {
                        Console.Clear();
                        FileManager.RecoverFile(path, password, outputPath);
                        
                    }
                    else if (Directory.Exists(path))
                    {
                        Console.Clear();
                        FileManager.RecoverDirectory(path, password, outputPath);
                        
                    }
                }
                else // Procesar archivo o carpeta (comprimir, encriptar o ambos)
                {
                    if (File.Exists(path))
                    {
                        FileManager.ProcessFile(path, encrypt, compress, password, useLZW, useHuffman, outputPath);
                        
                    }
                    else if (Directory.Exists(path))
                    {
                        FileManager.ProcessDirectory(path, encrypt, compress, password, useLZW, useHuffman, outputPath);
                       
                    }
                }
                Console.WriteLine("\n");
                Console.WriteLine("Presione cualquier tecla para volver al menú...");
                Console.ReadKey();
                Console.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
