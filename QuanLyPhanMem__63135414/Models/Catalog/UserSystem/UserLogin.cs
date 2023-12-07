using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QuanLyPhanMem__63135414.Models.Extension
{
    public class UserLogin
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Địa chỉ email không được để trống!")]
        public string email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Vui lòng nhập mật khẩu!")]
        [DataType(DataType.Password)]
        public string password { get; set; }

        public bool rememberMe { get; set; }

    }
}