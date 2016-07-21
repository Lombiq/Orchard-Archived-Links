using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Lombiq.ArchivedLinks.Models
{
    public class ArchivedLinkPart : ContentPart<ArchivedLinkPartRecord>
    {
        public string OriginalUrl
        {
            get { return Retrieve(x => x.OriginalUrl); }
            set { Store(x => x.OriginalUrl, value); }
        }
    }


    public class ArchivedLinkPartRecord : ContentPartRecord
    {
        public virtual string OriginalUrl { get; set; }
    }
}