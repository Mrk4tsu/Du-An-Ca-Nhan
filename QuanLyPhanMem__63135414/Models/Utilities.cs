using ImageResizer;
using System;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace QuanLyPhanMem__63135414.Models.Extension
{
    public class Utilities : Controller
    {
        internal static readonly string AVATAR_DEFAULT = "avatardefault.png";
        internal static readonly string WALLPAPER_DEFAULT = "defaultwallpaper.png";
        internal static readonly string CATEGORY_DEFAULT = "defaultctg.png";

        const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        //Singleton
        public static Utilities instance { get; } = new Utilities();
        public bool isEmailExist(string email)
        {
            using (QLPM_63135414_Entities db = new QLPM_63135414_Entities())
            {
                var v = db.Users.Where(e => e.email == email).FirstOrDefault();
                return v != null;
            }
        }
        public string getIdAsync(int length)
        {
            using (QLPM_63135414_Entities db = new QLPM_63135414_Entities())
            {
                Random random = new Random();
                StringBuilder first = new StringBuilder(length);//Tối đa 5 kí tự vì trong database set mã tối đa 10 kí tự
                for (int i = 0; i < length; i++)
                {
                    char randomChar = characters[random.Next(characters.Length)];

                    //Thêm kí tự vào chuỗi kết quả
                    first.Append(randomChar);
                }

                var products = db.Products.ToList();
                if (products.Any())
                {
                    var max = products.Select(p => p.id).Max();
                    int productId = int.Parse(max.Substring(length)) + 1;

                    string product = String.Concat("000", productId.ToString());
                    return first.ToString() + product.Substring(productId.ToString().Length - 1);
                }
                else
                {
                    // If the database is empty, generate the first ID
                    return first.ToString() + "00001";
                }
            }
        }
        public string getNewGuid()
        {
            Guid guid = Guid.NewGuid();
            return guid.ToString();
        }
        public string getIdCategory()
        {
            using (QLPM_63135414_Entities db = new QLPM_63135414_Entities())
            {
                var maMax = db.Categories.ToList().Select(n => n.id).Max();
                //CATE001
                int maNV = int.Parse(maMax.Substring(4)) + 1;
                string NV = String.Concat("00", maNV.ToString());
                return "CATE" + NV.Substring(maNV.ToString().Length - 1);
            }
        }
        public static string Hash(string value)
        {
            return Convert.ToBase64String(System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(value)));
        }
        public void cropAndResizeImage(string path)
        {
            var settings = new ResizeSettings
            {
                Width = 300,
                Height = 300,
                Mode = FitMode.Crop,
                Scale = ScaleMode.Both,
                Anchor = ContentAlignment.MiddleCenter,
            };

            ImageBuilder.Current.Build(path, path, settings);
        }

    }
}