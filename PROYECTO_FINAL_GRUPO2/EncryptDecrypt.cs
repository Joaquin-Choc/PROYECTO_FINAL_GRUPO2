using System.Text;

namespace PROYECTO_FINAL_GRUPO2
{
    internal class EncryptDecrypt
    {
        // Método para cifrar datos.
        public static byte[] EncryptData(byte[] data, string password)
        {
            byte[] header = Encoding.UTF8.GetBytes("HEADER"); // Cabecera para validar desencriptación.
            byte[] dataWithHeader = new byte[header.Length + data.Length];
            Buffer.BlockCopy(header, 0, dataWithHeader, 0, header.Length);
            Buffer.BlockCopy(data, 0, dataWithHeader, header.Length, data.Length);

            byte[] encryptedData = XOREncrypt(dataWithHeader, password);
            encryptedData = Transpose(encryptedData);
            encryptedData = SubstituteBytes(encryptedData);

            return encryptedData;
        }

        // Método para cifrar datos utilizando el algoritmo XOR.
        public static byte[] XOREncrypt(byte[] data, string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password); // Se convierte la contraseña a bytes.
            byte[] encryptedData = new byte[data.Length]; // Se crea un arreglo para los datos encriptados.

            // Se recorre cada byte del dato original.
            for (int i = 0; i < data.Length; i++)
            {
                // Se aplica el cifrado XOR entre el dato y la contraseña.
                encryptedData[i] = (byte)(data[i] ^ passwordBytes[i % passwordBytes.Length]);
            }

            return encryptedData; // Se devuelve el dato encriptado.
        }

        // Método para invertir el orden de los bytes.
        public static byte[] Transpose(byte[] data)
        {
            Array.Reverse(data); // Se invierte el orden de los bytes.
            return data; // Se devuelve el dato transpuesto.
        }

        // Método para sustituir bytes utilizando una tabla de sustitución simple.
        public static byte[] SubstituteBytes(byte[] data)
        {
            byte[] substitutionBox = GenerateSBox(); // Se genera una tabla de sustitución (S-box).
            byte[] substitutedData = new byte[data.Length]; // Se crea un arreglo para los datos sustituidos.

            // Se recorre cada byte del dato original.
            for (int i = 0; i < data.Length; i++)
            {
                // Se sustituye el byte utilizando la S-box.
                substitutedData[i] = substitutionBox[data[i]];
            }

            return substitutedData; // Se devuelve el dato con sustitución.
        }

        // Método para generar una tabla de sustitución (S-box simple).
        private static byte[] GenerateSBox()
        {
            byte[] sbox = new byte[256]; // Se crea un arreglo para la S-box.
                                         // Se llena la S-box con valores generados.
            for (int i = 0; i < 256; i++)
            {
                sbox[i] = (byte)((i * 31) % 256); // Generar una S-box simple.
            }
            return sbox; // Se devuelve la S-box generada.
        }

        // Método para descifrar datos (revertir los pasos de encriptación).
        public static byte[] DecryptData(byte[] encryptedData, string password)
        {
            encryptedData = ReverseSubstituteBytes(encryptedData);
            encryptedData = Transpose(encryptedData);
            byte[] decryptedData = XOREncrypt(encryptedData, password);

            // Verificar si el inicio coincide con la cabecera.
            byte[] header = Encoding.UTF8.GetBytes("HEADER");
            for (int i = 0; i < header.Length; i++)
            {
                if (decryptedData[i] != header[i])
                {
                    throw new Exception("Contraseña incorrecta."); // Lanzar excepción si la cabecera no coincide.
                }
            }

            // Eliminar la cabecera antes de devolver los datos.
            byte[] originalData = new byte[decryptedData.Length - header.Length];
            Buffer.BlockCopy(decryptedData, header.Length, originalData, 0, originalData.Length);

            return originalData;
        }

        // Método para revertir la sustitución de bytes.
        public static byte[] ReverseSubstituteBytes(byte[] data)
        {
            byte[] reverseSBox = GenerateReverseSBox(); // Se genera la S-box inversa.
            byte[] reversedData = new byte[data.Length]; // Se crea un arreglo para los datos revertidos.

            // Se recorre cada byte del dato original.
            for (int i = 0; i < data.Length; i++)
            {
                // Se revierte la sustitución utilizando la S-box inversa.
                reversedData[i] = reverseSBox[data[i]];
            }

            return reversedData; // Se devuelve el dato revertido.
        }

        // Método para generar una S-box inversa para revertir la sustitución.
        private static byte[] GenerateReverseSBox()
        {
            byte[] reverseSBox = new byte[256]; // Se crea un arreglo para la S-box inversa.
            byte[] sbox = GenerateSBox(); // Se genera la S-box original.

            // Se llena la S-box inversa con valores generados.
            for (int i = 0; i < 256; i++)
            {
                reverseSBox[sbox[i]] = (byte)i; // Generar la S-box inversa.
            }
            return reverseSBox; // Se devuelve la S-box inversa generada.
        }


    }
}
