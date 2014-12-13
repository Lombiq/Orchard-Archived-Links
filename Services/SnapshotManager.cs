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

namespace Lombiq.ArchivedLinks.Services
{
    public class SnapshotManager : ISnapshotManager
    {
        private WebClient _client;

        private readonly IStorageProvider _storageProvider;

        private readonly IWorkContextAccessor _workContextAccessor;
        

        public SnapshotManager(IStorageProvider storageProvider, IWorkContextAccessor workContextAccessor)
        {
            _storageProvider = storageProvider;
            _workContextAccessor = workContextAccessor;
        }


        public void SaveLink(string url)
        {
            try
            {
                var uriBuilder = new UriBuilder(url);

                var urlString = uriBuilder.Uri.ToString();

                var folderPath = _storageProvider.Combine("_ArchivedLinks", GetHash(urlString));

                if (!_storageProvider.FolderExists(folderPath)) _storageProvider.CreateFolder(folderPath);

                var contentType = GetContentType(uriBuilder.Uri);

                switch (contentType)
                {
                    case "application/pdf":
                    case "image/jpeg":
                    case "image/jpg":
                    case "image/png":
                    case "image/gif":
                        DownloadFile(urlString, uriBuilder.Uri, folderPath, true);
                        break;

                    case "text/html":
                        var htmlWeb = new HtmlWeb();

                        var document = htmlWeb.Load(urlString);

                        DownloadHtml(ref document, uriBuilder.Uri, folderPath);

                        htmlWeb.Get(urlString, "/");

                        var indexPath = _storageProvider.Combine(folderPath, "index.html");

                        if (_storageProvider.FileExists(indexPath)) _storageProvider.DeleteFile(indexPath);

                        var stream = _storageProvider.CreateFile(indexPath).OpenWrite();

                        document.Save(stream);

                        stream.Close();

                        break;

                    default:
                        throw new Exception("Url type not supported");
                }
            }
            catch (UriFormatException ex)
            {
                throw new UriFormatException("Url can not be empty", ex);
            }
            catch (Exception)
            {
                throw;
            }

            
        }

        public string GetSnapshotIndexPubliUrl(string url)
        {
            var uriBuilder = new UriBuilder(url);

            var contentType = GetContentType(uriBuilder.Uri);

            var uriString = uriBuilder.Uri.ToString();

            switch (contentType)
            {
                case "application/pdf":
                case "image/jpeg":
                case "image/jpg":
                case "image/png":
                case "image/gif":

                    var filename = Path.GetFileName(uriBuilder.Uri.LocalPath);

                    return _storageProvider.GetPublicUrl(_storageProvider.Combine(_storageProvider.Combine("_ArchivedLinks", GetHash(uriString)), filename));

                case "text/html":
                    return _storageProvider.GetPublicUrl(_storageProvider.Combine(_storageProvider.Combine("_ArchivedLinks", GetHash(uriString)), "index.html"));

                default:
                    return String.Empty;

            }            
        }

        public bool CheckUrlAvailable(string url)
        {
            var uriBuilder = new UriBuilder(url);

            HttpWebResponse response = null;

            var request = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri.ToString());

            request.Method = "HEAD";

            try
            {
                // if no exception (status is '2xx' return original url)
                response = (HttpWebResponse)request.GetResponse();

                return true;
            }
            catch (WebException)
            {
                // if status is not '2xx'
                return false;
            }
        }


        private string GetHash(string url)
        {
            using (var md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(url));

                var stringBuilder = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                    stringBuilder.Append(data[i].ToString("x2"));

                return stringBuilder.ToString();
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

            // to skip duplication:
            var storedResources = new Dictionary<string, string>();

            foreach (var resource in resources)
            {
                var attrName = resource.Attributes["href"] != null ? "href" : "src";
                var source = resource.Attributes[attrName].Value;

                // Resource not downloaded yet
                if (!storedResources.ContainsKey(source))
                {
                    var downloadedFile = DownloadFile(source, uri, folderPath);
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

        private string DownloadFile(string source, Uri uri, string folderPath, bool isSingleFile = false)
        {
            if (_client == null) _client = new WebClient();

            try
            {
                string destinationFolder = folderPath;

                var currentUri = new Uri(uri, source);

                if (!isSingleFile)
                {
                    var folderPieces = currentUri.LocalPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                    for (var i = 0; i < folderPieces.Length - 1; i++)
                    {
                        destinationFolder = _storageProvider.Combine(destinationFolder, folderPieces[i]);
                    }
                }

                if (!_storageProvider.FolderExists(destinationFolder)) _storageProvider.CreateFolder(destinationFolder);

                var filename = Path.GetFileName(currentUri.LocalPath);

                var destinationFile = _storageProvider.Combine(destinationFolder, filename);

                if (_storageProvider.FileExists(destinationFile)) _storageProvider.DeleteFile(destinationFile);

                byte[] fileData = _client.DownloadData(currentUri);

                if (!_storageProvider.FileExists(destinationFile))
                {
                    var stream = _storageProvider.CreateFile(destinationFile).OpenWrite();

                    stream.Write(fileData, 0, fileData.Length);

                    stream.Close();
                }

                if (_storageProvider.FileExists(destinationFile))
                    _storageProvider.GetPublicUrl(destinationFile);
            }
            catch (Exception)
            {
                if (isSingleFile) throw;
            }
            return String.Empty;
        }

        private string GetContentType(Uri uri)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uri.ToString());

                request.Method = "HEAD";

                var response = (HttpWebResponse)request.GetResponse();

                if (response.ContentType.ToLower().Contains("text/html"))
                    return "text/html";

                return response.ContentType.ToLower();
            }
            catch
            {
                return String.Empty;
            }
        }

    }
}