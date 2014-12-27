using Lombiq.ArchivedLinks.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lombiq.ArchivedLinks.Controllers
{
    public class ArchivedLinkController : Controller
    {
        private readonly ISnapshotManager _snapshotManager;


        public ArchivedLinkController(ISnapshotManager snapshotManager)
        {
            _snapshotManager = snapshotManager;
        }


        public ActionResult Index(string originalUrl)
        {
            var uriBuilder = new UriBuilder(originalUrl);

            if (_snapshotManager.CheckUriIsAvailable(uriBuilder.Uri))
            {
                return Redirect(uriBuilder.Uri.ToString());
            }
            else
            {
                return Redirect(_snapshotManager.GetSnapshotIndexPublicUrl(uriBuilder.Uri));
            }
        }
    }
}