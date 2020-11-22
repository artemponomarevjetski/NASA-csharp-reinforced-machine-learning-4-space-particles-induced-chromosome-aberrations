using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace GraficDisplay
{
    public partial class MainForm : Form
    {
        TextBox textBox2;
        Button buttonOK2;
        private void CompileSCDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form Form2 = new Form
            {
                Text = "Compile S.C. data; short report",
                Width = 500
            };
            //
            textBox2 = new TextBox
            {
                Text = @"C:\Users\aponomar\Desktop\combndSC", // short report
                Width = 400,
                Height = 50,
                Multiline = true,
                BackColor = Color.Blue,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };
            Form2.Controls.Add(textBox2);
            //
            buttonOK2 = new Button
            {
                Location = new Point(50, 100),
                Text = "Do"
            };
            buttonOK2.Click += new EventHandler(Button_Click2);
            Form2.Controls.Add(buttonOK2);
            //
            Button buttonCancel2 = new Button
            {
                Text = "Cancel",
                Location = new Point(buttonOK2.Left, buttonOK2.Height + buttonOK2.Top + 10)
            };
            Form2.CancelButton = buttonCancel2;
            Form2.Controls.Add(buttonCancel2);
            buttonCancel2.Click += new EventHandler(Cancel_Click2);
            //
            Form2.Show();
        }

        private void Cancel_Click2(object sender, EventArgs e)
        {
            Close();
        }

        private void Button_Click2(object sender, EventArgs e)
        {
            var text = textBox2.Text;
            string[] fileEntries = Directory.GetFiles(text);
            int nRowsMax = 2000; //  1048576;       
            string ionNameOld = "";
            double energyOld = 0.0;
            double doseOld = 0.0;
            string orientationOld = "";
            int maxNHist = 0;
            double nDSBaver = 0.0;
            //
            string appPath = Application.StartupPath;
            string wbPath = appPath + "\\temp\\SC.xls";
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
            //
            // Get a new workbook
            oWB = oXL.Workbooks.Add("");
            oSheet = (Microsoft.Office.Interop.Excel._Worksheet)oWB.ActiveSheet;
            //      object misvalue = System.Reflection.Missing.Value;
            oWB.RefreshAll();

            // Add table headers going cell by cell
            oSheet.Cells[1, 1] = "# MC hist's";
            oSheet.Cells[1, 2] = " #<DSB>";
            oSheet.Cells[1, 3] = " sample #";
            oSheet.Cells[1, 4] = "Ion";
            oSheet.Cells[1, 5] = "Z";
            oSheet.Cells[1, 6] = "E";
            oSheet.Cells[1, 7] = "dose, Gy";
            oSheet.Cells[1, 8] = "orientation";
            oSheet.Cells[1, 9] = "completion time";
            oSheet.Cells[1, 10] = "Algo type";
            oSheet.Cells[1, 11] = "File Name";

            //Format A1:Z1 as bold, vertical alignment = center
            oSheet.get_Range("A1", "Z1").Font.Bold = true;
            oSheet.get_Range("A1", "Z1").VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
            oSheet.get_Range("A1", "Z1").EntireColumn.AutoFit();
            oSheet.get_Range("A" + 2.ToString(), "Z" + 2.ToString()).Font.Size = 14;
            //         
            string[,] saNames = new string[nRowsMax, 17];
            //
            int j = 0; // file number
            int k = 1;
            List<int> sampleList = new List<int>();
            while (true)
            {
                try
                {
                    // get model data from the output files
                    if (!fileEntries[j].Contains("nDSBsOnly"))
                    {
                        long itemp1 = (new FileInfo(fileEntries[j]).Length);
                        if (itemp1 > 0)
                        {
                            int nDSB = 0, nline = 0, lineCount = 0, fileNumber = 0, nTracks = 0;
                            string[] lines;
                            try
                            {
                                nDSB = 0;
                                nline = 0;
                                lineCount = File.ReadAllLines(fileEntries[j]).Length;
                                lines = File.ReadAllLines(fileEntries[j]); // a new output file for each line                           
                                fileNumber = Convert.ToInt32(lines[nline++]); // nuc number from file contents 
                                nTracks = Convert.ToInt32(lines[nline++]);
                                int[] x = new int[nTracks];
                                int[] y = new int[nTracks];
                                int[] z = new int[nTracks];
                                for (int l = nline; l < nTracks + nline; l++)
                                {
                                    var line = lines[l];
                                    var lineData = line.Split('\t');
                                    x[l - nline] = Convert.ToInt32(lineData[1]);
                                    y[l - nline] = Convert.ToInt32(lineData[2]);
                                }
                                //
                                nline += nTracks;
                                nDSB = lineCount - (nTracks + 2);
                                nDSB /= 2;
                                nDSBaver += nDSB;
                                // parse file name 
                                char[] delimiterChars = { '_', '.' };
                                string[] inputData = Path.GetFileName(fileEntries[j]).Split(delimiterChars);
                                string ion = inputData[0];
                                double dose = Convert.ToDouble(inputData[1]) + 0.1 * Convert.ToDouble(inputData[2].Substring(0, inputData[2].IndexOf("Gy")));
                                double energy = Convert.ToDouble(inputData[2].Substring(0, inputData[2].IndexOf("Amorph")).Split('E').Last());
                                string orientation = (inputData[5] == "Parallel" ? "Parallel" : "Perpendicular");
                                if (!(ion == ionNameOld && energy == energyOld && dose == doseOld && orientation == orientationOld) || j == 0)
                                {
                                    int itemp = SampleNumber(ion, energy, dose, orientation);
                                    sampleList.Add(itemp);
                                    if (k >= 2)
                                    {
                                        saNames[k - 1, 0] = maxNHist.ToString();
                                        if (Double.IsNaN(nDSBaver) || Double.IsPositiveInfinity(nDSBaver) || Double.IsNegativeInfinity(nDSBaver))
                                        {
                                            saNames[k - 1, 1] = "nDSBaver is nan";
                                        }
                                        else
                                        {
                                            nDSBaver /= (Convert.ToDouble(maxNHist) > 0 ? Convert.ToDouble(maxNHist) : 1.0);
                                            if (nDSBaver == 0)
                                                saNames[k - 1, 12] = "All nDSBs are 0: " + nDSBaver.ToString() + fileEntries[j];
                                            else
                                                saNames[k - 1, 1] = RoundToSignificantDigits(nDSBaver, 5).ToString();
                                        }
                                        if (maxNHist < 1000)
                                        {
                                            oRng = oSheet.get_Range("A" + (k - 1 + 2).ToString(), "Z" + (k - 1 + 2).ToString());
                                            oRng.Font.Color = ColorTranslator.ToOle(Color.Red);
                                        }
                                    }
                                    //
                                    ionNameOld = ion;
                                    energyOld = energy;
                                    doseOld = dose;
                                    orientationOld = orientation;
                                    maxNHist = 0;
                                    nDSBaver = 0;
                                    //      saNames[k, 1] = (inputData[5] == "Parallel" ? inputData[6] : inputData[5]); 
                                    saNames[k, 2] = itemp.ToString();
                                    saNames[k, 3] = ion;
                                    int charge = Ioncharge(ion);
                                    saNames[k, 4] = (charge == 0 ? "" : charge.ToString()).ToString();
                                    saNames[k, 5] = energy.ToString();
                                    saNames[k, 6] = dose.ToString();
                                    saNames[k, 7] = orientation;
                                    saNames[k, 8] = File.GetCreationTime(fileEntries[j]).ToString();
                                    saNames[k, 9] = "Amorphous Tracks";
                                    saNames[k, 10] = fileEntries[j];
                                    //
                                    oSheet.get_Range("A2", "N" + (nRowsMax + 1).ToString()).Value2 = saNames;
                                    oSheet.get_Range("A" + (j + 2).ToString(), "Z" + (j + 2).ToString()).Font.Size = 14;
                                    oSheet.get_Range("A" + (j + 2).ToString(), "Z" + (j + 2).ToString()).ClearContents();
                                    oSheet.Application.ScreenUpdating = false;
                                    //      
                                    //AutoFit columns A:Z
                                    oRng = oSheet.get_Range("A1", "Z1");
                                    oRng.EntireColumn.AutoFit();
                                    //                       
                                    oWB.SaveAs(wbPath, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal,
                                        Type.Missing, Type.Missing, false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                                    //
                                    k++;
                                }
                                maxNHist++;
                            }
                            catch
                            {
                                saNames[k, 12] = "Data did not load properly:    \tnDSB = " + nDSBaver.ToString() + "          \t" + fileEntries[j].ToString();
                            }
                        }
                        else
                            saNames[k, 12] = "Empty file: " + fileEntries[j];
                    }
                    j++; // file number                                             
                    if (j > fileEntries.Length - 1)
                    {
                        saNames[k - 1, 0] = maxNHist.ToString();
                        nDSBaver /= (Convert.ToDouble(maxNHist) > 0 ? Convert.ToDouble(maxNHist) : 1.0);
                        saNames[k - 1, 1] = nDSBaver.ToString("#.###");
                        // list comparison
                        var missingSampleList = DefaultSampleList().Except(sampleList).ToList();
                        saNames[k + 2, 0] = "Missing Samples: ";
                        for (int l = 0; l < missingSampleList.Count; l++)
                        {
                            saNames[l + k + 3, 1] = missingSampleList[l].ToString();
                            oRng = oSheet.get_Range("A" + ((l + k + 3) + 2).ToString(), "Z" + ((l + k + 3) + 2).ToString());
                            oRng.Font.Color = ColorTranslator.ToOle(Color.Red);
                        }
                        oSheet.get_Range("A2", "N" + (nRowsMax + 1).ToString()).Value2 = saNames;
                        oSheet.get_Range("A" + (j + 2).ToString(), "Z" + (j + 2).ToString()).Font.Size = 14;
                        oSheet.get_Range("A" + (j + 2).ToString(), "Z" + (j + 2).ToString()).ClearContents();
                        oSheet.Application.ScreenUpdating = false;
                        //AutoFit columns A:Z
                        oRng = oSheet.get_Range("A1", "Z1");
                        oRng.EntireColumn.AutoFit();
                        //
                        oWB.SaveAs(wbPath, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal,
                            Type.Missing, Type.Missing, false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                        oWB.Close(false, wbPath, Type.Missing);
                        oXL.Quit();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oWB);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oXL);
                        GC.Collect();
                        Close();
                        break;
                    }
                }
                catch
                {
                    saNames[k++, 12] = "Processing faliure: " + fileEntries[j];
                    oWB.SaveAs(wbPath, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal,
                                    Type.Missing, Type.Missing, false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                }
            }
            Close();
        }

        static double RoundToSignificantDigits(double d, int digits)
        {
            if (d == 0)
                return 0;
            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
            return scale * Math.Round(d / scale, digits);
        }

        public List<int> DefaultSampleList()
        {
            List<int> l = new List<int>();
            int nDataPointsMax = 288;  // *2=576 
            for (int nonce = 0; nonce < nDataPointsMax; nonce++)
            {
                l.Add(2 * (nonce + 1) - 1);
                l.Add(2 * (nonce + 1));
            }
            return l;
        }

        public int SampleNumber(string ion, double m_dEnergy1, double dose, string orientation) // returns -1 when sample # is unknown
        {
            int nDataPointsMax = 2 * 288;
            for (int nonce = 1; nonce < nDataPointsMax + 1; nonce++)
            {
                double m_dMass = -1.0, m_dCharge = -1.0, m_dEnergy = -1.0;
                int idx = -1;
                switch ((nonce + 1) / 2)
                {
                    case 1: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 10.0; idx = 1; } break;  // C ions
                    case 2: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 10.0; idx = 2; } break;  // C ions
                    case 3: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 10.0; idx = 3; } break;  // C ions
                    case 4: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 10.0; idx = 4; } break;  // C ions
                    //
                    case 5: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 50.0; idx = 1; } break;  // C ions
                    case 6: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 50.0; idx = 2; } break;  // C ions
                    case 7: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 50.0; idx = 3; } break;  // C ions
                    case 8: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 50.0; idx = 4; } break;  // C ions
                    //
                    case 9: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 100.0; idx = 1; } break;  // C ions
                    case 10: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 100.0; idx = 2; } break;  // C ions
                    case 11: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 100.0; idx = 3; } break;  // C ions
                    case 12: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 100.0; idx = 4; } break;  // C ions
                    //
                    case 13: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 200.0; idx = 1; } break;  // C ions
                    case 14: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 200.0; idx = 2; } break;  // C ions
                    case 15: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 200.0; idx = 3; } break;  // C ions
                    case 16: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 200.0; idx = 4; } break;  // C ions
                    //
                    case 17: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 400.0; idx = 1; } break;  // C ions
                    case 18: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 400.0; idx = 2; } break;  // C ions
                    case 19: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 400.0; idx = 3; } break;  // C ions
                    case 20: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 400.0; idx = 4; } break;  // C ions
                    //
                    case 21: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 800.0; idx = 1; } break;  // C ions
                    case 22: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 800.0; idx = 2; } break;  // C ions
                    case 23: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 800.0; idx = 3; } break;  // C ions
                    case 24: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 800.0; idx = 4; } break;  // C ions
                    //
                    case 25: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 1000.0; idx = 1; } break;  // C ions
                    case 26: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 1000.0; idx = 2; } break;  // C ions
                    case 27: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 1000.0; idx = 3; } break;  // C ions
                    case 28: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 1000.0; idx = 4; } break;  // C ions
                    //
                    case 29: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 1600.0; idx = 1; } break;  // C ions
                    case 30: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 1600.0; idx = 2; } break;  // C ions
                    case 31: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 1600.0; idx = 3; } break;  // C ions
                    case 32: { m_dMass = 12.0; m_dCharge = 6.0; m_dEnergy = 1600.0; idx = 4; } break;  // C ions
                    //
                    case 33: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 10.0; idx = 1; } break;  // O ions
                    case 34: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 10.0; idx = 2; } break;  // O ions
                    case 35: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 10.0; idx = 3; } break;  // O ions
                    case 36: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 10.0; idx = 4; } break;  // O ions
                    //
                    case 37: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 50.0; idx = 1; } break;  // O ions
                    case 38: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 50.0; idx = 2; } break;  // O ions
                    case 39: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 50.0; idx = 3; } break;  // O ions
                    case 40: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 50.0; idx = 4; } break;  // O ions
                    //
                    case 41: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 100.0; idx = 1; } break;  // O ions
                    case 42: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 100.0; idx = 2; } break;  // O ions
                    case 43: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 100.0; idx = 3; } break;  // O ions
                    case 44: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 100.0; idx = 4; } break;  // O ions
                    //
                    case 45: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 200.0; idx = 1; } break;  // O ions
                    case 46: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 200.0; idx = 2; } break;  // O ions
                    case 47: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 200.0; idx = 3; } break;  // O ions
                    case 48: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 200.0; idx = 4; } break;  // O ions
                    //
                    case 49: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 400.0; idx = 1; } break;  // O ions
                    case 50: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 400.0; idx = 2; } break;  // O ions
                    case 51: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 400.0; idx = 3; } break;  // O ions
                    case 52: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 400.0; idx = 4; } break;  // O ions
                    //
                    case 53: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 800.0; idx = 1; } break;  // O ions
                    case 54: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 800.0; idx = 2; } break;  // O ions
                    case 55: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 800.0; idx = 3; } break;  // O ions
                    case 56: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 800.0; idx = 4; } break;  // O ions
                    //
                    case 57: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 1000.0; idx = 1; } break;  // O ions
                    case 58: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 1000.0; idx = 2; } break;  // O ions
                    case 59: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 1000.0; idx = 3; } break;  // O ions
                    case 60: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 1000.0; idx = 4; } break;  // O ions
                    //
                    case 61: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 1600.0; idx = 1; } break;  // O ions
                    case 62: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 1600.0; idx = 2; } break;  // O ions
                    case 63: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 1600.0; idx = 3; } break;  // O ions
                    case 64: { m_dMass = 16.0; m_dCharge = 8.0; m_dEnergy = 1600.0; idx = 4; } break;  // O ions
                    // 
                    case 65: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 10.0; idx = 1; } break;  // Ne ions
                    case 66: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 10.0; idx = 2; } break;  // Ne ions
                    case 67: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 10.0; idx = 3; } break;  // Ne ions
                    case 68: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 10.0; idx = 4; } break;  // Ne ions
                    //
                    case 69: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 50.0; idx = 1; } break;  // Ne ions 
                    case 70: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 50.0; idx = 2; } break;  // Ne ions
                    case 71: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 50.0; idx = 3; } break;  // Ne ions
                    case 72: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 50.0; idx = 4; } break;  // Ne ions
                    //
                    case 73: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 100.0; idx = 1; } break;  // Ne ions
                    case 74: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 100.0; idx = 2; } break;  // Ne ions
                    case 75: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 100.0; idx = 3; } break;  // Ne ions
                    case 76: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 100.0; idx = 4; } break;  // Ne ions
                    //
                    case 77: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 200.0; idx = 1; } break;  // Ne ions
                    case 78: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 200.0; idx = 2; } break;  // Ne ions
                    case 79: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 200.0; idx = 3; } break;  // Ne ions
                    case 80: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 200.0; idx = 4; } break;  // Ne ions
                    //
                    case 81: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 400.0; idx = 1; } break;  // Ne ions
                    case 82: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 400.0; idx = 2; } break;  // Ne ions
                    case 83: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 400.0; idx = 3; } break;  // Ne ions
                    case 84: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 400.0; idx = 4; } break;  // Ne ions
                    //
                    case 85: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 800.0; idx = 1; } break;  // Ne ions
                    case 86: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 800.0; idx = 2; } break;  // Ne ions
                    case 87: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 800.0; idx = 3; } break;  // Ne ions
                    case 88: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 800.0; idx = 4; } break;  // Ne ions
                    //
                    case 89: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 1000.0; idx = 1; } break;  // Ne ions
                    case 90: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 1000.0; idx = 2; } break;  // Ne ions
                    case 91: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 1000.0; idx = 3; } break;  // Ne ions
                    case 92: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 1000.0; idx = 4; } break;  // Ne ions
                    //
                    case 93: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 1600.0; idx = 1; } break;  // Ne ions
                    case 94: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 1600.0; idx = 2; } break;  // Ne ions
                    case 95: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 1600.0; idx = 3; } break;  // Ne ions
                    case 96: { m_dMass = 20.0; m_dCharge = 10.0; m_dEnergy = 1600.0; idx = 4; } break;  // Ne ions
                    //
                    case 97: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 10.0; idx = 1; } break;  // Si ions
                    case 98: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 10.0; idx = 2; } break;  // Si ions
                    case 99: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 10.0; idx = 3; } break;  // Si ions
                    case 100: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 10.0; idx = 4; } break;  // Si ions
                    //
                    case 101: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 50.0; idx = 1; } break;  // Si ions
                    case 102: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 50.0; idx = 2; } break;  // Si ions
                    case 103: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 50.0; idx = 3; } break;  // Si ions
                    case 104: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 50.0; idx = 4; } break;  // Si ions
                    //
                    case 105: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 100.0; idx = 1; } break;  // Si ions
                    case 106: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 100.0; idx = 2; } break;  // Si ions
                    case 107: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 100.0; idx = 3; } break;  // Si ions
                    case 108: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 100.0; idx = 4; } break;  // Si ions
                    //
                    case 109: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 200.0; idx = 1; } break;  // Si ions
                    case 110: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 200.0; idx = 2; } break;  // Si ions
                    case 111: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 200.0; idx = 3; } break;  // Si ions
                    case 112: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 200.0; idx = 4; } break;  // Si ions
                    //
                    case 113: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 400.0; idx = 1; } break;  // Si ions
                    case 114: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 400.0; idx = 2; } break;  // Si ions
                    case 115: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 400.0; idx = 3; } break;  // Si ions
                    case 116: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 400.0; idx = 4; } break;  // Si ions
                    //
                    case 117: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 800.0; idx = 1; } break;  // Si ions
                    case 118: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 800.0; idx = 2; } break;  // Si ions
                    case 119: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 800.0; idx = 3; } break;  // Si ions
                    case 120: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 800.0; idx = 4; } break;  // Si ions
                    //
                    case 121: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 1000.0; idx = 1; } break;  // Si ions
                    case 122: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 1000.0; idx = 2; } break;  // Si ions
                    case 123: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 1000.0; idx = 3; } break;  // Si ions
                    case 124: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 1000.0; idx = 4; } break;  // Si ions
                    //
                    case 125: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 1600.0; idx = 1; } break;  // Si ions
                    case 126: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 1600.0; idx = 2; } break;  // Si ions
                    case 127: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 1600.0; idx = 3; } break;  // Si ions
                    case 128: { m_dMass = 28.0; m_dCharge = 14.0; m_dEnergy = 1600.0; idx = 4; } break;  // Si ions
                    //
                    case 129: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 10.0; idx = 1; } break;  // Ar ions
                    case 130: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 10.0; idx = 2; } break;  // Ar ions
                    case 131: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 10.0; idx = 3; } break;  // Ar ions
                    case 132: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 10.0; idx = 4; } break;  // Ar ions
                    //
                    case 133: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 50.0; idx = 1; } break;  // Ar ions
                    case 134: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 50.0; idx = 2; } break;  // Ar ions
                    case 135: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 50.0; idx = 3; } break;  // Ar ions
                    case 136: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 50.0; idx = 4; } break;  // Ar ions
                    //
                    case 137: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 100.0; idx = 1; } break;  // Ar ions
                    case 138: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 100.0; idx = 2; } break;  // Ar ions
                    case 139: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 100.0; idx = 3; } break;  // Ar ions
                    case 140: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 100.0; idx = 4; } break;  // Ar ions
                    //
                    case 141: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 200.0; idx = 1; } break;  // Ar ions
                    case 142: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 200.0; idx = 2; } break;  // Ar ions
                    case 143: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 200.0; idx = 3; } break;  // Ar ions
                    case 144: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 200.0; idx = 4; } break;  // Ar ions
                    //
                    case 145: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 400.0; idx = 1; } break;  // Ar ions
                    case 146: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 400.0; idx = 2; } break;  // Ar ions
                    case 147: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 400.0; idx = 3; } break;  // Ar ions
                    case 148: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 400.0; idx = 4; } break;  // Ar ions
                    //
                    case 149: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 800.0; idx = 1; } break;  // Ar ions
                    case 150: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 800.0; idx = 2; } break;  // Ar ions
                    case 151: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 800.0; idx = 3; } break;  // Ar ions
                    case 152: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 800.0; idx = 4; } break;  // Ar ions
                    //
                    case 153: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 1000.0; idx = 1; } break;  // Ar ions
                    case 154: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 1000.0; idx = 2; } break;  // Ar ions
                    case 155: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 1000.0; idx = 3; } break;  // Ar ions
                    case 156: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 1000.0; idx = 4; } break;  // Ar ions
                    //
                    case 157: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 1600.0; idx = 1; } break;  // Ar ions
                    case 158: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 1600.0; idx = 2; } break;  // Ar ions
                    case 159: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 1600.0; idx = 3; } break;  // Ar ions
                    case 160: { m_dMass = 40.0; m_dCharge = 18.0; m_dEnergy = 1600.0; idx = 4; } break;  // Ar ions
                    //
                    case 161: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 10.0; idx = 1; } break;  // Ti ions
                    case 162: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 10.0; idx = 2; } break;  // Ti ions
                    case 163: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 10.0; idx = 3; } break;  // Ti ions
                    case 164: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 10.0; idx = 4; } break;  // Ti ions
                    //
                    case 165: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 50.0; idx = 1; } break;  // Ti ions
                    case 166: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 50.0; idx = 2; } break;  // Ti ions
                    case 167: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 50.0; idx = 3; } break;  // Ti ions
                    case 168: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 50.0; idx = 4; } break;  // Ti ions
                    //
                    case 169: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 100.0; idx = 1; } break;  // Ti ions
                    case 170: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 100.0; idx = 2; } break;  // Ti ions
                    case 171: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 100.0; idx = 3; } break;  // Ti ions
                    case 172: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 100.0; idx = 4; } break;  // Ti ions
                    //
                    case 173: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 200.0; idx = 1; } break;  // Ti ions
                    case 174: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 200.0; idx = 2; } break;  // Ti ions
                    case 175: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 200.0; idx = 3; } break;  // Ti ions
                    case 176: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 200.0; idx = 4; } break;  // Ti ions
                    //
                    case 177: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 400.0; idx = 1; } break;  // Ti ions
                    case 178: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 400.0; idx = 2; } break;  // Ti ions
                    case 179: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 400.0; idx = 3; } break;  // Ti ions
                    case 180: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 400.0; idx = 4; } break;  // Ti ions
                    //
                    case 181: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 800.0; idx = 1; } break;  // Ti ions
                    case 182: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 800.0; idx = 2; } break;  // Ti ions
                    case 183: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 800.0; idx = 3; } break;  // Ti ions
                    case 184: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 800.0; idx = 4; } break;  // Ti ions
                    //
                    case 185: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 1000.0; idx = 1; } break;  // Ti ions
                    case 186: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 1000.0; idx = 2; } break;  // Ti ions
                    case 187: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 1000.0; idx = 3; } break;  // Ti ions
                    case 188: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 1000.0; idx = 4; } break;  // Ti ions
                    //
                    case 189: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 1600.0; idx = 1; } break;  // Ti ions
                    case 190: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 1600.0; idx = 2; } break;  // Ti ions
                    case 191: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 1600.0; idx = 3; } break;  // Ti ions
                    case 192: { m_dMass = 48.0; m_dCharge = 22.0; m_dEnergy = 1600.0; idx = 4; } break;  // Ti ions
                    //
                    case 193: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 10.0; idx = 1; } break;  // p ions
                    case 194: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 10.0; idx = 2; } break;  // p ions
                    case 195: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 10.0; idx = 3; } break;  // p ions
                    case 196: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 10.0; idx = 4; } break;  // p ions
                    //
                    case 197: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 50.0; idx = 1; } break;  // p ions
                    case 198: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 50.0; idx = 2; } break;  // p ions
                    case 199: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 50.0; idx = 3; } break;  // p ions
                    case 200: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 50.0; idx = 4; } break;  // p ions
                    //
                    case 201: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 100.0; idx = 1; } break;  // p ions
                    case 202: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 100.0; idx = 2; } break;  // p ions
                    case 203: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 100.0; idx = 3; } break;  // p ions
                    case 204: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 100.0; idx = 4; } break;  // p ions
                    //
                    case 205: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 200.0; idx = 1; } break;  // p ions
                    case 206: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 200.0; idx = 2; } break;  // p ions
                    case 207: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 200.0; idx = 3; } break;  // p ions
                    case 208: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 200.0; idx = 4; } break;  // p ions
                    //
                    case 209: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 400.0; idx = 1; } break;  // p ions
                    case 210: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 400.0; idx = 2; } break;  // p ions
                    case 211: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 400.0; idx = 3; } break;  // p ions
                    case 212: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 400.0; idx = 4; } break;  // p ions
                    //
                    case 213: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 800.0; idx = 1; } break;  // p ions
                    case 214: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 800.0; idx = 2; } break;  // p ions
                    case 215: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 800.0; idx = 3; } break;  // p ions
                    case 216: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 800.0; idx = 4; } break;  // p ions
                    //
                    case 217: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 1000.0; idx = 1; } break;  // p ions
                    case 218: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 1000.0; idx = 2; } break;  // p ions
                    case 219: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 1000.0; idx = 3; } break;  // p ions
                    case 220: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 1000.0; idx = 4; } break;  // p ions
                    //
                    case 221: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 1600.0; idx = 1; } break;  // p ions
                    case 222: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 1600.0; idx = 2; } break;  // p ions
                    case 223: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 1600.0; idx = 3; } break;  // p ions
                    case 224: { m_dMass = 1.00; m_dCharge = 1.00; m_dEnergy = 1600.0; idx = 4; } break;  // p ions
                    //
                    case 225: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 10.0; idx = 1; } break;  // Fe ions
                    case 226: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 10.0; idx = 2; } break;  // Fe ions
                    case 227: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 10.0; idx = 3; } break;  // Fe ions
                    case 228: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 10.0; idx = 4; } break;  // Fe ions
                    //
                    case 229: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 50.0; idx = 1; } break;  // Fe ions
                    case 230: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 50.0; idx = 2; } break;  // Fe ions
                    case 231: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 50.0; idx = 3; } break;  // Fe ions
                    case 232: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 50.0; idx = 4; } break;  // Fe ions
                    //
                    case 233: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 100.0; idx = 1; } break;  // Fe ions
                    case 234: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 100.0; idx = 2; } break;  // Fe ions
                    case 235: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 100.0; idx = 3; } break;  // Fe ions
                    case 236: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 100.0; idx = 4; } break;  // Fe ions
                    //
                    case 237: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 200.0; idx = 1; } break;  // Fe ions
                    case 238: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 200.0; idx = 2; } break;  // Fe ions
                    case 239: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 200.0; idx = 3; } break;  // Fe ions
                    case 240: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 200.0; idx = 4; } break;  // Fe ions
                    //
                    case 241: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 400.0; idx = 1; } break;  // Fe ions
                    case 242: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 400.0; idx = 2; } break;  // Fe ions
                    case 243: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 400.0; idx = 3; } break;  // Fe ions
                    case 244: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 400.0; idx = 4; } break;  // Fe ions
                    //
                    case 245: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 800.0; idx = 1; } break;  // Fe ions
                    case 246: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 800.0; idx = 2; } break;  // Fe ions
                    case 247: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 800.0; idx = 3; } break;  // Fe ions
                    case 248: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 800.0; idx = 4; } break;  // Fe ions
                    //
                    case 249: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 1000.0; idx = 1; } break;  // Fe ions
                    case 250: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 1000.0; idx = 2; } break;  // Fe ions
                    case 251: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 1000.0; idx = 3; } break;  // Fe ions
                    case 252: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 1000.0; idx = 4; } break;  // Fe ions
                    //
                    case 253: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 1600.0; idx = 1; } break;  // Fe ions
                    case 254: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 1600.0; idx = 2; } break;  // Fe ions
                    case 255: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 1600.0; idx = 3; } break;  // Fe ions
                    case 256: { m_dMass = 56.0; m_dCharge = 26.0; m_dEnergy = 1600.0; idx = 4; } break;  // Fe ions
                    //
                    case 257: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 10.0; idx = 1; } break;  // He ions
                    case 258: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 10.0; idx = 2; } break;  // He ions
                    case 259: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 10.0; idx = 3; } break;  // He ions
                    case 260: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 10.0; idx = 4; } break;  // He ions
                    //
                    case 261: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 50.0; idx = 1; } break;  // He ions
                    case 262: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 50.0; idx = 2; } break;  // He ions
                    case 263: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 50.0; idx = 3; } break;  // He ions
                    case 264: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 50.0; idx = 4; } break;  // He ions
                    //
                    case 265: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 100.0; idx = 1; } break;  // He ions
                    case 266: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 100.0; idx = 2; } break;  // He ions
                    case 267: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 100.0; idx = 3; } break;  // He ions
                    case 268: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 100.0; idx = 4; } break;  // He ions
                    //
                    case 269: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 200.0; idx = 1; } break;  // He ions
                    case 270: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 200.0; idx = 2; } break;  // He ions
                    case 271: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 200.0; idx = 3; } break;  // He ions
                    case 272: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 200.0; idx = 4; } break;  // He ions
                    //
                    case 273: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 400.0; idx = 1; } break;  // He ions
                    case 274: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 400.0; idx = 2; } break;  // He ions
                    case 275: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 400.0; idx = 3; } break;  // He ions
                    case 276: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 400.0; idx = 4; } break;  // He ions
                    //
                    case 277: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 800.0; idx = 1; } break;  // He ions
                    case 278: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 800.0; idx = 2; } break;  // He ions
                    case 279: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 800.0; idx = 3; } break;  // He ions
                    case 280: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 800.0; idx = 4; } break;  // He ions
                    //
                    case 281: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 1000.0; idx = 1; } break;  // He ions
                    case 282: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 1000.0; idx = 2; } break;  // He ions
                    case 283: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 1000.0; idx = 3; } break;  // He ions
                    case 284: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 1000.0; idx = 4; } break;  // He ions
                    //
                    case 285: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 1600.0; idx = 1; } break;  // He ions
                    case 286: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 1600.0; idx = 2; } break;  // He ions
                    case 287: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 1600.0; idx = 3; } break;  // He ions
                    case 288: { m_dMass = 4.00; m_dCharge = 2.00; m_dEnergy = 1600.0; idx = 4; } break;  // He ions
                    //
                    default: return -1; //break;
                }
                if (m_dMass < 0) return -1;
                if (m_dCharge == Convert.ToDouble(m_dCharge1(ion)) && m_dEnergy == m_dEnergy1 && idx == Idx1(dose))
                {
                    if (orientation != "Parallel")
                        nonce++;
                    return nonce;
                }
            }
            return -1;
        }

        public int Idx1(double D) // if the dose is not listed, idx=0
        {
            // Dose index scheme
            int idx = 0;
            if (D == 0.5)
                idx = 1;
            if (D == 1.0)
                idx = 2;
            if (D == 2.0)
                idx = 3;
            if (D == 4.0)
                idx = 4;
            return idx;
        }

        public int m_dCharge1(string ion) // returns -1, if ion==""; returns 0 if the ion is unknown
        {
            if (ion == null || ion == "" || ion == String.Empty || String.IsNullOrEmpty(ion))
                return -1;
            int Z = 1;
            string str = "";
            while (true)
            {
                switch (Z)
                {
                    case 1:
                        str = "p";
                        break;
                    case 2:
                        str = "He";
                        break;
                    case 6:
                        str = "C";
                        break;
                    case 8:
                        str = "O";
                        break;
                    case 10:
                        str = "Ne";
                        break;
                    case 14:
                        str = "Si";
                        break;
                    case 18:
                        str = "Ar";
                        break;
                    case 22:
                        str = "Ti";
                        break;
                    case 26:
                        str = "Fe";
                        break;
                    default:
                        str = "";
                        break;
                }
                if (str == ion)
                    return Z;
                Z++;
                if (Z > 26)
                {
                    return 0;
                }
            }
        }
        private int Ioncharge(string str)
        {
            //         double mass=0.0;
            int charge = 0;
            switch (str)
            {
                case "Ar":
                    {
                        //                   mass=40.00;
                        charge = 18;
                    }
                    break;
                case "C":
                    {
                        //                   mass=     12.0;
                        charge = 6;
                    }
                    break;
                case "O":
                    {
                        //   m_dMass = 16.0;
                        charge = 8;
                    }
                    break;
                case "N":
                    {
                        //     m_dMass = 20.0; 
                        charge = 10;
                    }
                    break;
                case "Si":
                    {
                        //m_dMass = 28.0;
                        charge = 14;
                    }
                    break;
                case "Ti":
                    {
                        // m_dMass = 48.0;
                        charge = 22;
                    }
                    break;
                case "Ne":
                    {
                        charge = 10;
                    }
                    break;
                case "p":
                    {
                        //m_dMass = 1.00; 
                        charge = 1;
                    }
                    break;
                case "Fe":
                    {
                        //m_dMass = 56.0; 
                        charge = 26;
                    }
                    break;
                case "He":
                    {
                        //m_dMass = 4.00;
                        charge = 2;
                    }
                    break;
                default:
                    return 0;
            }
            return charge;
        }
    }
}