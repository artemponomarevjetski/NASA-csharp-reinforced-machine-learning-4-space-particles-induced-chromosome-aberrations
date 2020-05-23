using System;
using System.Drawing;
using System.Windows.Forms;

namespace GraficDisplay
{
    public partial class MainForm : Form
    {
        public partial class Radiation
        {
            public void OutputToExcel(RadiationShield[] rShields)
            {
                // output into an Excel spreadsheet
                Microsoft.Office.Interop.Excel.Application oXL;
                Microsoft.Office.Interop.Excel._Workbook oWB;
                Microsoft.Office.Interop.Excel._Worksheet oSheet;
                Microsoft.Office.Interop.Excel.Range oRng;
                //Start Excel and get Application object
                oXL = new Microsoft.Office.Interop.Excel.Application();
                oXL.ErrorCheckingOptions.BackgroundChecking = false; // This line disables error checking
                oXL.Visible = false; // true;
                oXL.UserControl = true;
                oXL.DisplayAlerts = false;

                // Get a new workbook
                oWB = oXL.Workbooks.Add("");
                oSheet = (Microsoft.Office.Interop.Excel._Worksheet)oWB.ActiveSheet;
                //      object misvalue = System.Reflection.Missing.Value;
                oWB.RefreshAll();
                string appPath = Application.StartupPath;
                string wbPath = appPath + "\\temp\\RadShield.xls";
                int nRowsMax = 2000;
                try
                {
                    for (int j = 0; j < nShields; j++)
                    {
                        int k = 0; // row number   

                        //Format A1:Z1 as bold, vertical alignment = center
                        oSheet.get_Range("A1", "Z1").Font.Bold = true;
                        oSheet.get_Range("A1", "Z1").VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                        oSheet.get_Range("A1", "Z1").EntireColumn.AutoFit();
                        oSheet.get_Range("A" + 2.ToString(), "Z" + 2.ToString()).Font.Size = 14;
                        //            
                        string[,] saNames = new string[nRowsMax, 17];
                        oSheet.get_Range("M1", "N1").EntireColumn.AutoFit();
                        oSheet.get_Range("L" + 1.ToString(), "M" + 1.ToString()).Font.Color = Color.DarkRed;
                        oSheet.Name = "Shield#" + j.ToString();
                        if (j != 0)
                        {
                            oSheet = (Microsoft.Office.Interop.Excel.Worksheet)oWB.Worksheets.Add
                                (System.Reflection.Missing.Value,
                                oWB.Worksheets[oWB.Worksheets.Count],
                                System.Reflection.Missing.Value,
                                System.Reflection.Missing.Value);
                        }

                        // Add table headers going cell by cell
                        k++;
                        oSheet.Cells[k, 1 + 3] = "Shield #";
                        oSheet.Cells[k, 2 + 3] = "Shield thickness, mm";
                        oSheet.Cells[k, 3 + 3] = "Shield density, g/cm^2";
                        oSheet.Cells[k, 4 + 3] = "Shield Efficiency";
                        oSheet.Cells[k, 5 + 3] = "Isomer Pathway ON/OFF";

                        //Format A1:Z1 as bold, vertical alignment = center
                        oSheet.get_Range("A1", "Z1").Font.Bold = true;
                        oSheet.get_Range("A1", "Z1").VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                        oSheet.get_Range("A1", "Z1").EntireColumn.AutoFit();
                        oSheet.get_Range("A" + 2.ToString(), "Z" + 2.ToString()).Font.Size = 14;
                        oSheet.get_Range("M1", "N1").EntireColumn.AutoFit();
                        oSheet.get_Range("L" + 1.ToString(), "M" + 1.ToString()).Font.Color = Color.DarkRed;

                        Array.Clear(saNames, 0, saNames.Length);
                        k++;
                        saNames[k - 1, 0 + 3] = (j + 1).ToString();
                        saNames[k - 1, 1 + 3] = MetaData.StructuralMetaData.ShieldThickness.ToString("F3");
                        saNames[k - 1, 2 + 3] = rShields[j].md.shieldStructure.density.ToString("F3");
                        saNames[k - 1, 3 + 3] = rShields[j].md.results.ShieldEfficiency.ToString("F3");
                        saNames[k - 1, 4 + 3] = "Shield #" + (j + 1).ToString()
                            + "\nShield Description:  \tdensity = " + rShields[j].md.shieldStructure.density.ToString("F3") + "\t\t g/cm^2"
                            + "\nconcentration of Hf = " + rShields[j].md.shieldStructure.concentrationOfHafnium.ToString("F3")
                            + "\nnumber of layers = " + rShields[j].md.shieldStructure.nlayers.ToString()
                            + "\nshield type = " + rShields[j].md.shieldStructure.sh_t.ToString()
                            + "\nmeanFreePath = " + rShields[j].md.physicalMetaData.meanFreePath.ToString("F3")
                            + "\nsensitivityFactor = " + rShields[j].md.physicalMetaData.sensitivityFactor.ToString("F3");
                        //
                        k = 2;
                        k++;
                        saNames[k - 1, 0] = "g/cm^2";
                        saNames[k - 1, 1] = "D/D0";
                        for (int l = 0; l < rShields[j].AttenuationOfEnergy_at_depth.Length; l++)
                        {
                            k++;
                            saNames[k - 1, 0] = rShields[j].Depth[l].ToString("F3");
                            saNames[k - 1, 1] = rShields[j].AttenuationOfEnergy_at_depth[l].ToString("F3");
                        }
                        //
                        oSheet.get_Range("A2", "N" + (nRowsMax + 1).ToString()).Value2 = saNames;
                        oSheet.get_Range("A" + (k + 2).ToString(), "Z" + (k + 2).ToString()).Font.Size = 14;
                        oSheet.get_Range("A" + (k + 2).ToString(), "Z" + (k + 2).ToString()).ClearContents();
                        oSheet.Application.ScreenUpdating = false;
                        //      
                        //AutoFit columns A:Z
                        oRng = oSheet.get_Range("A1", "Z1");
                        oRng.EntireColumn.AutoFit();
                    }
                    oWB.SaveAs(wbPath, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal,
                        Type.Missing, Type.Missing, false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                    oWB.Close(false, wbPath, Type.Missing);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oWB);
                    oXL.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oXL); // creates a workbook that leaves behind a process after opening it 
                }
                catch
                {
                    oWB.SaveAs(wbPath, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal,
                        Type.Missing, Type.Missing, false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                    oWB.Close(false, wbPath, Type.Missing);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oWB);
                    oXL.Quit();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oXL); // creates a workbook that leaves behind a process after opening it  }
                }
            }
        }
    }
}
