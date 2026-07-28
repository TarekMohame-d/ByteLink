using System.Reflection;
using DbUp;
using dotenv.net;
using Microsoft.Extensions.Configuration;
using Npgsql;

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("==================================================");
Console.WriteLine("        🚀 INITIALIZING MIGRATION RUNNER          ");
Console.WriteLine("==================================================");
Console.ResetColor();

var shouldReset = args.Contains("--reset");

if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development")
{
    DotEnv.Fluent().WithProbeForEnv(probeLevelsToSearch: 10).WithTrimValues().Load();
}

IConfiguration configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

string GetRequiredSetting(string key) =>
    configuration[key] ?? throw new InvalidOperationException($"💥 Critical Error: Configuration key [{key}] is missing.");


var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? GetRequiredSetting("ConnectionStrings:DefaultConnection");

var adminId = GetRequiredSetting("AdminSettings:Id");
var adminEmail = GetRequiredSetting("AdminSettings:Email");
var adminFirstName = GetRequiredSetting("AdminSettings:FirstName");
var adminLastName = GetRequiredSetting("AdminSettings:LastName");
var adminPassword = GetRequiredSetting("AdminSettings:PasswordHash");

if (shouldReset)
{
    LogWarning("⚠️ Reset flag detected! Dropping the entire database and all its contents...");

    var connectionBuilder = new NpgsqlConnectionStringBuilder(connectionString);
    var targetDatabaseName = connectionBuilder.Database;

    if (!string.IsNullOrWhiteSpace(targetDatabaseName))
    {
        connectionBuilder.Database = "postgres";

        using var connection = new NpgsqlConnection(connectionBuilder.ConnectionString);
        connection.Open();

        var dropCommandText = $"DROP DATABASE IF EXISTS \"{targetDatabaseName}\" WITH (FORCE);";
        using var command = new NpgsqlCommand(dropCommandText, connection);
        command.ExecuteNonQuery();

        LogSuccess($"✔ Database [{targetDatabaseName}] has been completely dropped.");
    }
}

EnsureDatabase.For.PostgresqlDatabase(connectionString);

// Define Monolith Modules
var modules = new[] { "Identity", "Messaging" };

// Execute Migrations Per Module (Isolated Schemas)
foreach (var module in modules)
{
    var schemaName = module.ToLowerInvariant();
    var scriptPrefix = $"ByteLink.Migrations.Scripts.{module}.";

    LogSection($"Processing Module: [{module}] -> Target Schema: [{schemaName}]");

    using (var connection = new NpgsqlConnection(connectionString))
    {
        connection.Open();
        using var command = new NpgsqlCommand($"CREATE SCHEMA IF NOT EXISTS {schemaName};", connection);
        command.ExecuteNonQuery();
    }

    var upgrader = DeployChanges
        .To.PostgresqlDatabase(connectionString)
        .WithScriptsEmbeddedInAssembly(
            Assembly.GetExecutingAssembly(),
            scriptName => scriptName.StartsWith(scriptPrefix) && scriptName.EndsWith(".sql")
        )
        .WithTransaction()
        .JournalToPostgresqlTable(schemaName, "schema_versions")
        .LogToConsole()
        .WithVariables(
            new Dictionary<string, string>
            {
                { "adminUserId", adminId },
                { "adminFirstName", adminFirstName },
                { "adminLastName", adminLastName },
                { "adminEmail", adminEmail },
                { "adminPasswordHash", adminPassword }
            }
        )
        .Build();

    if (upgrader.IsUpgradeRequired())
    {
        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            LogError($"❌ Migration FAILED on module: {module}", result.Error.ToString());
            return -1;
        }

        LogSuccess($"✔ Module [{module}] migrated successfully.");
        continue;
    }

    LogSuccess($"✔ Module [{module}] is up to date.", ConsoleColor.DarkGreen);
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("\n============================================================");
Console.WriteLine("    🎉 ALL DATABASE MIGRATIONS COMPLETED SUCCESSFULLY!  ");
Console.WriteLine("============================================================");
Console.ResetColor();
return 0;

static void LogSection(string message)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"\n--- {message} ---");
    Console.ResetColor();
}

static void LogSuccess(string message, ConsoleColor color = ConsoleColor.Green)
{
    Console.ForegroundColor = color;
    Console.WriteLine(message);
    Console.ResetColor();
}

static void LogWarning(string message)
{
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine(message);
    Console.ResetColor();
}

static void LogError(string message, string details = "")
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine($"\n{message}");
    if (!string.IsNullOrWhiteSpace(details))
    {
        Console.Error.WriteLine($"Details: {details}");
    }
    Console.ResetColor();
}
