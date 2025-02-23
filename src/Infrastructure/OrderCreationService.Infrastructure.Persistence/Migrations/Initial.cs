using FluentMigrator;

namespace OrderCreationService.Infrastructure.Persistence.Migrations;

[Migration(1, nameof(Initial))]
public class Initial : Migration
{
    public override void Up()
    {
        const string sql =
        """
        create table products
        (
            product_id    bigint primary key generated always as identity,
        
            product_name  text  not null,
            product_price money not null
        );

        create type order_state as enum 
        (
            'created',
            'processing',
            'completed',
            'cancelled'
        );

        create table orders
        (
            order_id         bigint primary key generated always as identity,
        
            order_state      order_state              not null,
            order_created_at timestamp with time zone not null,
            order_created_by text                     not null
        );

        create table order_items
        (
            order_item_id       bigint primary key generated always as identity,
            order_id            bigint  not null references orders (order_id),
            product_id          bigint  not null references products (product_id),
        
            order_item_quantity int     not null,
            order_item_deleted  boolean not null
        );

        create type order_history_item_kind as enum 
        (
            'created',
            'item_added',
            'item_removed',
            'state_changed'
        );

        create table order_history
        (
            order_history_item_id         bigint primary key generated always as identity,
            order_id                      bigint not null references orders (order_id),
        
            order_history_item_created_at timestamp with time zone not null,
            order_history_item_kind       order_history_item_kind  not null,
            order_history_item_payload    jsonb                    not null
        );

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
        drop table order_history;
        drop table order_items;
        drop table orders;
        drop table products;
        drop type order_history_item_kind;
        drop type order_state;
        drop table inbox;
        drop table outbox;
        """;

        Execute.Sql(sql);
    }
}