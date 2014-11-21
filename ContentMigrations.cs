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
            //SchemaBuilder.CreateTable(typeof(LinkPart).Name,
            //        table => table
            //            .ContentPartRecord()
            //            .Column<string>("OriginalUrl")
            //            .Column<string>("FolderPath")
            //            .Column<DateTime>("LastModified", column => column.Nullable().WithDefault(null))
            //            .Column<DateTime>("Created", column => column.WithDefault(DateTime.Now))
            //        );

            //ContentDefinitionManager.AlterPartDefinition(typeof(LinkPart).Name,
            //    part => part.Attachable());

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