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

        //firstname
        [Required(AllowEmptyStrings = false, ErrorMessage = "Họ vui lòng không được để trống!")]
        public string firstname { get; set; }

        //lastname
        [Required(AllowEmptyStrings = false, ErrorMessage = "Tên vui lòng không được để trống!")]
        public string lastname { get; set; }

        //birthday
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime birthday { get; set; }
    }
}