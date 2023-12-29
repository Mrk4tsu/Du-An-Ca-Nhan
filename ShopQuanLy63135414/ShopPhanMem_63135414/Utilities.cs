using ImageResizer;
using ShopPhanMem_63135414.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;

namespace ShopPhanMem_63135414
{
    public class Utilities
    {
        internal static readonly string AVATAR_DEFAULT = "avatardefault.png";
        internal static readonly string WALLPAPER_DEFAULT = "defaultwallpaper.png";
        internal static readonly string CATEGORY_DEFAULT = "defaultctg.png";
        internal static readonly string PRODUCT_IMAGE_DEFAULT = "productImgDefault.png";

        const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        //Singleton
        public static Utilities instance { get; } = new Utilities();
        public string randomCharacter(int length)
        {
            Random random = new Random();
            StringBuilder first = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                char randomChar = characters[random.Next(characters.Length)];

                //Thêm kí tự vào chuỗi kết quả
                first.Append(randomChar);
            }
            return first.ToString();
        }
        public bool isEmailExist(string email)
        {
            using (QLPM63135414Entities db = new QLPM63135414Entities())
            {
                var v = db.Users.Where(e => e.email == email).FirstOrDefault();
                return v != null;
            }
        }
        public string getIdAsync(int length)
        {
            using (QLPM63135414Entities db = new QLPM63135414Entities())
            {
                var products = db.Products.ToList();
                if (products.Any())
                {
                    var max = products.Select(p => p.id).Max();
                    int productId = int.Parse(max.Substring(length)) + 1;

                    string product = string.Concat("000", productId.ToString());
                    return randomCharacter(5) + product.Substring(productId.ToString().Length - 1);
                }
                else
                {
                    // If the database is empty, generate the first ID
                    return randomCharacter(5) + "00001";
                }
            }
        }
        public string getIDImage(int lenght)
        {
            using (QLPM63135414Entities db = new QLPM63135414Entities())
            {
                Random random = new Random();
                StringBuilder first = new StringBuilder(lenght);//Tối đa 5 kí tự vì trong database set mã tối đa 10 kí tự
                for (int i = 0; i < lenght; i++)
                {
                    char randomChar = characters[random.Next(characters.Length)];

                    //Thêm kí tự vào chuỗi kết quả
                    first.Append(randomChar);
                }

                var products = db.ProductImages.ToList();
                if (products.Any())
                {
                    var max = products.Select(p => p.id).Max();
                    int productId = int.Parse(max.Substring(lenght)) + 1;

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
            using (QLPM63135414Entities db = new QLPM63135414Entities())
            {
                var maMax = db.Categories.ToList().Select(n => n.id).Max();
                //CATE001
                int categoryID = int.Parse(maMax.Substring(4)) + 1;
                string cate = String.Concat("00", categoryID.ToString());
                return "CATE" + cate.Substring(categoryID.ToString().Length - 1);
            }
        }
        public string getIdCart()
        {
            using (QLPM63135414Entities db = new QLPM63135414Entities())
            {
                var carts = db.Carts.ToList();
                if (carts.Any())
                {
                    var maMax = db.Carts.ToList().Select(n => n.Id).Max();
                    int cardId = int.Parse(maMax.Substring(2)) + 1;
                    string cart = string.Concat("00", cardId.ToString());

                    return "CR" + cart.Substring(cardId.ToString().Length - 1);
                }
                else
                    return "CR001";
            }
        }
        public string getIdOrder()
        {
            using (QLPM63135414Entities db = new QLPM63135414Entities())
            {
                var latestOrder = db.Orders.OrderByDescending(o => o.OrderId).FirstOrDefault();
                if (latestOrder != null)
                {
                    int orderId = int.Parse(latestOrder.OrderId.Substring(2)) + 1;
                    return $"OD{orderId:D3}";
                }
                else
                {
                    return "OD001";
                }
            }
        }
        private static int orderIdCounter = 1;
       
        public string getIdCartItem()
        {
            using (QLPM63135414Entities db = new QLPM63135414Entities())
            {
                var carts = db.CartItems.ToList();

                if (carts.Any())
                {
                    var maMax = db.CartItems.ToList().Select(n => n.Id).Max();
                    int cardItemId = int.Parse(maMax.Substring(5)) + 1;
                    string item = string.Concat("00", cardItemId.ToString());

                    return randomCharacter(5) + item.Substring(cardItemId.ToString().Length - 1);
                }
                else
                    return randomCharacter(5) + "001";
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