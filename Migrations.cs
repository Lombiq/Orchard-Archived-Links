using Lombiq.ArchivedLinks.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

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
                    .Listable());

            return 1;
        }
    }
}