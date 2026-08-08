namespace LoanManagementSystem.Application.Interfaces;

public interface IExcelService
{
    byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName);
}