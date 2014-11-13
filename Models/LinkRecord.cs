using HtmlAgilityPack;
using Orchard.FileSystems.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Lombiq.ArchivedLinks.Models
{
    public class LinkRecord
    {
        private WebClient _client;        
        private IStorageProvider _storageProvider;

        
        public virtual int Id { get; set; }

        public virtual string OriginalUrl { get; set; }

        public virtual string FolderPath { get; set; }

        public virtual DateTime? LastModified { get; set; }

        public virtual DateTime Created { get; set; }


        public string Archiving(IStorageProvider storageProvider)
        {
            if (String.IsNullOrEmpty(OriginalUrl)) return null;

            _storageProvider = storageProvider;

            FolderPath = _storageProvider.Combine("ArchivedLinks", Id.ToString());

            if (!_storageProvider.FolderExists(FolderPath))
                _storageProvider.CreateFolder(FolderPath);

            var htmlWeb = new HtmlWeb();
            var document = htmlWeb.Load(OriginalUrl);

            DownloadHtml(ref document);

            htmlWeb.Get(OriginalUrl, "/");

            var indexPath = _storageProvider.Combine(FolderPath, "index.html");
            var stream = _storageProvider.CreateFile(indexPath).OpenWrite();
            document.Save(stream);

            return _storageProvider.GetPublicUrl(indexPath);
        }

        private void DownloadHtml(ref HtmlDocument document)
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
                var src = resource.Attributes[attrName].Value;

                // Resource not downloaded yet
                if (!storedResources.ContainsKey(src))
                {
                    var downloadedFile = DownloadFile(src);
                    if (downloadedFile != String.Empty)
                    {
                        resource.Attributes[attrName].Value = downloadedFile;
                        storedResources.Add(src, downloadedFile);
                    }
                }

                // Resource already downloaded
                else
                {
                    resource.Attributes[attrName].Value = storedResources[src];
                }
            }
        }

        private string DownloadFile(string filePath)
        {
            var baseUri = new Uri(OriginalUrl, UriKind.Absolute);
            var currentUri = new Uri(baseUri, filePath);

            try
            {
                var dstFolder = FolderPath + Path.GetDirectoryName(currentUri.LocalPath);
                if (!_storageProvider.FolderExists(dstFolder))
                {
                    _storageProvider.CreateFolder(dstFolder);
                }

                var filename = Path.GetFileName(currentUri.LocalPath);

                var dstFile = Path.Combine(dstFolder, filename);
                byte[] fileData = _client.DownloadData(currentUri);

                if (!_storageProvider.FileExists(dstFile))
                {
                    var stream = _storageProvider.CreateFile(dstFile).OpenWrite();
                    stream.Write(fileData, 0, fileData.Length);
                }

                if (_storageProvider.FileExists(dstFile))
                {
                    return _storageProvider.GetPublicUrl(dstFile);
                }
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