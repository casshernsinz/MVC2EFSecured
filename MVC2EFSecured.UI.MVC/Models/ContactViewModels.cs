using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;//Added for validation&formatting

namespace MVC2EFSecured.UI.MVC.Models
{
    public class ContactViewModels
    {
        [Required(ErrorMessage = "* Name is Required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "* Email is Required")]
        //If you need to use a regular expression to match a pattern, you can use the syntax below
        //[RegularExpression("ExpressionPatternHere",ErrorMessage = "Message Here")]
        [DataType(DataType.EmailAddress)]//uses a built-in regex pattern to validate user input
        public string Email { get; set; }

        [Required(ErrorMessage = "* Subject is Required")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "* Message is Required")]
        [UIHint("MultilineText")]//Provides a text area (multiline text box) for this property
        public string Message { get; set; }
    }
}