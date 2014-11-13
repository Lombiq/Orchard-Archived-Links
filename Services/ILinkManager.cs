using Lombiq.ArchivedLinks.Models;
using Orchard;
using Orchard.FileSystems.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lombiq.ArchivedLinks.Services
{
    public interface ILinkManager : IDependency
    {
        IEnumerable<LinkRecord> GetLinks();

        LinkRecord GetLink(int id);

        LinkRecord GetLink(string url);

        string SaveLink(string originalUrl, IStorageProvider storageProvider);
    }
}
