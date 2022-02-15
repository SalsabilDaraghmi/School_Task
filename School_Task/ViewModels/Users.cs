using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace School_Task.ViewModels
{
    public class Users
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Lastname { get; set; }
        public String Username { get; set; }
        
        [Required(ErrorMessage = "Please enter your email address")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        [MaxLength(50)]
       
        public string Email { get; set; }
        
        
        public int? age { get; set; }
        public String Role { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public String Password { get; set; }




    }
}