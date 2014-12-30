using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Utilities;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Lombiq.ArchivedLinks.Models
{
    public class LinkPart : ContentPart
    {
        public string OriginalUrl
        {
            get { return this.Retrieve(p => p.OriginalUrl); }
            set { this.Store(p => p.OriginalUrl, value); }
        }
    }
}