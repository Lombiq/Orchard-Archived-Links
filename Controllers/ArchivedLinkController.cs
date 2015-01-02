using Lombiq.ArchivedLinks.Helpers;
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
            try
            {
                Uri uri = UriBuilderHelper.TryCreateUri(originalUrl);

                if (await _snapshotManager.CheckUriIsAvailable(uri))
                {
                    return Redirect(uri.ToString());
                }
                else
                {
                    var archivedLinkPart = _contentManager.Query().Where<ArchivedLinkPartRecord>(link => link.OriginalUrl == originalUrl).List().FirstOrDefault();
                    if (archivedLinkPart != null)
                    {
                        var commonPart = archivedLinkPart.As<CommonPart>();
                        if (commonPart != null)
                        {
                            var snapshotTaken = commonPart.ModifiedUtc == null ? commonPart.CreatedUtc : commonPart.ModifiedUtc;
                            return View(new SnapshotViewModel
                            {
                                SnapshotIndexPublicUrl = _snapshotManager.GetSnapshotIndexPublicUrl(uri),
                                SnapshotTaken = (DateTime)snapshotTaken
                            });
                        }
                    }

                    throw new Exception();
                }
            }
            catch (UriFormatException)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "Bad Request");
            }
        }
    }
}