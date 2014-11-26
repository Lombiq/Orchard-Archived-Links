using Lombiq.ArchivedLinks.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.ArchivedLinks
{
    public class ContentMigrations : DataMigrationImpl
    {
        public int Create()
        {
            ContentDefinitionManager.AlterTypeDefinition("Link",
                cfg => cfg
                    .WithPart(typeof(LinkPart).Name)
                    .WithPart("CommonPart")
                    .Creatable()
                    .Draftable());

            return 1;
        }
    }
}