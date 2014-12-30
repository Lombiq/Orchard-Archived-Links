using Lombiq.ArchivedLinks.Models;
using Lombiq.ArchivedLinks.Services;
using Lombiq.ArchivedLinks.ViewModels;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Lombiq.ArchivedLinks.Controllers
{
    [Themed]
    public class ArchivedLinkController : Controller
    {
        private readonly ISnapshotManager _snapshotManager;
        private readonly IContentManager _contentManager;


        public ArchivedLinkController(ISnapshotManager snapshotManager, IContentManager contentManager)
        {
            _snapshotManager = snapshotManager;
            _contentManager = contentManager;
        }


        public async Task<ActionResult> Index(string originalUrl)
        {
            Uri uri;
            if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out uri))
            {
                uri = new Uri(String.Format("http://{0}", originalUrl), UriKind.Absolute);
            }

            if (await _snapshotManager.CheckUriIsAvailable(uri))
            {
                return Redirect(uri.ToString());
            }
            else
            {
                var linkContentItem = _contentManager.Query(VersionOptions.Latest).ForPart<LinkPart>().List()
                    .Where(link => link.OriginalUrl == originalUrl).FirstOrDefault();

                if (linkContentItem != null)
                {
                    var linkCommonPart = _contentManager.Get<CommonPart>(linkContentItem.Id, VersionOptions.Latest);
                    var snapshotTaken = linkCommonPart.ModifiedUtc == null ? linkCommonPart.CreatedUtc : linkCommonPart.ModifiedUtc;

                    return View(new SnapshotIframeViewModel
                    {
                        SnapshotIndexPublicUrl = _snapshotManager.GetSnapshotIndexPublicUrl(uri),
                        SnapshotTaken = (DateTime)snapshotTaken
                    });
                }

                throw new Exception();
            }
        }
    }
}