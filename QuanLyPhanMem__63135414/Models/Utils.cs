using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace QuanLyPhanMem__63135414.Models.Extension
{
    public class Utils
    {
        public static string getUserId()
        {
            Guid guid = Guid.NewGuid();
            return guid.ToString();
        }
        public static string Hash(string value)
        {
            return Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(value)));
        }
    }
}