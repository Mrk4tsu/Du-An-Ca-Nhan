using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QuanLyPhanMem__63135414.Models
{
    [MetadataType(typeof(UserMetaData))]
    public partial class User
    {
        public string confirmPassword { get; set; }
    }
    public class UserMetaData
    {
        //email
        [Required(AllowEmptyStrings = false, ErrorMessage = "Địa chỉ email vui lòng không để trống!")]
        [DataType(DataType.EmailAddress)]
        public string email { get; set; }

        //password
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Mật khẩu ít nhất phải có 6 kí tự!")]
        public string password { get; set; }


        //confirmPassword
        [DataType(DataType.Password)]
        [Compare("password", ErrorMessage = "Mật khẩu không khớp, vui lòng kiểm tra lại!")]
        public string confirmPassword { get; set; }

        //firstname
        [Required(AllowEmptyStrings = false, ErrorMessage = "Họ vui lòng không được để trống!")]
        public string firstname { get; set; }

        //lastname
        [Required(AllowEmptyStrings = false, ErrorMessage = "Tên vui lòng không được để trống!")]
        public string lastname { get; set; }

        //birthday
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime birthday { get; set; }
    }
}