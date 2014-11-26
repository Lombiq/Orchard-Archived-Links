using HtmlAgilityPack;
using Lombiq.ArchivedLinks.Models;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.FileSystems.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace Lombiq.ArchivedLinks.Services
{
    public class LinkPartManager : ILinkManager
    {
        private readonly IStorageProvider _storageProvider;
        private readonly IContentManager _contentManager;
        private WebClient _client;

        public LinkPartManager(IStorageProvider storageProvider, IContentManager contentManager)
        {
            _storageProvider = storageProvider;
            _contentManager = contentManager;
        }

        public string SaveLink(LinkPart linkPart)
        {
            Archiving(linkPart);
            return "ArchivedLink-Jump-"+linkPart.Id.ToString();
        }

        public string GetPublicUrl(int id)
        {
            return _storageProvider.GetPublicUrl(_storageProvider.Combine(_storageProvider.Combine("_ArchivedLinks", id.ToString()), "index.html"));
        }

        public string CheckJumpUrl(string jumpUrl)
        {
            // _ArchivedLinks/ID/index.html
            // /ArchivedLink-Jump-id

            var jumpUrlPattern = new Regex(@"^ArchivedLink-Jump-\d+$", RegexOptions.IgnoreCase);
            if (jumpUrlPattern.Match(jumpUrl).Success) 
            {
                var pieces = jumpUrl.Split(new[] { '-' });
                var id = int.Parse(pieces[2]);

                var contenPart = _contentManager.Get(id).As<LinkPart>();

                if (contenPart == null)
                    return String.Empty;

                HttpWebResponse response = null;
                var originalUrl = contenPart.OriginalUrl;
                var request = (HttpWebRequest)WebRequest.Create(originalUrl);
                request.Method = "HEAD";


                try
                {
                    // if no exception (status is '200 OK' return original url)
                    response = (HttpWebResponse)request.GetResponse();
                    return originalUrl;
                }
                catch (WebException)
                {
                    // if status is not '200 OK'
                    return _storageProvider.GetPublicUrl(_storageProvider.Combine(_storageProvider.Combine("_ArchivedLinks", id.ToString()), "index.html"));
                }
            }

            return String.Empty;
        }




        private void Archiving(LinkPart linkPart)
        {
            linkPart.FolderPath = _storageProvider.Combine("_ArchivedLinks", linkPart.Id.ToString());

            var currentUrl = linkPart.OriginalUrl;
            if (String.IsNullOrEmpty(currentUrl))
                return;

            var currentFolderPath = linkPart.FolderPath;

            if (!_storageProvider.FolderExists(currentFolderPath))
                _storageProvider.CreateFolder(currentFolderPath);

            var htmlWeb = new HtmlWeb();
            var document = htmlWeb.Load(currentUrl);

            DownloadHtml(ref document, linkPart);

            htmlWeb.Get(currentUrl, "/");

            var indexPath = _storageProvider.Combine(currentFolderPath, "index.html");
            if (_storageProvider.FileExists(indexPath))
                _storageProvider.DeleteFile(indexPath);
            var stream = _storageProvider.CreateFile(indexPath).OpenWrite();
            document.Save(stream);
        }

        private void DownloadHtml(ref HtmlDocument document, LinkPart linkPart)
        {
            if (_client == null) _client = new WebClient();

            var stylesheets = document.DocumentNode.Descendants("link")
                .Where(x => x.Attributes.Contains("rel") && (x.Attributes["rel"].Value.ToLower() == "stylesheet" || x.Attributes["rel"].Value.ToLower() == "shortcut icon") && x.Attributes.Contains("href"));

            var scripts = document.DocumentNode.Descendants("script")
                .Where(x => x.Attributes.Contains("src"));

            var images = document.DocumentNode.Descendants("img")
                .Where(x => x.Attributes.Contains("src"));

            var resources = stylesheets.Concat(scripts).Concat(images);

            // to skip duplication:
            var storedResources = new Dictionary<string, string>();

            foreach (var resource in resources)
            {
                var attrName = resource.Attributes["href"] != null ? "href" : "src";
                var source = resource.Attributes[attrName].Value;

                // Resource not downloaded yet
                if (!storedResources.ContainsKey(source))
                {
                    var downloadedFile = DownloadFile(linkPart, source);
                    if (downloadedFile != String.Empty)
                    {
                        resource.Attributes[attrName].Value = downloadedFile;
                        storedResources.Add(source, downloadedFile);
                    }
                }

                // Resource already downloaded
                else
                {
                    resource.Attributes[attrName].Value = storedResources[source];
                }
            }
        }

        private string DownloadFile(LinkPart linkPart, string source)
        {
            var baseUri = new Uri(linkPart.OriginalUrl, UriKind.Absolute);
            var currentUri = new Uri(baseUri, source);

            try
            {

                var folderPieces = currentUri.LocalPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                var destinationFolder = linkPart.FolderPath;

                for (var i = 0; i < folderPieces.Length-1; i++)
                    destinationFolder = _storageProvider.Combine(destinationFolder, folderPieces[i]);

                if (!_storageProvider.FolderExists(destinationFolder))
                    _storageProvider.CreateFolder(destinationFolder);

                var filename = Path.GetFileName(currentUri.LocalPath);
                var destinationFile = _storageProvider.Combine(destinationFolder, filename);

                if (_storageProvider.FileExists(destinationFile)) 
                    _storageProvider.DeleteFile(destinationFile);

                byte[] fileData = _client.DownloadData(currentUri);

                if (!_storageProvider.FileExists(destinationFile))
                {
                    var stream = _storageProvider.CreateFile(destinationFile).OpenWrite();
                    stream.Write(fileData, 0, fileData.Length);
                }

                if (_storageProvider.FileExists(destinationFile))
                    return _storageProvider.GetPublicUrl(destinationFile);
            }
            catch (PathTooLongException)
            {
                // todo
            }
            catch (UriFormatException)
            {
                // todo
            }
            catch (Exception)
            {
                // todo
            }
            return String.Empty;
        }

    }
}