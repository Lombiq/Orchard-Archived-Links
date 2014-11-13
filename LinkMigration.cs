using Lombiq.ArchivedLinks.Models;
using Orchard.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.ArchivedLinks
{
    public class LinkMigration : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable(typeof(LinkRecord).Name,
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("OriginalUrl")
                    .Column<string>("FolderPath")
                    .Column<DateTime>("LastModified", column => column.Nullable().WithDefault(null))
                    .Column<DateTime>("Created")
                );

            return 1;
        }
    }
}