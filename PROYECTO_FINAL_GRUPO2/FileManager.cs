using System.Diagnostics;

namespace PROYECTO_FINAL_GRUPO2
{
    internal class FileManager
    {

        // Método para procesar un archivo.
        public static bool ProcessFile(string filePath, bool encrypt, bool compress, string password, bool useLZW, bool useHuffman, string outputPath)
        {
            try
            {
                // Verificar si el archivo ya ha sido comprimido/encriptado
                if (filePath.EndsWith(".lzw") || filePath.EndsWith(".huff") || filePath.EndsWith(".lzw.huff") || filePath.EndsWith(".enc"))
                {
                    Console.WriteLine("El archivo ya ha sido comprimido o encriptado previamente y no se puede volver a procesar.");
                    return false;
                }

                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                byte[] originalData = File.ReadAllBytes(filePath);
                byte[] data = originalData;
                string newFilePath = Path.Combine(outputPath, Path.GetFileName(filePath));

                if (compress)
                {
                    if (useLZW && useHuffman)
                    {
                        data = CompressionDecompression.CompressWithHuffman(CompressionDecompression.CompressWithLZW(data));
                        newFilePath += ".lzw.huff";
                    }
                    else if (useLZW)
                    {
                        data = CompressionDecompression.CompressWithLZW(data);
                        newFilePath += ".lzw";
                    }
                    else
                    {
                        data = CompressionDecompression.CompressWithHuffman(data);
                        newFilePath += ".huff";
                    }
                }

                if (encrypt)
                {
                    data = EncryptDecrypt.EncryptData(data, password);
                    newFilePath += ".enc";
                }

                File.WriteAllBytes(newFilePath, data);
                stopwatch.Stop();

                double compressionRate = compress ? (1 - (double)data.Length / originalData.Length) : -1;

                LogOperation("Procesado", filePath, newFilePath, stopwatch.Elapsed, compressionRate, outputPath);
                Console.WriteLine($"Archivo procesado y guardado en: {newFilePath}");
                

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al procesar el archivo '{filePath}': {ex.Message}");
                
                return false;
            }
            
        }

        // Método para recuperar un archivo.
        public static void RecoverFile(string filePath, string password, string outputPath)
        {
            try
            {
                // Verificar que el archivo tenga una extensión válida para recuperación
                if (filePath.EndsWith(".txt") && !filePath.Contains(".enc") && !filePath.Contains(".lzw") && !filePath.Contains(".huff"))
                {
                    Console.WriteLine($"El archivo '{Path.GetFileName(filePath)}' no ha sido comprimido ni encriptado y no se puede recuperar.");
                    return;
                }

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                byte[] data = File.ReadAllBytes(filePath);
                string recoveredFilePath = Path.Combine(outputPath, Path.GetFileName(filePath));

                if (filePath.EndsWith(".enc"))
                {
                    bool passwordCorrect = false;
                    while (!passwordCorrect)
                    {
                        Console.WriteLine("Ingrese la contraseña para desencriptar:");
                        string inputPassword = Console.ReadLine();
                        try
                        {
                            data = EncryptDecrypt.DecryptData(data, inputPassword);
                            recoveredFilePath = recoveredFilePath.Replace(".enc", "");
                            passwordCorrect = true;
                            Console.Clear();
                        }
                        catch
                        {
                            while (true)
                            {
                                Console.WriteLine("Contraseña incorrecta. ¿Desea intentar de nuevo? (s/n)");
                                string? respuesta = Console.ReadLine()?.ToLower();

                                if (respuesta == "s")
                                {
                                    Console.Clear();
                                    break;
                                }
                                else if (respuesta == "n")
                                {
                                    return;
                                }
                                else
                                {
                                    Console.WriteLine("Opción no válida. Por favor, ingrese 's' para sí o 'n' para no.");
                                    Console.ReadKey();
                                    Console.Clear();
                                }
                            }
                        }
                    }
                }

                if (recoveredFilePath.EndsWith(".lzw.huff"))
                {
                    data = CompressionDecompression.DecompressWithLZW(CompressionDecompression.DecompressWithHuffman(data));
                    recoveredFilePath = recoveredFilePath.Replace(".lzw.huff", "");
                    Console.WriteLine("Descomprimiendo archivo con LZW + Huffman...");
                    Console.WriteLine("Presione cualquier tecla para continuar");

                }
                else if (recoveredFilePath.EndsWith(".lzw"))
                {
                    data = CompressionDecompression.DecompressWithLZW(data);
                    recoveredFilePath = recoveredFilePath.Replace(".lzw", "");
                    Console.WriteLine("Descomprimiendo archivo con LZW...");
                    Console.WriteLine("Presione cualquier tecla para continuar");

                }
                else if (recoveredFilePath.EndsWith(".huff"))
                {
                    data = CompressionDecompression.DecompressWithHuffman(data);
                    recoveredFilePath = recoveredFilePath.Replace(".huff", "");
                    Console.WriteLine("Descomprimiendo archivo con Huffman...");
                    Console.WriteLine("Presione cualquier tecla para continuar");

                }

                File.WriteAllBytes(recoveredFilePath, data);
                stopwatch.Stop();

                // Pasar -1 como compressionRate para evitar mostrar la tasa de compresión en el log
                LogOperation("Recuperado", filePath, recoveredFilePath, stopwatch.Elapsed, -1, outputPath);
                Console.WriteLine($"Archivo recuperado: {recoveredFilePath}");
                
               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al recuperar el archivo '{filePath}': {ex.Message}");
              
               
            }

           
        }

        // Método para procesar una carpeta.
        public static void ProcessDirectory(string directoryPath, bool encrypt, bool compress, string password, bool useLZW, bool useHuffman, string outputPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                    throw new DirectoryNotFoundException("El directorio especificado no existe.");
                    

                string operations = (compress ? "Comprimido" : "") + (encrypt ? "_Encriptado" : "");
                string newDirectoryPath = Path.Combine(outputPath, Path.GetFileName(directoryPath) + operations + "_processed");

                bool anyFileProcessed = false;

                foreach (var file in Directory.GetFiles(directoryPath))
                {
                    // Procesa cada archivo en la carpeta
                    bool fileProcessed = ProcessFile(file, encrypt, compress, password, useLZW, useHuffman, newDirectoryPath);
                    anyFileProcessed |= fileProcessed;
                }

                if (anyFileProcessed) // Solo crea la carpeta si se procesó al menos un archivo
                {
                    Console.WriteLine($"Carpeta procesada y guardada en: {newDirectoryPath}");
                    
                    
                }
                else
                {
                    Console.WriteLine("Todos los archivos en la carpeta ya están comprimidos o encriptados. No se realizó ninguna operación.");
                    
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al procesar el directorio '{directoryPath}': {ex.Message}");
                
            }
        }

        // Método para recuperar una carpeta.
        public static void RecoverDirectory(string directoryPath, string password, string outputPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath)){
                    throw new DirectoryNotFoundException("El directorio especificado no existe.");

                    
                }

                string recoveredDirectoryPath = Path.Combine(outputPath, $"{Path.GetFileName(directoryPath)}_recovered");

                bool anyFileRecovered = false; // Variable para verificar si al menos un archivo fue recuperado

                var files = Directory.GetFiles(directoryPath);

                foreach (var file in files)
                {
                    // Verificar que el archivo tenga una extensión válida para recuperación
                    if (file.EndsWith(".txt") && !file.Contains(".enc") && !file.Contains(".lzw") && !file.Contains(".huff"))
                    {
                        Console.WriteLine($"El archivo '{Path.GetFileName(file)}' no ha sido comprimido ni encriptado y no se puede recuperar.");
                        
                        continue;
                    }

                    try
                    {
                        byte[] data = File.ReadAllBytes(file);
                        string recoveredFilePath = Path.Combine(recoveredDirectoryPath, Path.GetFileName(file));

                        // Procesar archivo según sus extensiones
                        if (file.EndsWith(".enc"))
                        {
                            bool passwordCorrect = false;
                            while (!passwordCorrect)
                            {
                                try
                                {
                                    Console.WriteLine($"Ingrese la contraseña para desencriptar el archivo: {Path.GetFileName(file)}");
                                    string inputPassword = Console.ReadLine();
                                    data = EncryptDecrypt.DecryptData(data, inputPassword);
                                    recoveredFilePath = recoveredFilePath.Replace(".enc", "");
                                    passwordCorrect = true;
                                    
                                }
                                catch
                                {
                                    while (true)
                                    {
                                        Console.WriteLine("Contraseña incorrecta. ¿Desea intentar de nuevo? (s/n)");
                                        string? respuesta = Console.ReadLine()?.ToLower();

                                        if (respuesta == "s")
                                        {
                                            Console.Clear();
                                            break;
                                        }
                                        else if (respuesta == "n")
                                        {
                                            return;
                                        }
                                        else
                                        {
                                            Console.WriteLine("Opción no válida. Por favor, ingrese 's' para sí o 'n' para no.");
                                            Console.ReadKey();
                                            Console.Clear();
                                        }
                                    }

                                }
                                
                            }
                        }

                        if (recoveredFilePath.EndsWith(".lzw.huff"))
                        {
                            data = CompressionDecompression.DecompressWithLZW(CompressionDecompression.DecompressWithHuffman(data));
                            recoveredFilePath = recoveredFilePath.Replace(".lzw.huff", "");
                            Console.WriteLine("Descomprimiendo archivo con LZW + Huffman...");
                            Console.WriteLine("Presione cualquier tecla para continuar");

                        }
                        else if (recoveredFilePath.EndsWith(".lzw"))
                        {
                            data = CompressionDecompression.DecompressWithLZW(data);
                            recoveredFilePath = recoveredFilePath.Replace(".lzw", "");
                            Console.WriteLine("Descomprimiendo archivo con LZW...");
                            Console.WriteLine("Presione cualquier tecla para continuar");

                        }
                        else if (recoveredFilePath.EndsWith(".huff"))
                        {
                            data = CompressionDecompression.DecompressWithHuffman(data);
                            recoveredFilePath = recoveredFilePath.Replace(".huff", "");
                            Console.WriteLine("Descomprimiendo archivo con Huffman...");
                            Console.WriteLine("Presione cualquier tecla para continuar");

                        }

                        // Guardar el archivo recuperado y actualizar el estado de recuperación
                        if (!Directory.Exists(recoveredDirectoryPath))
                            Directory.CreateDirectory(recoveredDirectoryPath);

                        File.WriteAllBytes(recoveredFilePath, data);
                        anyFileRecovered = true;

                        LogOperation("Recuperado", file, recoveredFilePath, TimeSpan.Zero, -1, outputPath);
                        Console.WriteLine($"Archivo recuperado: {recoveredFilePath}");
                        Console.WriteLine("Presione cualquier tecla para continuar");
                        Console.ReadKey();
                        Console.Clear();
                        

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al procesar el archivo {file}: {ex.Message}");
                        Console.WriteLine("¿Desea continuar con los siguientes archivos? (s/n)");
                        if (Console.ReadLine()?.ToLower() != "s") 
                        {
                            return;
                        }
                       
                    }
                }

                if (anyFileRecovered)
                {
                    Console.WriteLine($"Proceso de recuperación completado en: {recoveredDirectoryPath}");
                    
                }
                else
                {
                    Console.WriteLine("No se recuperaron archivos ya que todos estaban sin compresión o encriptación.");
                    

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al recuperar el directorio '{directoryPath}': {ex.Message}");
                

            }
        }

        // Método para registrar una operación en el log.
        public static void LogOperation(string operation, string originalFile, string newFile, TimeSpan timeTaken, double compressionRate, string outputPath)
        {
            string logFilePath = Path.Combine(outputPath, "operations_log.txt");
            string logEntry = $"{DateTime.Now}: {operation} realizada en {originalFile}, generado {newFile}, Tiempo: {timeTaken.TotalSeconds}s";

            // Agregar la tasa de compresión solo si se ha aplicado compresión
            if (compressionRate != -1)
            {
                logEntry += $", Tasa de compresión: {compressionRate * 100:0.00}%";
            }

            File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }
    }
}
