using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QuanLyPhanMem_63135414.Models
{
    [MetadataType(typeof(UserMetaData))]
    public partial class User
    {
        public string confirmPassword { get; set; }
        public Guid codeActive { get; set; }
    }
    public class UserMetaData
    {
        //email
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email ID required")]
        [DataType(DataType.EmailAddress)]
        public string email { get; set; }

        //password
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Minimum 6 characters required")]
        public string password { get; set; }


        //confirmPassword
        [DataType(DataType.Password)]
        [Compare("password", ErrorMessage = "Confirm password and password do not match")]
        public string confirmPassword { get; set; }

        //firstname
        [Required(AllowEmptyStrings = false, ErrorMessage = "First name required")]
        public string firstname { get; set; }

        //lastname
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last name required")]
        public string lastname { get; set; }

        //birthday
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime birthday { get; set; }
    }
}