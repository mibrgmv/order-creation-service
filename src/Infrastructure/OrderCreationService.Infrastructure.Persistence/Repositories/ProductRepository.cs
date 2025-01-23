using Npgsql;
using OrderCreationService.Application.Abstractions.Queries;
using OrderCreationService.Application.Abstractions.Repositories;
using OrderCreationService.Application.Models.Models;
using OrderCreationService.Infrastructure.Persistence.Exceptions;
using OrderCreationService.Infrastructure.Persistence.Extensions;
using System.Data;
using System.Runtime.CompilerServices;

namespace OrderCreationService.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ProductRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<long[]> AddProductsAsync(IReadOnlyCollection<Product> products, CancellationToken cancellationToken)
    {
        const string sql = """
        insert into products (product_name, product_price) 
        select name, 
               price 
        from unnest(:names, :prices) as source(name, price)
        returning product_id;
        """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("names", products.Select(x => x.Name).ToArray())
            .AddParameter("prices", products.Select(x => x.Price).ToArray());

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        IList<long> ids = [];

        while (await reader.ReadAsync(cancellationToken)) ids.Add(reader.GetInt64("product_id"));

        return ids.ToArray();
    }

    public async Task<Product> GetProductAsync(long productId, CancellationToken cancellationToken)
    {
        const string sql = """
        select product_id, 
               product_name, 
               product_price
        from products
        where product_id = :productId
        """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("productId", productId);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return new Product(
                reader.GetInt64("product_id"),
                reader.GetString("product_name"),
                reader.GetDecimal("product_price"));
        }

        throw new ProductNotFoundException("Product Not Found");
    }

    public async IAsyncEnumerable<Product> QueryProductsAsync(ProductQuery query, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
        select product_id, 
               product_name, 
               product_price
        from products
        where (product_id > :cursor)
          and (CARDINALITY(:ids) = 0 or product_id = any (:ids))
          and (:name_pattern::text is null or product_name like :name_pattern::text)
          and (:min_price::money is null or product_price > :min_price::money)
          and (:max_price::money is null or product_price < :max_price::money)
        limit :page_size;
        """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using NpgsqlCommand command = new NpgsqlCommand(sql, connection)
            .AddParameter("ids", query.Ids)
            .AddParameter("name_pattern", query.NamePattern)
            .AddParameter("min_price", query.MinPrice)
            .AddParameter("max_price", query.MaxPrice)
            .AddParameter("cursor", query.Cursor)
            .AddParameter("page_size", query.PageSize);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new Product(
                reader.GetInt64("product_id"),
                reader.GetString("product_name"),
                reader.GetDecimal("product_price"));
        }
    }
}