using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.ArchivedLinks.ViewModels
{
    public class SnapshotViewModel
    {
        public DateTime SnapshotTaken { get; set; }

        public Uri SnapshotIndexPublicUrl { get; set; }
    }
}