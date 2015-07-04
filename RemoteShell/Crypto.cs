using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace RemoteShell
{
    class Crypto
    {
        public ICryptoTransform ict;
        private byte[] IV;

           private void initialize(string password,byte[] salt, byte[] iv)
           {
               AesCryptoServiceProvider csp=new AesCryptoServiceProvider();
               csp.Mode = CipherMode.CBC;
               csp.Padding = PaddingMode.PKCS7;
               var spec = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(password), salt, 65536);
               byte[] key = spec.GetBytes(16);
               csp.Key = key;

               if (iv == null)      //enc
               {
                   csp.GenerateIV();
                   this.IV = csp.IV;
                   ict = csp.CreateEncryptor();
                  
               }
               else
               {
                   csp.IV = iv;
                   //ict = csp.CreateDecryptor();
                   ict = csp.CreateEncryptor();
                   this.IV = iv;
               }
           }

            public Crypto(string password,byte[] salt, byte[] iv)
            {
                initialize(password, salt,iv);
            }

           public Crypto(string password, byte[] salt)
           {
               initialize(password, salt,null);
           }

           public byte[] decrypt(byte[] ciphertext)
           {
               byte[] output = ict.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
               return output;
           }

           public byte[] encrypt(byte[] plaintext)
           {
               byte[] output = ict.TransformFinalBlock(plaintext, 0, plaintext.Length);
               return output;
           }

           public string decrypt(string ciphertext)
           {
               byte[] ct=Convert.FromBase64String(ciphertext);
               byte[] pt = decrypt(ct);
               return Encoding.UTF8.GetString(pt);
           }

           public string encrypt(string plaintext)
           {
               byte[] pt = Encoding.UTF8.GetBytes(plaintext);
               byte[] ct = encrypt(pt);
               return Convert.ToBase64String(ct);
           }

           public byte[] getIv()
           {
               return IV;
           }
    }
}
