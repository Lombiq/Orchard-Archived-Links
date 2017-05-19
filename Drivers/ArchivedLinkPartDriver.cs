using Lombiq.ArchivedLinks.Models;
using Lombiq.ArchivedLinks.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using System;
using Orchard.Exceptions;
using Lombiq.ArchivedLinks.Helpers;
using Orchard.ContentManagement.Handlers;
using Lombiq.ArchivedLinks.ViewModels;

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
            return Combined(
                ContentShape("Parts_ArchivedLink", () =>
                {
                    var uri = UriBuilderHelper.TryCreateUri(part.OriginalUrl);
                    return shapeHelper.Parts_ArchivedLink(
                        OriginalUrl: part.OriginalUrl,
                        SnapshotUrl: _snapshotManager.GetSnapshotIndexPublicUrl(uri).ToString()
                    );
                }),
                ContentShape("Parts_ArchivedLink_SummaryAdmin", () => shapeHelper.Parts_ArchivedLink_SummaryAdmin())
            );
        }

        protected override DriverResult Editor(ArchivedLinkPart part, dynamic shapeHelper)
        {
            if (part.IsNew() || string.IsNullOrEmpty(part.OriginalUrl))
            {
                return ContentShape("Parts_ArchivedLink_Edit",
                   () => shapeHelper.EditorTemplate(
                       TemplateName: "Parts.ArchivedLink",
                       Model: part,
                       Prefix: Prefix));
            }

            var uri = UriBuilderHelper.TryCreateUri(part.OriginalUrl);
            var snapshotUrl = _snapshotManager.GetSnapshotIndexPublicUrl(uri).ToString();

            var model = new ArchivedLinkViewModel()
            {
                OriginalUrl = part.OriginalUrl,
                SnapshotUrl = snapshotUrl
            };

            return Combined(

                ContentShape("Parts_ArchivedLink_Edit",
                  () => shapeHelper.EditorTemplate(
                      TemplateName: "Parts.ArchivedLink",
                      Model: part,
                      Prefix: Prefix)),
                ContentShape("Parts_ArchivedLink_Edit",
                  () => shapeHelper.EditorTemplate(
                      TemplateName: "Parts.ArchivedLink.Edit",
                      Model: model,
                      Prefix: Prefix)));

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

        protected override void Exporting(ArchivedLinkPart part, ExportContentContext context)
        {
            context.Element(part.PartDefinition.Name).SetAttributeValue("OriginalUrl", part.OriginalUrl);
        }

        protected override void Importing(ArchivedLinkPart part, ImportContentContext context)
        {
            context.ImportAttribute(part.PartDefinition.Name, "OriginalUrl", value => part.OriginalUrl = value);
        }
    }
}