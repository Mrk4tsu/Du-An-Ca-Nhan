using ImageResizer;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace QuanLyPhanMem__63135414.Models.Extension
{
    public class Utilities : Controller
    {
        public static readonly string AVATAR_DEFAULT = "avatardefault.png";
        public static readonly string WALLPAPER_DEFAULT = "defaultwallpaper.png";
        internal static readonly string CATEGORY_DEFAULT = "defaultctg.png";

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