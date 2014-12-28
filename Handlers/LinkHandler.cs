using Lombiq.ArchivedLinks.Models;
using Lombiq.ArchivedLinks.Services;
using Orchard.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.ArchivedLinks.Handlers
{
    public class LinkHandler : ContentHandler
    {
        private readonly ISnapshotManager _snapshotManager;


        public LinkHandler(ISnapshotManager snapshotManager)
        {
            _snapshotManager = snapshotManager;

            OnRemoving<LinkPart>((context, part) =>
            {
                Uri uri;
                if (!Uri.TryCreate(part.OriginalUrl, UriKind.Absolute, out uri))
                {
                    uri = new Uri(String.Format("http://{0}", part.OriginalUrl), UriKind.Absolute);
                }
                snapshotManager.RemoveLink(uri);
            });
        }
    }
}