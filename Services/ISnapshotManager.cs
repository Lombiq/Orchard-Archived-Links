using Lombiq.ArchivedLinks.Models;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lombiq.ArchivedLinks.Services
{
    /// <summary>
    /// Makes backup from web resources (websites, images, pdfs).
    /// The result is a copy on your server what you can present the user if the original source is down.
    /// </summary>
    public interface ISnapshotManager : IDependency
    {
        /// <summary>
        /// This method implements the archiving process.
        /// Creates directories, copies files from their original place.
        /// </summary>
        /// <param name="uri">The uri to save.</param>
        void SaveSnapshot(Uri uri);

        /// <summary>
        /// Searches and returns the saved uri.
        /// </summary>
        /// <param name="uri">The uri to check whether it has been archived.</param>
        /// <returns>Public uri of the snapshot or empty if uri not saved.</returns>
        Uri GetSnapshotIndexPublicUrl(Uri uri);

        /// <summary>
        /// Checks whether the given uri is available.
        /// </summary>
        /// <param name="uri">Uri to check.</param>
        /// <returns>True if the original uri is available.</returns>
        Task<bool> CheckUriIsAvailable(Uri uri);

        /// <summary>
        /// Removes the snapshot of the given uri.
        /// </summary>
        /// <param name="uri">Uri to remove.</param>
        void RemoveSnapshot(Uri uri);
    }
}
