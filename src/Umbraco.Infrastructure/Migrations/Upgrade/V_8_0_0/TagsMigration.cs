﻿using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0
{
    public class TagsMigration : MigrationBase
    {
        public TagsMigration(IMigrationContext context)
            : base(context)
        { }

        protected override void Migrate()
        {
            // alter columns => non-null
            AlterColumn<TagDto>(Cms.Core.Constants.DatabaseSchema.Tables.Tag, "group");
            AlterColumn<TagDto>(Cms.Core.Constants.DatabaseSchema.Tables.Tag, "tag");

            // kill unused parentId column
            Delete.Column("ParentId").FromTable(Cms.Core.Constants.DatabaseSchema.Tables.Tag).Do();
        }
    }
}
