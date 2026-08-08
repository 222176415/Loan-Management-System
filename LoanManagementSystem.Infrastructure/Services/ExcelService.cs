using ClosedXML.Excel;
using System.Text.RegularExpressions;
using LoanManagementSystem.Application.Interfaces;
namespace LoanManagementSystem.Infrastructure.Services;

public class ExcelService : IExcelService
{
    public byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);
        var table = worksheet.Cell(1, 1).InsertTable(data);

        // 2. Format the Headers (Row 1)
        var headerRow = worksheet.Row(1);
        
        // Pretty formatting for Header Row
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Font.FontSize = 12;
        headerRow.Style.Font.FontColor = XLColor.White;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#2c3e50"); // Professional Dark Blue
        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRow.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        headerRow.Height = 25;

   
        foreach (var cell in headerRow.CellsUsed())
        {
     
            string prettyHeader = Regex.Replace(cell.Value.ToString(), "([a-z])([A-Z])", "$1 $2");
            cell.Value = prettyHeader;
        }
        
        var dataRows = worksheet.Rows(2, worksheet.LastRowUsed().RowNumber());
        foreach (var row in dataRows)
        {
            if (row.RowNumber() % 2 == 0)
                row.Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fa");
        }


        var range = worksheet.RangeUsed();
        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        range.Style.Border.OutsideBorderColor = XLColor.LightGray;
        range.Style.Border.InsideBorderColor = XLColor.LightGray;


        worksheet.Columns().AdjustToContents(); 
        worksheet.SheetView.FreezeRows(1);      

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
