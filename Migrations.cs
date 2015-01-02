using Lombiq.ArchivedLinks.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.ArchivedLinks
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable(typeof(ArchivedLinkPartRecord).Name,
                table => table
                    .ContentPartRecord()
                    .Column<string>("OriginalUrl")
                )
            .AlterTable(typeof(ArchivedLinkPartRecord).Name,
                table => table
                    .CreateIndex("OriginalUrlIndex", new string[] { "OriginalUrl" })
                );

            ContentDefinitionManager.AlterTypeDefinition("ArchivedLink",
                cfg => cfg
                    .DisplayedAs("Archived Link")
                    .WithPart(typeof(ArchivedLinkPart).Name)
                    .WithPart("CommonPart")
                    .Creatable()
                    .Draftable());

            return 1;
        }
    }
}