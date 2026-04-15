using System;
using System.Security.Cryptography;
using UnityEngine;

namespace YesChef.Core
{
    [CreateAssetMenu(menuName = "YesChef/Encryption Config", fileName = "EncryptionConfig")]
    public class EncryptionConfig : ScriptableObject
    {
        [Header("AES-256 Key (32 bytes) as Base64")]
        public string keyBase64;
        
        [Header("AES IV (16 bytes) as Base64")]
        public string ivBase64;

        public byte[] KeyBytes => Convert.FromBase64String(keyBase64);
        public byte[] IVBytes  => Convert.FromBase64String(ivBase64);

        /// <summary>
        /// Generates a random AES-256 key and IV, then stores them as Base64 strings.
        /// </summary>
        public void GenerateNewKeys()
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                aes.GenerateIV();

                keyBase64 = Convert.ToBase64String(aes.Key);
                ivBase64  = Convert.ToBase64String(aes.IV);
            }

            // Mark the ScriptableObject as dirty so Unity saves the changes
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
    }
}