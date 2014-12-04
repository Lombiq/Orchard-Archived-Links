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
    /// <summary>
    /// Making backup from web resources
    /// </summary>
    public interface ILinkManager : IDependency
    {
        /// <summary>
        /// This method should implement the archiving process.
        /// Creates directories, copy files from their original place,
        /// </summary>
        /// <param name="linkPart">This LinkPart tells from url (website) to snaphot</param>
        void SaveLink(LinkPart linkPart);

        /// <summary>
        /// Searches the archived link by id and returns its index.html
        /// </summary>
        /// <param name="id">LinkPart id</param>
        /// <returns>public url of index.html</returns>
        string GetPublicUrl(int id);

        /// <summary>
        /// Checks the given url is available
        /// </summary>
        /// <param name="url">url to check</param>
        /// <returns>If original url is available it will be returned</returns>
        string CheckUrl(string url);

        /// <summary>
        /// Use this method to get the archived link if original is not available.
        /// </summary>
        /// <param name="jumpUrl">jump url</param>
        /// <returns>If original url is available it will be returned</returns>
        string UseJumpUrl(string jumpUrl);
    }
}
