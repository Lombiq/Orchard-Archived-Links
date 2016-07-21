using System;

namespace Lombiq.ArchivedLinks.ViewModels
{
    public class SnapshotViewModel
    {
        public DateTime SnapshotTaken { get; set; }

        public Uri SnapshotIndexPublicUrl { get; set; }
    }
}