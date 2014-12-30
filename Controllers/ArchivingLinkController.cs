using Lombiq.ArchivedLinks.Models;
using Lombiq.ArchivedLinks.Services;
using Lombiq.ArchivedLinks.ViewModels;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lombiq.ArchivedLinks.Controllers
{
    [Themed]
    public class ArchivingLinkController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly ISnapshotManager _snapshotManager;


        public ArchivingLinkController(IContentManager contentManager, ISnapshotManager snapshotManager)
        {
            _contentManager = contentManager;
            _snapshotManager = snapshotManager;
        }


        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                uri = new Uri(String.Format("http://{0}", url, UriKind.Absolute));
            }

            _snapshotManager.SaveLink(uri);

            var linkContentItem = _contentManager.New("Link");
            linkContentItem.As<LinkPart>().OriginalUrl = url;
            _contentManager.Create(linkContentItem);

            return RedirectToAction("Snapshot", new { uri = uri });
        }

        public ActionResult Snapshot(Uri uri) 
        {
            return View(new SnapshotViewModel
            {
                SnapshotUrl = _snapshotManager.GetSnapshotIndexPublicUrl(uri),
                OriginalUrl = uri.ToString()
            });
        }
	}
}