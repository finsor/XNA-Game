using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.IO;

namespace OrFins
{
    class AES_Encryption
    {
        #region Data
        private byte[] key = { 123, 217, 19, 11, 24, 26, 85, 45, 114, 184, 27, 162, 37, 112, 222, 209, 241, 24, 175, 144, 173, 53, 196, 29, 24, 26, 17, 218, 131, 236, 53, 209 };
        private byte[] vector = { 146, 64, 191, 111, 23, 3, 113, 119, 231, 121, 221, 112, 79, 32, 114, 156 };
        private ICryptoTransform encryptor, decryptor;
        private UTF8Encoding encoder;
        #endregion

        #region Construction
        public AES_Encryption()
        {
            RijndaelManaged rm = new RijndaelManaged();
            encryptor = rm.CreateEncryptor(key, vector);
            decryptor = rm.CreateDecryptor(key, vector);
            encoder = new UTF8Encoding();
        }
        #endregion

        #region Public functions
        public string Read(StreamReader reader)
        {
            return Decrypt(reader.ReadLine());
        }
        public void Write(StreamWriter writer, object obj)
        {
            writer.WriteLine(Encrypt(obj.ToString()));
        }
        #endregion

        #region Private functions
        private string Encrypt(string unencrypted)
        {
            // 1. Convert the unencrypted string into a bytes array.
            // 2. Encrypt the array using AES.
            // 3. Convert the encryption to base64.
            return Convert.ToBase64String(Encrypt(encoder.GetBytes(unencrypted)));
        }
        private string Decrypt(string encrypted)
        {
            // Reversing Encrypt(string) function
            if (encrypted == null)
                return null;

            return encoder.GetString(Decrypt(Convert.FromBase64String(encrypted)));
        }
        private byte[] Encrypt(byte[] buffer)
        {
            // Encrypt the bytes buffer
            return Transform(buffer, encryptor);
        }
        private byte[] Decrypt(byte[] buffer)
        {
            // Decrypt the bytes buffer
            return Transform(buffer, decryptor);
        }
        private byte[] Transform(byte[] buffer, ICryptoTransform transform)
        {
            MemoryStream stream = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(stream, transform, CryptoStreamMode.Write))
            {
                cs.Write(buffer, 0, buffer.Length);
            }
            return stream.ToArray();
        }
        #endregion
    }
}
