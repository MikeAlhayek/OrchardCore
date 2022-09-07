using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Npgsql;
using OrchardCore.Data.YesSql.Abstractions;
using OrchardCore.Environment.Shell.Descriptor.Models;
using YesSql;
using YesSql.Provider.MySql;
using YesSql.Provider.PostgreSql;
using YesSql.Provider.Sqlite;
using YesSql.Provider.SqlServer;
using YesSql.Sql;

namespace OrchardCore.Data;

public class DbConnectionValidator : IDbConnectionValidator
{
    private readonly IEnumerable<DatabaseProvider> _databaseProviders;
    private readonly ITableNameConvention _tableNameConvention;
    private readonly YesSqlOptions _yesSqlOptions;

    public DbConnectionValidator(
        IEnumerable<DatabaseProvider> databaseProviders,
        ITableNameConvention tableNameConvention,
        IOptions<YesSqlOptions> yesSqlOptions
        )
    {
        _databaseProviders = databaseProviders;
        _tableNameConvention = tableNameConvention;
        _yesSqlOptions = yesSqlOptions.Value;
    }

    public async Task<DbConnectionValidatorResult> ValidateAsync(string databaseProvider, string connectionString, string tablePrefix)
    {
        if (String.IsNullOrWhiteSpace(databaseProvider))
        {
            return DbConnectionValidatorResult.NoProvider;
        }

        if (!Enum.TryParse(databaseProvider, out DatabaseProviderName providerName) || providerName == DatabaseProviderName.None)
        {
            return DbConnectionValidatorResult.UnsupportedProvider;
        }

        var provider = _databaseProviders.FirstOrDefault(x => x.Value == providerName);

        if (provider != null && !provider.HasConnectionString)
        {
            return DbConnectionValidatorResult.DocumentTableNotFound;
        }

        if (String.IsNullOrWhiteSpace(connectionString))
        {
            return DbConnectionValidatorResult.InvalidConnection;
        }

        var factory = GetFactory(providerName, connectionString);

        using var connection = factory.CreateConnection();

        try
        {
            await connection.OpenAsync();
        }
        catch
        {
            return DbConnectionValidatorResult.InvalidConnection;
        }

        var selectBuilder = GetSelectBuilderForDocumentTable(tablePrefix, providerName);

        try
        {
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = selectBuilder.ToSqlString();

            using var result = await selectCommand.ExecuteReaderAsync();

            // At this point, the query work and the 'Document' table exists.
            if (result.HasRows)
            {
                // At this point we know that the Document table and the ShellDescriptor record exists.
                // This also means that this table is used for a tenant.
                return DbConnectionValidatorResult.ShellDescriptorDocumentFound;
            }

            // At this point the Document table exists with no ShellDescriptor document.
            // This also means that this table is used for other purposes that a tenant (ex., Database Shells Configuration.)
            return DbConnectionValidatorResult.DocumentTableFound;
        }
        catch (SqlException e)
        {
            for (var i = 0; i < e.Errors.Count; i++)
            {
                if (e.Errors[i].Number == 207)
                {
                    // This means that the table exists but some expected columns do not exists.
                    // This likely to mean that the the table was not created using YesSql.
                    return DbConnectionValidatorResult.DocumentTableFound;
                }
                else if (e.Errors[i].Number == 208)
                {
                    return DbConnectionValidatorResult.DocumentTableNotFound;
                }
            }

            return DbConnectionValidatorResult.InvalidConnection;
        }
        catch (MySqlException e)
        {
            return e.ErrorCode switch
            {
                MySqlErrorCode.NoSuchTable => DbConnectionValidatorResult.DocumentTableNotFound,
                // This means that the table exists but some expected columns do not exists.
                // This likely to mean that the the table was not created using YesSql.
                MySqlErrorCode.BadFieldError => DbConnectionValidatorResult.DocumentTableFound,
                _ => DbConnectionValidatorResult.InvalidConnection,
            };
        }
        catch (PostgresException e)
        {
            return e.SqlState switch
            {
                //https://www.postgresql.org/docs/current/errcodes-appendix.html
                // 'undefined_table'
                "42P01" => DbConnectionValidatorResult.DocumentTableNotFound,

                // 'undefined_column' this likely to mean that the the table was not created using YesSql.
                "42703" => DbConnectionValidatorResult.DocumentTableFound,
                _ => DbConnectionValidatorResult.InvalidConnection
            };
        }
        catch
        {
            // At this point we know that the document table does not exist.
            return DbConnectionValidatorResult.DocumentTableNotFound;
        }
    }

    private ISqlBuilder GetSelectBuilderForDocumentTable(string tablePrefix, DatabaseProviderName providerName)
    {
        var selectBuilder = GetSqlBuilder(providerName, tablePrefix);
        selectBuilder.Select();
        // Here we explicitly select the expected column used by YesSql instead of '*'
        // to ensure that this table can be consumed by YesSql.
        selectBuilder.AddSelector("Id, Type, Content, Version");
        selectBuilder.Table(_tableNameConvention.GetDocumentTable());
        selectBuilder.WhereAnd($"Type = '{typeof(ShellDescriptor).FullName}, {typeof(ShellDescriptor).Assembly.GetName().Name}'");
        selectBuilder.Take("1");

        return selectBuilder;
    }

    private static IConnectionFactory GetFactory(DatabaseProviderName providerName, string connectionString)
    {
        return providerName switch
        {
            DatabaseProviderName.SqlConnection => new DbConnectionFactory<SqlConnection>(connectionString),
            DatabaseProviderName.MySql => new DbConnectionFactory<MySqlConnection>(connectionString),
            DatabaseProviderName.Sqlite => new DbConnectionFactory<SqliteConnection>(connectionString),
            DatabaseProviderName.Postgres => new DbConnectionFactory<NpgsqlConnection>(connectionString),
            _ => throw new ArgumentOutOfRangeException(nameof(providerName), "Unsupported database provider"),
        };
    }

    private ISqlBuilder GetSqlBuilder(DatabaseProviderName providerName, string tablePrefix)
    {
        ISqlDialect dialect = providerName switch
        {
            DatabaseProviderName.SqlConnection => new SqlServerDialect(),
            DatabaseProviderName.MySql => new MySqlDialect(),
            DatabaseProviderName.Sqlite => new SqliteDialect(),
            DatabaseProviderName.Postgres => new PostgreSqlDialect(),
            _ => throw new ArgumentOutOfRangeException(nameof(providerName), "Unsupported database provider"),
        };

        var prefix = String.Empty;

        if (!String.IsNullOrEmpty(tablePrefix))
        {
            prefix = tablePrefix.Trim() + (_yesSqlOptions.TablePrefixSeparator ?? String.Empty);
        }

        return new SqlBuilder(prefix, dialect);
    }
}
