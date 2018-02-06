using System;
using System.Security.Cryptography;
using System.Text;

namespace Monifier.BusinessLogic.Auth
{
    public static class HashHelper
    {
        public static string ComputeHash(string value, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
                return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
            }
        }
        
        public static string CreateSalt()  
        {  
            var bytes = new byte[16];  
            using(var keyGenerator = RandomNumberGenerator.Create())  
            {  
                keyGenerator.GetBytes(bytes);  
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();  
            }  
        }
    }
}