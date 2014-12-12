using Lombiq.ArchivedLinks.Models;
using Lombiq.ArchivedLinks.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
                return shapeHelper.Parts_Link(
                    OriginalUrl: part.OriginalUrl,
                    SnapshotUrl: _snapshotManager.GetSnapshotIndexPubliUrl(part.OriginalUrl)
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
                    _snapshotManager.SaveLink(part.OriginalUrl);
                }
                catch (UriFormatException)
                {
                    updater.AddModelError("UriFormatException", T("There was a problem with the given url: {0}", part.OriginalUrl));
                }
                catch (Exception ex)
                {
                    updater.AddModelError("Exception", T("There was a problem while saving your url: {0}", ex.Message));
                }
            }
            return Editor(part, shapeHelper);
        }
    }
}