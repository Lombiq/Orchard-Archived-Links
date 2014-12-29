using HtmlAgilityPack;
using Lombiq.ArchivedLinks.Models;
using Orchard;
using Orchard.Data;
using Orchard.Environment;
using Orchard.FileSystems.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using Orchard.Environment.Extensions;
using System.Threading.Tasks;

namespace Lombiq.ArchivedLinks.Services
{
    public class SnapshotManager : ISnapshotManager
    {
        private readonly IStorageProvider _storageProvider;


        public SnapshotManager(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }


        public void SaveLink(Uri uri)
        {
            var urlString = uri.ToString();

            // This folder will be created before download process will copy files into it
            var folderPath = _storageProvider.Combine("_ArchivedLinks", urlString.GetHashCode().ToString());

            var contentType = GetContentType(uri);

            var allowableFileContentTypes = new string[] { "application/pdf", "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (allowableFileContentTypes.Contains(contentType))
            {
                DownloadFile(urlString, uri, folderPath, true);
            }
            else if (contentType == "text/html")
            {
                var htmlWeb = new HtmlWeb();
                var document = htmlWeb.Load(urlString);

                DownloadHtml(ref document, uri, folderPath);
                htmlWeb.Get(urlString, "/");

                var indexPath = _storageProvider.Combine(folderPath, "index.html");
                if (_storageProvider.FileExists(indexPath)) _storageProvider.DeleteFile(indexPath);

                using (var stream = _storageProvider.CreateFile(indexPath).OpenWrite())
                {
                    document.Save(stream);
                }
            }
            else
            {
                throw new NotSupportedException("Uri type not supported");
            }
        }

        public string GetSnapshotIndexPublicUrl(Uri uri)
        {
            var contentType = GetContentType(uri);

            var uriString = uri.ToString();

            var allowableFileContentTypes = new string[] { "application/pdf", "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (allowableFileContentTypes.Contains(contentType))
            {
                var filename = Path.GetFileName(uri.LocalPath);
                return _storageProvider.GetPublicUrl(_storageProvider.Combine(_storageProvider.Combine("_ArchivedLinks", uriString.GetHashCode().ToString()), filename));
            }
            else if (contentType == "text/html")
            {
                return _storageProvider.GetPublicUrl(_storageProvider.Combine(_storageProvider.Combine("_ArchivedLinks", uriString.GetHashCode().ToString()), "index.html"));
            }
            else
            {
                return string.Empty;
            }
        }

        public async Task<bool> CheckUriIsAvailable(Uri uri)
        {
            var request = WebRequest.Create(uri.ToString());
            request.Method = "HEAD";

            try
            {
                // If no exception the status code is '2xx', so return true.
                await request.GetResponseAsync();
                return true;
            }
            catch (WebException)
            {
                // If status is not '2xx' return false.
                return false;
            }
        }

        public void RemoveLink(Uri uri)
        {
            var folderPath = _storageProvider.Combine("_ArchivedLinks", uri.ToString().GetHashCode().ToString());

            if (_storageProvider.FolderExists(folderPath))
            {
                _storageProvider.DeleteFolder(folderPath);
            }
        }


        private void DownloadHtml(ref HtmlDocument document, Uri uri, string folderPath)
        {
            var stylesheets = document.DocumentNode.Descendants("link")
                .Where(x => x.Attributes.Contains("rel") && (x.Attributes["rel"].Value.ToLower() == "stylesheet" || x.Attributes["rel"].Value.ToLower() == "shortcut icon") && x.Attributes.Contains("href"));

            var images = document.DocumentNode.Descendants("img")
                .Where(x => x.Attributes.Contains("src"));

            var pdfs = document.DocumentNode.Descendants("a")
                .Where(x => x.Attributes.Contains("href") && x.Attributes["href"].Value.ToLower().Contains(".pdf"));

            var resources = stylesheets.Concat(pdfs).Concat(images);

            // To skip duplication:
            var storedResources = new Dictionary<string, string>();

            foreach (var resource in resources)
            {
                var attrName = resource.Attributes["href"] != null ? "href" : "src";
                var source = resource.Attributes[attrName].Value;

                // Resource not downloaded yet.
                if (!storedResources.ContainsKey(source))
                {
                    var downloadedFile = DownloadFile(source, uri, folderPath);
                    if (downloadedFile != string.Empty)
                    {
                        resource.Attributes[attrName].Value = downloadedFile;
                        storedResources.Add(source, downloadedFile);
                    }
                }
                // Resource already downloaded.
                else
                {
                    resource.Attributes[attrName].Value = storedResources[source];
                }
            }

            // Adding timestamp to be able to establish when the snapshot was taken
            var documentBody = document.DocumentNode.SelectSingleNode("//body");
            if (documentBody != null)
            {
                var dateNode = document.CreateElement("p");
                dateNode.InnerHtml = String.Format("Snapshot taken at {0}", DateTime.UtcNow);
                dateNode.SetAttributeValue("style", "width: 100%;z-index: 99999;position: absolute;background-color:white; color:black;");
                documentBody.PrependChild(dateNode);
            }
        }

        private string DownloadFile(string source, Uri uri, string folderPath, bool isSingleFile = false)
        {
            var destinationFolder = folderPath;

            var currentUri = new Uri(uri, source);

            if (!isSingleFile)
            {
                var folderSegments = currentUri.LocalPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                for (var i = 0; i < folderSegments.Length - 1; i++)
                {
                    destinationFolder = _storageProvider.Combine(destinationFolder, folderSegments[i]);
                }
            }

            if (!_storageProvider.FolderExists(destinationFolder)) _storageProvider.CreateFolder(destinationFolder);

            var filename = Path.GetFileName(currentUri.LocalPath);
            var destinationFile = _storageProvider.Combine(destinationFolder, filename);

            if (_storageProvider.FileExists(destinationFile)) _storageProvider.DeleteFile(destinationFile);

            using (var _client = new WebClient())
            {
                var fileData = _client.DownloadData(currentUri);
                using (var stream = _storageProvider.CreateFile(destinationFile).OpenWrite())
                {
                    stream.Write(fileData, 0, fileData.Length);
                }
            }

            // If file creation successful
            if (_storageProvider.FileExists(destinationFile))
                return _storageProvider.GetPublicUrl(destinationFile);

            return string.Empty;
        }

        private string GetContentType(Uri uri)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri.ToString());
            request.Method = "HEAD";

            var response = (HttpWebResponse)request.GetResponse();

            // If the content is html the content type may contain extra information
            // eg: 'text/html;charset=UTF-8'. This is why Contains method is necessary.
            if (response.ContentType.ToLower().Contains("text/html"))
                return "text/html";

            return response.ContentType.ToLower();
        }
    }
}