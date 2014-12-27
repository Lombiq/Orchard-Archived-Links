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

namespace Lombiq.ArchivedLinks.Services
{
    public class SnapshotManager : ISnapshotManager
    {
        private readonly IStorageProvider _storageProvider;
        private readonly IWorkContextAccessor _workContextAccessor;


        public SnapshotManager(IStorageProvider storageProvider, IWorkContextAccessor workContextAccessor)
        {
            _storageProvider = storageProvider;
            _workContextAccessor = workContextAccessor;
        }


        public void SaveLink(Uri uri)
        {
            var urlString = uri.ToString();

            // This folder will be created before download process will copy files into it
            var folderPath = _storageProvider.Combine("_ArchivedLinks", urlString.GetHashCode().ToString());

            var contentType = GetContentType(uri);

            switch (contentType)
            {
                case "application/pdf":
                case "image/jpeg":
                case "image/jpg":
                case "image/png":
                case "image/gif":
                    DownloadFile(urlString, uri, folderPath, true);
                    break;

                case "text/html":
                    var htmlWeb = new HtmlWeb();
                    var document = htmlWeb.Load(urlString);

                    DownloadHtml(ref document, uri, folderPath);
                    htmlWeb.Get(urlString, "/");

                    var indexPath = _storageProvider.Combine(folderPath, "index.html");
                    if (_storageProvider.FileExists(indexPath)) _storageProvider.DeleteFile(indexPath);

                    var stream = _storageProvider.CreateFile(indexPath).OpenWrite();
                    document.Save(stream);
                    stream.Close();
                    break;

                default:
                    throw new Exception("Uri type not supported");
            }
        }

        public string GetSnapshotIndexPublicUrl(Uri uri)
        {
            var contentType = GetContentType(uri);

            var uriString = uri.ToString();

            switch (contentType)
            {
                case "application/pdf":
                case "image/jpeg":
                case "image/jpg":
                case "image/png":
                case "image/gif":
                    var filename = Path.GetFileName(uri.LocalPath);
                    return _storageProvider.GetPublicUrl(_storageProvider.Combine(_storageProvider.Combine("_ArchivedLinks", uriString.GetHashCode().ToString()), filename));

                case "text/html":
                    return _storageProvider.GetPublicUrl(_storageProvider.Combine(_storageProvider.Combine("_ArchivedLinks", uriString.GetHashCode().ToString()), "index.html"));

                default:
                    return string.Empty;
            }
        }

        public bool CheckUriIsAvailable(Uri uri)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri.ToString());
            request.Method = "HEAD";

            try
            {
                // If no exception the status code is '2xx', so return true.
                request.GetResponse();
                return true;
            }
            catch (WebException)
            {
                // If status is not '2xx' return false.
                return false;
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

                var stream = _storageProvider.CreateFile(destinationFile).OpenWrite();
                stream.Write(fileData, 0, fileData.Length);
                stream.Close();
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

            // If the content is html the content type maybe contains extra informations
            // eg: 'text/html;charset=UTF-8'. This is why Contains method is necessary.
            if (response.ContentType.ToLower().Contains("text/html"))
                return "text/html";

            return response.ContentType.ToLower();
        }
    }
}