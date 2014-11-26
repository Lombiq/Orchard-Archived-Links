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
            return ContentShape("Parts_Link", ()=>
            {
                string url;
                if (!String.IsNullOrEmpty(_linkManager.CheckJumpUrl("ArchivedLink-Jump-" + part.Id.ToString())))
                {
                    url = part.OriginalUrl;
                }
                else {
                    url = _linkManager.GetPublicUrl(part.Id);
                }

                return shapeHelper.Parts_Link(Url: url);
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