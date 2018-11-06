using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using HearstWebService.Common.Helpers;
using HearstWebService.Data.Models;
using HearstWebService.Interfaces;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HearstWebService.BusinessLogic.Reports
{
    public class ReportLogic: BaseLogic, IReportLogic
    {
        private const string TemplateFilePathSettingKey = "TemplatePath";
        private const string ServiceWebPathSettingKey = "ServiceWebPath";
        private const string TmpFolderPathSettingKey = "TmpFolder";

        protected override string LogPrefix => "[Data export log]";

        private readonly Lazy<IStoredProceduresLogic> _storedProceduresLogic;

        public ReportLogic(Lazy<IStoredProceduresLogic> storedProceduresLogic, 
            Lazy<IDbAccessor> dbAccessor, 
            Lazy<ILogger> logger,
            Lazy<ICache> cache)
            : base(dbAccessor, logger)
        {
            _storedProceduresLogic = storedProceduresLogic;
        }

        public async Task<string> CreateReport(ReportParameters parameters)
        {
            var checkResult = await CheckParameters(parameters);
            if (!checkResult.HasValue)
            {
                LogAndThrow("Failed to get valid parameter values");
            }
            else if (!checkResult.Value)
            {
                LogAndThrow("Invalid parameter value", true);
            }
            else
            {
                var dataTable = await _storedProceduresLogic.Value.GetReportDataTable(parameters);
                if (dataTable == null)
                {
                    LogAndThrow("Failed to get report data");
                }

                var dbSettings = await _dbAccessor.Value.GetSettings();
                var templateFilePath = dbSettings.FirstOrDefault(p => p.Name == TemplateFilePathSettingKey)?.Value;
                if (string.IsNullOrEmpty(templateFilePath))
                {
                    LogAndThrow("Template file path not found in settings");
                }
                var tmpFolderPath = dbSettings.FirstOrDefault(p => p.Name == TmpFolderPathSettingKey)?.Value;
                if (string.IsNullOrEmpty(tmpFolderPath))
                {
                    LogAndThrow("Temp folder path not found in settings");
                }

                var newFilePath = Path.Combine(tmpFolderPath, GetOutputFileName(parameters.Entity));
                File.Copy(templateFilePath, newFilePath, true);

                if (File.Exists(newFilePath))
                {
                    try
                    {
                        ProcessDocument(dataTable, newFilePath, parameters);
                        _logger.Value.Info("Report successfully created", LogPrefix);
                    }
                    catch (Exception ex)
                    {
                        _logger.Value.Error("Failed to process document", ex, LogPrefix);
                        throw;
                    }
                }
                else
                {
                    LogAndThrow($"Failed to copy template {templateFilePath} to new location {newFilePath}");
                }

                return newFilePath;
            }

            return null;
        }

        private async Task<bool?> CheckParameters(ReportParameters parameters)
        {
            try
            {
                return (!parameters.KindVgo.HasValue || ConfigHelper.Instance.KindVgoValidValues.Contains(parameters.KindVgo.Value)) &&
                    (await _dbAccessor.Value.GetDistinctValidReportEntitiesAsync()).Contains(parameters.Entity) &&
                    (await _dbAccessor.Value.GetDistinctValidReportYearsAsync()).Contains(parameters.Year) &&
                    (await _dbAccessor.Value.GetDistinctValidReportScenariosAsync()).Contains(parameters.Scenario);
            }
            catch
            {
                return null;
            }
        }

        private void ProcessDocument(DataTable dataTable, string filePath, ReportParameters parameters)
        {
            using (var document = SpreadsheetDocument.Open(filePath, true))
            {
                var wbPart = document.WorkbookPart;

                document.WorkbookPart.Workbook.CalculationProperties.ForceFullCalculation = true;
                document.WorkbookPart.Workbook.CalculationProperties.FullCalculationOnLoad = true;

                UpdateValue(wbPart, "Control", "B1", parameters.Scenario, 0, true);
                UpdateValue(wbPart, "Control", "B2", parameters.Year, 0, true);
                UpdateValue(wbPart, "Control", "B3", "Working", 0, true);

                string[] shete_list = new string[] { "Balance Sheet", "Inter-Company", "Total P&L", "ELL", "ELT", "ELS", "ELD", "ELG", "MAX", "MAS", "VST", "DEP", "PAR", "PAT", "PSY", "PST", "PSL", "MCL", "MCT", "SHT", "WMN", "TVG", "VBK", "COL", "MER", "OTH", "DSR", "Ecom Template", "ELL.RU_tot", "ELG.RU_tot", "ELD.RU_tot", "MAX.RU_tot", "VST.RU_tot", "PAR.RU_tot", "PSY.RU_tot", "MCL.RU_tot", "WD.RU_tot", "SHT.RU_tot", "WMN.RU_tot", "EUK.RU_tot", "TVGm", "POR.RU", "DNGS", "DNNV", "DPRM", "DHSU", "DSMR", "DSCH", "DKRS", "DRST", "DMZP", "DRGN", "DRL", "DZP", "DAV", "DMM", "Overheads & Unallocated" };
                foreach (string sht in shete_list)
                {
                    UpdateValue(wbPart, sht, "G12", parameters.Entity, 0, true);
                }

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    string sn = Convert.ToString(dataTable.Rows[i][8]);

                    int row = Convert.ToInt32(dataTable.Rows[i][9]);

                    char c;
                    int j;
                    for (j = 10, c = 'M'; j <= 21 & c <= 'X'; j++, c++)
                    {
                        UpdateValue(wbPart, sn, c + Convert.ToString(row), Convert.ToString(dataTable.Rows[i][j], CultureInfo.InvariantCulture), 0, false);
                    }
                }

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }
        }

        public bool UpdateValue(WorkbookPart wbPart, string sheetName, string addressName, string value, UInt32Value styleIndex, bool isString)
        {
            // Assume failure.
            bool updated = false;

            Sheet sheet = wbPart.Workbook.Descendants<Sheet>().Where(
                (s) => s.Name == sheetName).FirstOrDefault();

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(wbPart.GetPartById(sheet.Id))).Worksheet;
                Cell cell = InsertCellInWorksheet(ws, addressName);

                if (isString)
                {
                    // Either retrieve the index of an existing string,
                    // or insert the string into the shared string table
                    // and get the index of the new item.
                    int stringIndex = InsertSharedStringItem(wbPart, value);

                    cell.CellValue = new CellValue(stringIndex.ToString());
                    cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                }
                else
                {
                    cell.CellValue = new CellValue(value);
                    cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                }

                if (styleIndex > 0)
                    cell.StyleIndex = styleIndex;

                // Save the worksheet.
                //ws.Save();
                updated = true;
            }

            return updated;
        }

        private Cell InsertCellInWorksheet(Worksheet ws, string addressName)
        {
            SheetData sheetData = ws.GetFirstChild<SheetData>();
            Cell cell = null;

            UInt32 rowNumber = GetRowIndex(addressName);
            Row row = GetRow(sheetData, rowNumber);

            // If the cell you need already exists, return it.
            // If there is not a cell with the specified column name, insert one.  
            Cell refCell = row.Elements<Cell>().
                Where(c => c.CellReference.Value == addressName).FirstOrDefault();
            if (refCell != null)
            {
                cell = refCell;
            }
            else
            {
                cell = CreateCell(row, addressName);
            }
            return cell;
        }

        // Add a cell with the specified address to a row.
        private Cell CreateCell(Row row, String address)
        {
            Cell cellResult;
            Cell refCell = null;

            // Cells must be in sequential order according to CellReference. 
            // Determine where to insert the new cell.
            foreach (Cell cell in row.Elements<Cell>())
            {
                if (string.Compare(cell.CellReference.Value, address, true) > 0)
                {
                    refCell = cell;
                    break;
                }
            }

            cellResult = new Cell();
            cellResult.CellReference = address;

            row.InsertBefore(cellResult, refCell);
            return cellResult;
        }

        // Return the row at the specified rowIndex located within
        // the sheet data passed in via wsData. If the row does not
        // exist, create it.
        private Row GetRow(SheetData wsData, UInt32 rowIndex)
        {
            var row = wsData.Elements<Row>().
                Where(r => r.RowIndex.Value == rowIndex).FirstOrDefault();
            if (row == null)
            {
                row = new Row();
                row.RowIndex = rowIndex;
                wsData.Append(row);
            }
            return row;
        }

        // Given an Excel address such as E5 or AB128, GetRowIndex
        // parses the address and returns the row index.
        private UInt32 GetRowIndex(string address)
        {
            string rowPart;
            UInt32 l;
            UInt32 result = 0;

            for (int i = 0; i < address.Length; i++)
            {
                if (UInt32.TryParse(address.Substring(i, 1), out l))
                {
                    rowPart = address.Substring(i, address.Length - i);
                    if (UInt32.TryParse(rowPart, out l))
                    {
                        result = l;
                        break;
                    }
                }
            }
            return result;
        }

        private int InsertSharedStringItem(WorkbookPart wbPart, string value)
        {
            int index = 0;
            bool found = false;
            var stringTablePart = wbPart
                .GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

            // If the shared string table is missing, something's wrong.
            // Just return the index that you found in the cell.
            // Otherwise, look up the correct text in the table.
            if (stringTablePart == null)
            {
                // Create it.
                stringTablePart = wbPart.AddNewPart<SharedStringTablePart>();
            }

            var stringTable = stringTablePart.SharedStringTable;
            if (stringTable == null)
            {
                stringTable = new SharedStringTable();
            }

            // Iterate through all the items in the SharedStringTable. 
            // If the text already exists, return its index.
            foreach (SharedStringItem item in stringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == value)
                {
                    found = true;
                    break;
                }
                index += 1;
            }

            if (!found)
            {
                stringTable.AppendChild(new SharedStringItem(new Text(value)));
                stringTable.Save();
            }

            return index;
        }

        private string GetOutputFileName(string entity)
        {
            return $"Template_V4_{entity}_{DateTime.Now.ToString("yyyy.MM.dd_HH.mm.ss")}.xlsx";
        }
    }
}
