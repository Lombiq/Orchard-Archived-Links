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
using Lombiq.ArchivedLinks.Helpers;

namespace Lombiq.ArchivedLinks.Drivers
{
    public class ArchivedLinkPartDriver : ContentPartDriver<ArchivedLinkPart>
    {
        private readonly ISnapshotManager _snapshotManager;

        public Localizer T { get; set; }


        public ArchivedLinkPartDriver(ISnapshotManager snapshotManager)
        {
            _snapshotManager = snapshotManager;

            T = NullLocalizer.Instance;
        }


        protected override DriverResult Display(ArchivedLinkPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_ArchivedLink", () =>
            {
                var uri = UriBuilderHelper.TryCreateUri(part.OriginalUrl);
                return shapeHelper.Parts_ArchivedLink(
                    OriginalUrl: part.OriginalUrl,
                    SnapshotUrl: _snapshotManager.GetSnapshotIndexPublicUrl(uri).ToString()
                );
            });
        }

        protected override DriverResult Editor(ArchivedLinkPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_ArchivedLink_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts.ArchivedLink",
                    Model: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(ArchivedLinkPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                try
                {
                    var uri = UriBuilderHelper.TryCreateUri(part.OriginalUrl);
                    _snapshotManager.SaveSnapshot(uri);
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