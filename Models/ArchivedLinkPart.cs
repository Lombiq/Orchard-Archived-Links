using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Utilities;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Lombiq.ArchivedLinks.Models
{
    public class ArchivedLinkPartRecord : ContentPartRecord
    {
        public virtual string OriginalUrl { get; set; }
    }

    public class ArchivedLinkPart : ContentPart<ArchivedLinkPartRecord>
    {
        public string OriginalUrl
        {
            get { return Retrieve(x => x.OriginalUrl); }
            set { Store(x => x.OriginalUrl, value); }
        }
    }
}