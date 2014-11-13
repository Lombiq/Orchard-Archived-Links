using Lombiq.ArchivedLinks.Models;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.ArchivedLinks.Services
{
    public class LinkManager : ILinkManager
    {
        private readonly IRepository<LinkRecord> _linkRepository;

        public LinkManager(IRepository<LinkRecord> linkRepository)
        {
            _linkRepository = linkRepository;
        }

        public IEnumerable<LinkRecord> GetLinks()
        {
            return _linkRepository.Table;
        }

        public LinkRecord GetLink(int id)
        {
            return _linkRepository.Table.Where(link => link.Id == id).FirstOrDefault();
        }

        public LinkRecord GetLink(string url)
        {
            return _linkRepository.Table.Where(link => link.OriginalUrl == url).FirstOrDefault();
        }

        public string SaveLink(string originalUrl, Orchard.FileSystems.Media.IStorageProvider storageProvider)
        {
            if (String.IsNullOrEmpty(originalUrl))
                throw new ArgumentException("URL can not be empty");

            var searchUri = new Uri(originalUrl);
            var currentLink = GetLink(originalUrl);

            if (currentLink == null)
            {
                currentLink = new LinkRecord();
                currentLink.Created = DateTime.Now;
                currentLink.OriginalUrl = originalUrl;
                _linkRepository.Create(currentLink);
            }
            else
            {
                currentLink.LastModified = DateTime.Now;
            }

            return currentLink.Archiving(storageProvider);   
        }

    }
}