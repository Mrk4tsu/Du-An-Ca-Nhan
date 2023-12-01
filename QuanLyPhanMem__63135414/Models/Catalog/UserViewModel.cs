using System;

namespace QuanLyPhanMem__63135414.Models.Extension
{
    public class UserViewModel
    {
        public string userId { get; set; }
        public string roleId { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string userAvatar { get; set; }
        public string userWallpaper { get; set; }
        public Nullable<System.DateTime> birthday { get; set; }
        public string address { get; set; }
        public string phoneNumber { get; set; }
        public int quantityProject { get; set; }
        public string bio { get; set; }
        public string codeActive { get; set; }
        public Nullable<bool> isActive { get; set; }
        public virtual UserRole UserRole { get; set; }
    }
}