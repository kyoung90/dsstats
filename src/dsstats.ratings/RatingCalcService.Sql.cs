using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace dsstats.ratings;

public abstract partial class RatingCalcService
{
    private async Task Csv2Mysql(string fileName,
                        string tableName,
                        string connectionString)
    {
        if (!File.Exists(fileName))
        {
            logger.LogWarning("file not found: {filename}", fileName);
            return;
        }

        var tempTable = tableName + "_temp";
        var oldTable = tempTable + "_old";

        try
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandTimeout = 360;

            command.CommandText = @$"
DROP TABLE IF EXISTS {tempTable};
DROP TABLE IF EXISTS {oldTable};
CREATE TABLE {tempTable} LIKE {tableName};
SET FOREIGN_KEY_CHECKS = 0;
LOAD DATA INFILE '{fileName}' INTO TABLE {tempTable}
COLUMNS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '""' ESCAPED BY '""' LINES TERMINATED BY '\r\n';

RENAME TABLE {tableName} TO {oldTable}, {tempTable} TO {tableName};
DROP TABLE {oldTable};
SET FOREIGN_KEY_CHECKS = 1;";

            await command.ExecuteNonQueryAsync();

            File.Delete(fileName);
        }
        catch (Exception ex)
        {
            logger.LogError("failed writing csv2sql {filename}: {error}", fileName, ex.Message);
        }
    }

    private async Task ContinueCsv2Mysql(string fileName,
                    string tableName,
                    string connectionString)
    {
        if (!File.Exists(fileName))
        {
            logger.LogWarning("file not found: {filename}", fileName);
            return;
        }

        try
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandTimeout = 360;

            command.CommandText = @$"
SET FOREIGN_KEY_CHECKS = 0;
LOAD DATA INFILE '{fileName}' INTO TABLE {tableName}
COLUMNS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '""' ESCAPED BY '""' LINES TERMINATED BY '\r\n';

SET FOREIGN_KEY_CHECKS = 1;";

            await command.ExecuteNonQueryAsync();

            File.Delete(fileName);
        }
        catch (Exception ex)
        {
            logger.LogError("failed continue writing csv2sql {filename}: {error}", fileName, ex.Message);
        }
    }
}
