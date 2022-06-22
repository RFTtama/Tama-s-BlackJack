using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Tama_s_BlackJack
{
    class Encryption
    {
        public String fileName;                             //ファイル名 別途指定が必要
        private const String IV = @"7W7crKL0GS5tnb76";      //開始場所 ※変更するな
        private const String key = @"6BE57GwW05K1u393";     //キー ※変更するな     

        public Encryption()
        {
            fileName = null;
        }

        /// <summary>
        /// 指定した文字列を指定したファイルに暗号化して出力する
        /// </summary>
        /// <param name="encryptString">平文</param>
        public void Encrypt(String encryptString)
        {
            using (RijndaelManaged rijndael = new RijndaelManaged())
            {
                rijndael.BlockSize = 128;
                rijndael.KeySize = 128;
                rijndael.Mode = CipherMode.CBC;
                rijndael.Padding = PaddingMode.PKCS7;

                rijndael.IV = Encoding.UTF8.GetBytes(IV);
                rijndael.Key = Encoding.UTF8.GetBytes(key);

                ICryptoTransform encryptor = rijndael.CreateEncryptor(rijndael.Key, rijndael.IV);
                using (FileStream fStream = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    using (CryptoStream ctStream = new CryptoStream(fStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(ctStream))
                        {
                            sw.Write(encryptString);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 指定したファイルにある暗号化データを復号する
        /// </summary>
        /// <returns>文字列(List): 行に応じて分けられる</returns>
        public List<String> Decrypt()
        {
            using (RijndaelManaged rijndael = new RijndaelManaged())
            {
                rijndael.BlockSize = 128;
                rijndael.KeySize = 128;
                rijndael.Mode = CipherMode.CBC;
                rijndael.Padding = PaddingMode.PKCS7;

                rijndael.IV = Encoding.UTF8.GetBytes(IV);
                rijndael.Key = Encoding.UTF8.GetBytes(key);

                ICryptoTransform decryptor = rijndael.CreateDecryptor(rijndael.Key, rijndael.IV);

                List<String> plain = new List<String>();
                using (FileStream fStream = new FileStream(fileName, FileMode.Open))
                {
                    using (CryptoStream ctStream = new CryptoStream(fStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(ctStream))
                        {
                            while (true)
                            {
                                String str = String.Empty;
                                str = sr.ReadLine();
                                if (str == null) break;
                                plain.Add(str);
                            }
                        }
                    }
                }
                return plain;
            }
        }
    }
}
