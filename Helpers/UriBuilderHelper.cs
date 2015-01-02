using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.ArchivedLinks.Helpers
{
    public static class UriBuilderHelper
    {
        public static Uri TryCreateUri(string uriString)
        {
            Uri uri;
            if (!Uri.TryCreate(uriString, UriKind.Absolute, out uri) && !Uri.TryCreate("http://" + uriString, UriKind.Absolute, out uri))
            {
                throw new UriFormatException();
            }

            return uri;
        }
    }
}