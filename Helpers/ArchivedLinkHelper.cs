using Lombiq.ArchivedLinks.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.ArchivedLinks.Helpers
{
    public class ArchivedLinkHelper : ContentHandler
    {
        public ArchivedLinkHelper(IRepository<ArchivedLinkPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}