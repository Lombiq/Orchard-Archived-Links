using Lombiq.ArchivedLinks.Models;
using Lombiq.ArchivedLinks.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.ArchivedLinks.Drivers
{
    public class LinkPartDriver : ContentPartDriver<LinkPart>
    {
        private readonly ILinkManager _linkManager;


        public LinkPartDriver(ILinkManager linkManager)
        {
            _linkManager = linkManager;
        }


        protected override DriverResult Display(LinkPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_Link", () =>
            {
                return shapeHelper.Parts_Link(
                    OriginalUrl: part.OriginalUrl,
                    JumpUrl: "/archived-link?originalUrl=" + part.OriginalUrl,
                    SnapshotUrl: _linkManager.GetPublicUrl(part.Id)
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
                _linkManager.SaveLink(part);
            }
            return Editor(part, updater, shapeHelper);
        }
    }
}