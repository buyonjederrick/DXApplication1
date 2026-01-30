using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.AspNetCore;
using DevExpress.DashboardAspNetCore;
using DevExpress.DashboardCommon;
using DevExpress.DashboardWeb;
using DevExpress.DataAccess.Excel;
using DevExpress.DataAccess.Sql;
using DXApplication1;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
AppDomain.CurrentDomain.SetData("DataDirectory", builder.Environment.ContentRootPath);
builder.Services
               .AddResponseCompression()
               .AddDevExpressControls()
               .AddControllers();

builder.Services.AddScoped((IServiceProvider serviceProvider) =>
{
    DashboardConfigurator configurator = new DashboardConfigurator();
    configurator.SetConnectionStringsProvider(new DashboardConnectionStringsProvider(builder.Configuration));

    DashboardFileStorage dashboardFileStorage = new DashboardFileStorage(builder.Environment.ContentRootFileProvider.GetFileInfo("Data/Dashboards").PhysicalPath);
    configurator.SetDashboardStorage(dashboardFileStorage);

    DataSourceInMemoryStorage dataSourceStorage = new DataSourceInMemoryStorage();

    // Registers an SQL data source.
    DashboardSqlDataSource sqlDataSource = new DashboardSqlDataSource("SQL Data Source", "NWindConnectionString");
    sqlDataSource.DataProcessingMode = DataProcessingMode.Client;
    SelectQuery query = SelectQueryFluentBuilder
        .AddTable("Categories").SelectAllColumnsFromTable()
        .Join("Products", "CategoryID").SelectAllColumnsFromTable()
        .Build("Products_Categories");
    sqlDataSource.Queries.Add(query);
    dataSourceStorage.RegisterDataSource("sqlDataSource", sqlDataSource.SaveToXml());

    // Registers an Object data source.
    DashboardObjectDataSource objDataSource = new DashboardObjectDataSource("Object Data Source");
    dataSourceStorage.RegisterDataSource("objDataSource", objDataSource.SaveToXml());

    // Registers an Excel data source.
    DashboardExcelDataSource excelDataSource = new DashboardExcelDataSource("Excel Data Source");
    excelDataSource.FileName = builder.Environment.ContentRootFileProvider.GetFileInfo("Data/Sales.xlsx").PhysicalPath;
    excelDataSource.SourceOptions = new ExcelSourceOptions(new ExcelWorksheetSettings("Sheet1"));
    dataSourceStorage.RegisterDataSource("excelDataSource", excelDataSource.SaveToXml());

    configurator.SetDataSourceStorage(dataSourceStorage);

    configurator.DataLoading += (s, e) =>
    {
        if (e.DataSourceName == "Object Data Source")
        {
            e.Data = Invoices.CreateData();
        }
    };
    return configurator;
});

builder.Services.AddRazorPages();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseDevExpressControls();
app.UseRouting();
app.MapDashboardRoute("dashboardControl", "DefaultDashboard");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();