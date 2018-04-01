using System.Security.Cryptography;
using System.Text;

namespace Irixi_Aligner_Common.Classes.BaseClass
{
    public class HashGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string GetHashSHA256(string Text)
        { 
            var crypt = new SHA1CryptoServiceProvider();
            var data = Encoding.UTF8.GetBytes(Text);
            var hash = crypt.ComputeHash(data);
            return Encoding.UTF8.GetString(hash);
            
        }
    }
}
