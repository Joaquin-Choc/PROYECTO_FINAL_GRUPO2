using System.Text;

namespace PROYECTO_FINAL_GRUPO2
{
    internal class CompressionDecompression
    {
        // Clase para representar un nodo del árbol de Huffman
        public class HuffmanNode
        {
            public byte Value { get; set; }
            public int Frequency { get; set; }
            public HuffmanNode Left { get; set; }
            public HuffmanNode Right { get; set; }
            public bool IsLeaf => Left == null && Right == null;
        }

        public class HuffmanCompression
        {
            private Dictionary<byte, string> encodingMap;
            private HuffmanNode root;

            public byte[] Compress(byte[] data)
            {
                if (data == null || data.Length == 0)
                    throw new ArgumentException("Los datos de entrada no pueden ser nulos o vacíos.");

                // Calcular frecuencias
                Dictionary<byte, int> frequencies = CalculateFrequencies(data);

                // Filtrar y contar solo frecuencias válidas
                var validFrequencies = frequencies.Where(f => f.Value > 0).ToDictionary(f => f.Key, f => f.Value);

                if (validFrequencies.Count == 0)
                    throw new InvalidOperationException("No se encontraron frecuencias válidas en los datos");

                // Construcción de árbol de Huffman
                root = BuildHuffmanTree(validFrequencies);
                if (root == null)
                    throw new InvalidOperationException("No se pudo construir el árbol de Huffman");

                encodingMap = new Dictionary<byte, string>();
                GenerateHuffmanCodes(root, "", encodingMap);

                // Preparar datos comprimidos
                List<byte> compressedData = new List<byte> { 0 }; // Marcador de caso normal

                // Guardar número de entradas de frecuencia válidas
                compressedData.AddRange(BitConverter.GetBytes(validFrequencies.Count));

                // Guardar cada byte con su frecuencia
                foreach (var pair in validFrequencies)
                {
                    compressedData.Add(pair.Key);
                    compressedData.AddRange(BitConverter.GetBytes(pair.Value));
                }

                // Convertir datos a bits
                StringBuilder bitString = new StringBuilder();
                foreach (byte b in data)
                {
                    if (encodingMap.ContainsKey(b))
                        bitString.Append(encodingMap[b]);
                }

                // Calcular y agregar padding
                int paddingLength = (8 - (bitString.Length % 8)) % 8;
                compressedData.Add((byte)paddingLength);

                // Convertir bits a bytes
                for (int i = 0; i < bitString.Length; i += 8)
                {
                    string chunk = (i + 8 <= bitString.Length)
                        ? bitString.ToString(i, 8)
                        : bitString.ToString(i, bitString.Length - i).PadRight(8, '0');
                    compressedData.Add(Convert.ToByte(chunk, 2));
                }

                // Mensaje de depuración para verificar la compresión finalizada
                Console.WriteLine($"Compresión completada. Tamaño de datos comprimidos: {compressedData.Count} bytes.");

                return compressedData.ToArray();
            }



            public byte[] Decompress(byte[] compressedData)
            {
                if (compressedData == null || compressedData.Length < 2)
                    throw new ArgumentException("Datos comprimidos inválidos");

                int position = 0;
                byte caseMarker = compressedData[position++];

                if (caseMarker == 1)
                {
                    if (position + 5 > compressedData.Length)
                        throw new ArgumentException("Datos comprimidos truncados");

                    byte value = compressedData[position++];
                    int count = BitConverter.ToInt32(compressedData, position);
                    return Enumerable.Repeat(value, count).ToArray();
                }

                // Leer número de entradas en la tabla de frecuencias como entero (4 bytes)
                if (position + 4 > compressedData.Length)
                    throw new ArgumentException("Datos comprimidos truncados en el conteo de frecuencias");

                int frequencyCount = BitConverter.ToInt32(compressedData, position);
                position += 4;

                if (frequencyCount <= 0)
                    throw new ArgumentException("Tabla de frecuencias vacía o inválida");

                Dictionary<byte, int> frequencies = new Dictionary<byte, int>();

                // Leer la tabla de frecuencias
                for (int i = 0; i < frequencyCount; i++)
                {
                    if (position + 5 > compressedData.Length)
                        throw new ArgumentException("Datos comprimidos truncados en la tabla de frecuencias");

                    byte value = compressedData[position++];
                    int frequency = BitConverter.ToInt32(compressedData, position);
                    position += 4;

                    if (frequency > 0)
                        frequencies[value] = frequency;
                    else
                        Console.WriteLine($"Advertencia: Frecuencia inválida de {frequency} para el valor {value}");
                }

                if (frequencies.Count == 0)
                    throw new ArgumentException("No hay frecuencias válidas para construir el árbol de Huffman");

                // Reconstruir árbol
                root = BuildHuffmanTree(frequencies);
                if (root == null)
                    throw new InvalidOperationException("No se pudo reconstruir el árbol de Huffman");

                if (position >= compressedData.Length)
                    throw new ArgumentException("Datos comprimidos truncados");

                int padding = compressedData[position++];

                List<byte> decodedData = new List<byte>();
                HuffmanNode current = root;

                for (int i = position; i < compressedData.Length; i++)
                {
                    string bits = Convert.ToString(compressedData[i], 2).PadLeft(8, '0');

                    if (i == compressedData.Length - 1 && padding > 0)
                        bits = bits.Substring(0, 8 - padding);

                    foreach (char bit in bits)
                    {
                        current = bit == '0' ? current.Left : current.Right;

                        if (current.IsLeaf)
                        {
                            decodedData.Add(current.Value);
                            current = root;
                        }
                    }
                }

                return decodedData.ToArray();
            }


            private Dictionary<byte, int> CalculateFrequencies(byte[] data)
            {
                var frequencies = new Dictionary<byte, int>();
                foreach (byte b in data)
                {
                    if (!frequencies.ContainsKey(b))
                        frequencies[b] = 0;
                    frequencies[b]++;
                }
                return frequencies;
            }

            private HuffmanNode BuildHuffmanTree(Dictionary<byte, int> frequencies)
            {
                if (frequencies == null || frequencies.Count == 0)
                    throw new ArgumentException("No hay frecuencias para construir el árbol");

                var priorityQueue = new List<HuffmanNode>();

                foreach (var freq in frequencies)
                {
                    priorityQueue.Add(new HuffmanNode
                    {
                        Value = freq.Key,
                        Frequency = freq.Value
                    });
                }

                while (priorityQueue.Count > 1)
                {
                    priorityQueue.Sort((a, b) => a.Frequency.CompareTo(b.Frequency));

                    var left = priorityQueue[0];
                    var right = priorityQueue[1];
                    priorityQueue.RemoveRange(0, 2);

                    var parent = new HuffmanNode
                    {
                        Frequency = left.Frequency + right.Frequency,
                        Left = left,
                        Right = right
                    };

                    priorityQueue.Add(parent);
                }

                return priorityQueue.FirstOrDefault();
            }

            private void GenerateHuffmanCodes(HuffmanNode node, string code, Dictionary<byte, string> encodingMap)
            {
                if (node == null)
                    return;

                if (node.IsLeaf)
                {
                    encodingMap[node.Value] = code.Length > 0 ? code : "0";
                    return;
                }

                GenerateHuffmanCodes(node.Left, code + "0", encodingMap);
                GenerateHuffmanCodes(node.Right, code + "1", encodingMap);
            }
        }

        // Método para comprimir datos utilizando Huffman.
        public static byte[] CompressWithHuffman(byte[] data)
        {
            try
            {
                var huffman = new HuffmanCompression();
                return huffman.Compress(data);
            }
            catch (Exception ex)
            {
                throw new Exception("Error en la compresión Huffman: " + ex.Message);
            }
        }

        // Método para descomprimir datos utilizando Huffman.
        public static byte[] DecompressWithHuffman(byte[] compressedData)
        {
            try
            {
                var huffman = new HuffmanCompression();
                return huffman.Decompress(compressedData);
            }
            catch (Exception ex)
            {
                throw new Exception("Error en la descompresión Huffman: " + ex.Message);
            }
        }


        // Método para comprimir datos utilizando el algoritmo LZW con un diccionario de tamaño máximo de 12 bits (4096 entradas).
        public static byte[] CompressWithLZW(byte[] data)
        {
            int maxDictSize = 4096; // Tamaño máximo del diccionario (12 bits)
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            List<int> compressedData = new List<int>();

            // Inicializa el diccionario con los caracteres ASCII estándar
            for (int i = 0; i < 256; i++)
            {
                dictionary.Add(((char)i).ToString(), i);
            }

            string current = string.Empty;

            foreach (byte b in data)
            {
                string next = current + (char)b;

                // Si el diccionario contiene la secuencia actual, expandirla
                if (dictionary.ContainsKey(next))
                {
                    current = next;
                }
                else
                {
                    // Agrega el código de la secuencia actual a la salida comprimida
                    compressedData.Add(dictionary[current]);

                    // Si el diccionario no ha alcanzado el tamaño máximo, agrega la nueva secuencia
                    if (dictionary.Count < maxDictSize)
                    {
                        dictionary.Add(next, dictionary.Count);
                    }

                    current = ((char)b).ToString();
                }
            }

            // Agrega el código de la última secuencia
            if (!string.IsNullOrEmpty(current))
            {
                compressedData.Add(dictionary[current]);
            }

            // Convierte la lista de códigos comprimidos en un arreglo de bytes
            return compressedData.SelectMany(code => BitConverter.GetBytes((short)code)).ToArray();
        }

        // Método para descomprimir datos utilizando el algoritmo LZW con un diccionario de tamaño máximo de 12 bits (4096 entradas).
        public static byte[] DecompressWithLZW(byte[] compressedData)
        {
            int maxDictSize = 4096; // Tamaño máximo del diccionario (12 bits)
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            List<byte> decompressedData = new List<byte>();

            // Inicializa el diccionario con los caracteres ASCII estándar
            for (int i = 0; i < 256; i++)
            {
                dictionary.Add(i, ((char)i).ToString());
            }

            List<short> codes = new List<short>();
            for (int i = 0; i < compressedData.Length; i += 2)
            {
                codes.Add(BitConverter.ToInt16(compressedData, i));
            }

            string previous = dictionary[codes[0]];
            decompressedData.AddRange(previous.Select(c => (byte)c));

            for (int i = 1; i < codes.Count; i++)
            {
                string entry;
                int code = codes[i];

                if (dictionary.ContainsKey(code))
                {
                    entry = dictionary[code];
                }
                else if (code == dictionary.Count)
                {
                    entry = previous + previous[0];
                }
                else
                {
                    throw new Exception("Error en el código LZW: secuencia de códigos inválida.");
                }

                decompressedData.AddRange(entry.Select(c => (byte)c));

                // Si el diccionario no ha alcanzado el tamaño máximo, agregar la nueva secuencia
                if (dictionary.Count < maxDictSize)
                {
                    dictionary.Add(dictionary.Count, previous + entry[0]);
                }

                previous = entry;
            }

            return decompressedData.ToArray();
        }

    }
}
