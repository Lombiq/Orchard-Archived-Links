using Lombiq.ArchivedLinks.Helpers;
using Lombiq.ArchivedLinks.Models;
using Lombiq.ArchivedLinks.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.ArchivedLinks.Handlers
{
    public class ArchivedLinkHandler : ContentHandler
    {
        public ArchivedLinkHandler(Work<ISnapshotManager> snapshotManagerWork)
        {
            OnRemoving<ArchivedLinkPart>((context, part) =>
            {
                Uri uri = UriBuilderHelper.TryCreateUri(part.OriginalUrl);
                snapshotManagerWork.Value.RemoveSnapshot(uri);
            });
        }
    }
}