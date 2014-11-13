using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Lombiq.ArchivedLinks.ViewModel
{
    public class LinkViewModel
    {
        [Required]
        public string Url { get; set; }
    }
}