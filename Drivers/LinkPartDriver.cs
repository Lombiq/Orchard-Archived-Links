using Lombiq.ArchivedLinks.Models;
using Lombiq.ArchivedLinks.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Exceptions;
using Orchard.Environment.Extensions;

namespace Lombiq.ArchivedLinks.Drivers
{
    public class LinkPartDriver : ContentPartDriver<LinkPart>
    {
        private readonly ISnapshotManager _snapshotManager;

        public Localizer T { get; set; }


        public LinkPartDriver(ISnapshotManager snapshotManager)
        {
            _snapshotManager = snapshotManager;

            T = NullLocalizer.Instance;
        }


        protected override DriverResult Display(LinkPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_Link", () =>
            {
                Uri uri;
                if (!Uri.TryCreate(part.OriginalUrl, UriKind.Absolute, out uri))
                {
                    uri = new Uri(String.Format("http://{0}", part.OriginalUrl), UriKind.Absolute);
                }

                return shapeHelper.Parts_Link(
                    OriginalUrl: uri.ToString(),
                    SnapshotUrl: _snapshotManager.GetSnapshotIndexPublicUrl(uri)
                );
            });
        }

        protected override DriverResult Editor(LinkPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_Link_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts.Link",
                    Model: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(LinkPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                try
                {
                    Uri uri;
                    if (!Uri.TryCreate(part.OriginalUrl, UriKind.Absolute, out uri)) {
                        uri = new Uri(String.Format("http://{0}", part.OriginalUrl), UriKind.Absolute);
                    }
                    _snapshotManager.SaveLink(uri);
                }
                catch (UriFormatException)
                {
                    updater.AddModelError("UriFormatException", T("There was a problem with the given url: {0}", part.OriginalUrl));
                }
                catch (Exception ex)
                {
                    if (ex.IsFatal()) throw;

                    updater.AddModelError("Exception", T("Unknown problem while saving your url."));
                }
            }
            return Editor(part, shapeHelper);
        }
    }
}