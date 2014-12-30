using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.ArchivedLinks.ViewModels
{
    public class SnapshotIframeViewModel
    {
        public DateTime SnapshotTaken { get; set; }

        public string SnapshotIndexPublicUrl { get; set; }
    }
}