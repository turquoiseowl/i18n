using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using i18n.DataAnnotations;//use i18n.DataAnnotations instead of System.ComponentModel.DataAnnotations

namespace i18n.Demo.MVC4.Models
{
    public class AttributeTestViewModel
    {
        [Required(ErrorMessage="{0} has to be there")]
        [Display(Name = "First Name", Description="Test1", Prompt="Test")]//make sure all attribute properties are defined on one line, otherwise they will not be picked up by postbuild
        [StringLength(20, MinimumLength=5, ErrorMessage="{0} is too long at {2} or too short at {1}")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage="{0} is not valid.")]
        public string Email { get; set; }
    }
}