using Lombiq.ArchivedLinks.Services;
using Lombiq.ArchivedLinks.ViewModel;
using Orchard.FileSystems.Media;
using Orchard.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lombiq.ArchivedLinks.Controllers
{
    [Themed]
    public class LinkController : Controller
    {
        private readonly ILinkManager _linkManager;
        private readonly IStorageProvider _storageProvider;

        public LinkController(ILinkManager linkManager, IStorageProvider storageProvider)
        {
            _linkManager = linkManager;
            _storageProvider = storageProvider;
        }

        //
        // GET: /Link/
        public ActionResult Index()
        {
            var urls = _linkManager.GetLinks();
            return View(urls);
        }

        public ActionResult SaveLink()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SaveLink(LinkViewModel model)
        {
            if (ModelState.IsValid)
            {
                var indexHtml = _linkManager.SaveLink(model.Url, _storageProvider);
                return Redirect(indexHtml);
            }
            return View(model);
        }


	}
}