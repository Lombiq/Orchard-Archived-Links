using Lombiq.ArchivedLinks.Models;
using Lombiq.ArchivedLinks.Services;
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
    [OrchardFeature("Lombiq.ArchivedLinks.Frontend")]
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
            var uri = new UriBuilder(url).Uri;

            _snapshotManager.SaveLink(uri);

            var contentItem = _contentManager.New("Link");
            _contentManager.Create(contentItem);

            return RedirectToAction("Snapshot", new { snapshotUrl = (_snapshotManager.GetSnapshotIndexPublicUrl(uri)) });
        }

        public ActionResult Snapshot(string snapshotUrl) 
        {
            return View((object)snapshotUrl);
        }
	}
}