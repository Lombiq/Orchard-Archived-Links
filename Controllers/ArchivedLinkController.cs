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
            Uri uri;
            if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out uri))
            {
                uri = new Uri(String.Format("http://{0}", originalUrl), UriKind.Absolute);
            }

            if (_snapshotManager.CheckUriIsAvailable(uri))
            {
                return Redirect(uri.ToString());
            }
            else
            {
                return Redirect(_snapshotManager.GetSnapshotIndexPublicUrl(uri));
            }
        }
    }
}