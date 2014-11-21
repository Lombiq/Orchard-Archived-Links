using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.ArchivedLinks.Models
{
    public class LinkRecord
    {
        public string OriginalUrl { get; set; }
        
        public string FolterPath { get; set; }

        public DateTime? LastModified { get; set; }

        public DateTime? Created { get; set; }
    }
}