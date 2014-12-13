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
    /// Making backup from web resources (websites, images, pdfs)
    /// </summary>
    public interface ISnapshotManager : IDependency
    {
        /// <summary>
        /// This method implements the archiving process.
        /// Creates directories, copies files from their original place.
        /// </summary>
        /// <param name="url">The url to save</param>
        void SaveLink(string url);

        /// <summary>
        /// Searches and returns the saved url
        /// </summary>
        /// <param name="url">The url to check whether it has been archived</param>
        /// <returns>Public url of the snapshot or empty if url not saved</returns>
        string GetSnapshotIndexPubliUrl(string url);

        /// <summary>
        /// Checks if the given url is available
        /// </summary>
        /// <param name="url">url to check</param>
        /// <returns>If original url is available it will be returned</returns>
        bool CheckUrlAvailable(string url);
    }
}
