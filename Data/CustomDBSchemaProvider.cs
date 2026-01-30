using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.Xpo.DB;

namespace DXApplication1.Data
{
    public class CustomDBSchemaProvider : DBSchemaProviderEx
    {
        // Define the list of tables you want to EXCLUDE
        private readonly List<string> _excludedTables = new List<string>
        {
            "sysdiagrams",   // Common system table
            "SecretTable",   // Example placeholder
            "AdminLogs" ,     // Example placeholder
            "HangFire.AggregatedCounter",
            "HangFire.Counter",
            "HangFire.Hash",
            "HangFire.Job",
            "HangFire.List",
            "HangFire.Set",
            "HangFire.State",
            "HangFire.Schema",
            "HangFire.Server",
            "HangFire.JobParameter",
            "HangFire.JobQueue",
            "__EFMigrationsHistory",
            "AcronymDefinition",
            "BatchRecords",
            "Allowances",
            "BulkPayslipSharings",
            "bulkPayslipProcessingDetails",
            "DeductionAllowancRawImports",
            "EmployeeDeductionParameterImportBatches",

        };

        public override DBTable[] GetTables(SqlDataConnection connection, params string[] tableList)
        {
            // Get the default list of tables
            var tables = base.GetTables(connection, tableList);

            // Filter out tables that match our excluded list (case-insensitive)
            return tables
                .Where(table => !_excludedTables.Contains(table.Name, StringComparer.OrdinalIgnoreCase))
                .ToArray();
        }

        public override DBTable[] GetViews(SqlDataConnection connection, params string[] viewList)
        {
            var views = base.GetViews(connection, viewList);
             return views
                .Where(view => !_excludedTables.Contains(view.Name, StringComparer.OrdinalIgnoreCase))
                .ToArray();
        }
    }
}
