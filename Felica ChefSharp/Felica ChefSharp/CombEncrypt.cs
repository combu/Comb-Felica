using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Felica_ChefSharp
{
    static public class CombEncrypt
    {
        // 128bit(16byte)のIV（初期ベクタ）とKey（暗号キー）
        private const string AesKey = @"yasBMRBSjDF7b3sV";

        /// <summary>
        /// 文字列をAESで暗号化
        /// </summary>
        static public byte[] Encrypt(string text)
        {
            // AES暗号化サービスプロバイダ
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 128;
            aes.GenerateIV();
            aes.Key = Encoding.UTF8.GetBytes(AesKey);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // 文字列をバイト型配列に変換
            byte[] src = Encoding.Unicode.GetBytes(text);

            // 暗号化する
            using (ICryptoTransform encrypt = aes.CreateEncryptor())
            {
                byte[] dest = encrypt.TransformFinalBlock(src, 0, src.Length);


                byte[] returnArray = new byte[dest.Length + aes.IV.Length];

                Array.Copy(aes.IV, returnArray, aes.IV.Length);
                Array.Copy(dest, 0, returnArray, aes.IV.Length, dest.Length);

                return returnArray;
            }
        }

        /// <summary>
        /// 文字列をAESで復号化
        /// </summary>
        static public string Decrypt(byte[] src)
        {
            // AES暗号化サービスプロバイダ
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 128;

            byte[] useIv = new byte[16];
            Array.Copy(src, 0, useIv, 0, 16);

            aes.IV = useIv;
            aes.Key = Encoding.UTF8.GetBytes(AesKey);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // 複号化する
            using (ICryptoTransform decrypt = aes.CreateDecryptor())
            {
                byte[] decSrc = new byte[src.Length - 16];
                Array.Copy(src, 16, decSrc, 0, src.Length-16);

                byte[] dest = decrypt.TransformFinalBlock(decSrc, 0, decSrc.Length);
                return Encoding.Unicode.GetString(dest);
            }
        }
    }
}
