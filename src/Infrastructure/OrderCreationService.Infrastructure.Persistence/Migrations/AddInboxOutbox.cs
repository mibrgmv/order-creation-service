using FluentMigrator;

namespace OrderCreationService.Infrastructure.Persistence.Migrations;

[Migration(2, nameof(AddInboxOutbox))]
public class AddInboxOutbox : Migration
{
    public override void Up()
    {
        const string sql =
        """
        create table outbox
        (
            message_id           bigint    primary key generated always as identity,
            
            message_type         text      not null,
            message_key          bytea     not null,
            message_value        bytea     not null,
            message_created_at   timestamp with time zone not null,
            message_processed_at timestamp with time zone
        );

        create unique index inbox_key_value_idx on outbox (message_key, message_value);

        create table inbox
        (
            message_id           bigint    primary key generated always as identity,
            
            message_key          bytea     not null,
            message_value        bytea     not null,
            message_created_at   timestamp with time zone not null,
            message_processed_at timestamp with time zone
        );
        
        create unique index outbox_key_value_idx on inbox (message_key, message_value);
        """;

        Execute.Sql(sql);
    }

    public override void Down()
    {
        const string sql =
        """
        drop table inbox;
        drop table outbox;
        """;

        Execute.Sql(sql);
    }
}