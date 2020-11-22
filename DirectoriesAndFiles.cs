using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GraficDisplay
{
    public partial class MainForm : Form
    {
        private TextBox textBox1;
        //private void RemoveBlankSpacesToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    Form Form1 = new Form
        //    {
        //        Text = "Remove Blank Spaces"
        //    };
        //    textBox1 = new TextBox
        //    {
        //        Text = "Enter a dir name with files: ",
        //        Width = 250,
        //        Height = 50,
        //        Multiline = true,
        //        BackColor = Color.Blue,
        //        ForeColor = Color.White,
        //        BorderStyle = BorderStyle.Fixed3D
        //    };
        //    Form1.Controls.Add(textBox1);
        //    Button button = new Button
        //    {
        //        Location = new Point(50, 100),
        //        Text = "Do"
        //    };
        //    button.Click += new EventHandler(Button_Click);
        //    Form1.Controls.Add(button);
        //    Form1.Show();
        //}

        private void Button_Click(object sender, EventArgs e)
        {
            string text = textBox1.Text;
            string[] fileEntries = Directory.GetFiles(text);
            foreach (string fileName in fileEntries)
            {
                string str = fileName.Replace(" ", string.Empty);
                File.Move(fileName, str);
            }
            MessageBox.Show("Blank spaces in file names are removed in dir " + text.ToString());
        }

        private Button buttonOK;
        private void PostprocessSCDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form Form1 = new Form
            {
                Text = "Post-process S.C. data; detailed report",
                Width = 500
            };
            textBox1 = new TextBox
            {
                Text = @"C:\Users\aponomar\Desktop\combndSC", // detailed report
                Width = 400,
                Height = 50,
                Multiline = true,
                BackColor = Color.Blue,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };
            Form1.Controls.Add(textBox1);
            //
            buttonOK = new Button
            {
                Location = new Point(50, 100),
                Text = "Do"
            };
            buttonOK.Click += new EventHandler(Button_Click1);
            Form1.Controls.Add(buttonOK);
            //
            Button buttonCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(buttonOK.Left, buttonOK.Height + buttonOK.Top + 10)
            };
            Form1.CancelButton = buttonCancel;
            Form1.Controls.Add(buttonCancel);
            buttonCancel.Click += new EventHandler(Cancel_Click);
            //
            //checkBox = new CheckBox();
            //checkBox.Location = new Point(buttonOK.Left, buttonOK.Height + buttonOK.Top + 45);
            //checkBox.Name = "Remove Empty Lines in Data Files";
            //checkBox.Text = "Remove Empty Lines in Data Files";
            //checkBox.Size = new Size(200, 17);
            //checkBox.Checked = boolRemoveBlankLines;
            //checkBox.CheckedChanged += new EventHandler(CheckBoxOnCheckedChanged2);
            //Form1.Controls.Add(checkBox);
            //
            Form1.Show();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Button_Click1(object sender, EventArgs e)
        {
            string text = textBox1.Text;
            string[] fileEntries = Directory.GetFiles(text);
            //if (boolRemoveBlankLines)
            //{
            //    foreach (string fileName in fileEntries)
            //    {
            //        string n = "";
            //        StreamReader sr = new StreamReader(fileName);
            //        while (!sr.EndOfStream)
            //        {
            //            var line = sr.ReadLine();
            //            n += line + Environment.NewLine;
            //        }
            //        sr.Close();
            //        n = Regex.Replace(n, @"^\s+$[\r\n]*", "", RegexOptions.Multiline); // get rid of any empty lines
            //        File.WriteAllText(fileName, n);
            //    }
            //    MessageBox.Show("Empty lines removed in all files...");
            //    Application.Exit();
            //}
            //else
            //{
            const int nSheetsMax = 30;
            int nRowsMax = 2000;  // 1,000 is the expected max number of MC histories; max #rows is 1048576 
            int nBooks = fileEntries.Length / nSheetsMax;
            string ionNameOld = "";
            double energyOld = 0.0;
            double doseOld = 0.0;
            string orientationOld = "";
            int maxNHist = 0;
            double nDSBaver = 0.0;
            int maxNHistOld = 0;
            double nDSBaverOld = 0.0;
            string wbPath = null;
            int nsheet = 0;
            int nsheetOld = 0;
            int j = 0; // file number
            //
            string appPath = Application.StartupPath;
            File.WriteAllText(appPath + "fileEntries.txt", fileEntries.ToString());
            for (int i = 0; i < nBooks; i++)
            {
                //string str2 = File.ReadAllText(appPath + "\\nonce.dat");
                //int nonce = Convert.ToInt32(str2);
                //nonce++;
                //File.WriteAllText(appPath + "\\nonce.dat", nonce.ToString());
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
                object misvalue = System.Reflection.Missing.Value;
                oWB.RefreshAll();
                //
                string[,] saNames = new string[nRowsMax, 17];
                int k = 0;
                try
                {
                    int ns = 0;
                    nsheetOld = nsheet;

                    // Add table headers going cell by cell
                    oSheet.Cells[1, 1 + 1] = "MC hist #";
                    oSheet.Cells[1, 2 + 1] = "Ion name";
                    oSheet.Cells[1, 3 + 1] = "Ion charge";
                    oSheet.Cells[1, 4 + 1] = "Ion E";
                    oSheet.Cells[1, 5 + 1] = "Dose, Gy";
                    oSheet.Cells[1, 6 + 1] = "#tracks";
                    oSheet.Cells[1, 7 + 1] = "#DSBs";
                    oSheet.Cells[1, 8 + 1] = "orientation";
                    oSheet.Cells[1, 9 + 1] = "completion time";
                    oSheet.Cells[1, 10 + 1] = "Algo type";
                    oSheet.Cells[1, 11 + 1] = "File Name";

                    //Format A1:Z1 as bold, vertical alignment = center
                    oSheet.get_Range("A1", "Z1").Font.Bold = true;
                    oSheet.get_Range("A1", "Z1").VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                    oSheet.get_Range("A1", "Z1").EntireColumn.AutoFit();
                    oSheet.get_Range("A" + 2.ToString(), "Z" + 2.ToString()).Font.Size = 14;
                    oSheet.get_Range("A" + 1.ToString(), "M" + 1.ToString()).Font.Color = Color.DarkRed;
                    //
                    k = 0;
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
                                            string line = lines[l];
                                            string[] lineData = line.Split('\t');
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
                                        if (ion == ionNameOld && energy == energyOld && dose == doseOld && orientation == orientationOld)
                                        {
                                            maxNHist++;
                                            saNames[1, 0] = "#MC histories = " + maxNHist.ToString();
                                            saNames[2, 0] = "#DSB aver = " + (maxNHist == 0 ? nDSBaver : (nDSBaver / maxNHist)).ToString();
                                            saNames[3, 0] = "#DSB/Gy = " + ((maxNHist == 0 ? nDSBaver : (nDSBaver / maxNHist)) / dose).ToString();
                                            wbPath = SampleNumber(ion, energy, dose, orientation).ToString();
                                        }
                                        else
                                        {
                                            ionNameOld = ion;
                                            energyOld = energy;
                                            doseOld = dose;
                                            orientationOld = orientation;
                                            maxNHist = 0;
                                            nDSBaver = 0.0;
                                            if (j != 0)
                                            {
                                                ns++;
                                                nsheet++;
                                                if (ns == nSheetsMax)
                                                {
                                                    break;
                                                }

                                                oSheet = (Microsoft.Office.Interop.Excel.Worksheet)oWB.Worksheets.Add
                                                    (System.Reflection.Missing.Value,
                                                    oWB.Worksheets[oWB.Worksheets.Count],
                                                    System.Reflection.Missing.Value,
                                                    System.Reflection.Missing.Value);

                                                // Add table headers going cell by cell
                                                oSheet.Cells[1, 1 + 1] = "MC hist #";
                                                oSheet.Cells[1, 2 + 1] = "Ion name";
                                                oSheet.Cells[1, 3 + 1] = "Ion charge";
                                                oSheet.Cells[1, 4 + 1] = "Ion E";
                                                oSheet.Cells[1, 5 + 1] = "Dose, Gy";
                                                oSheet.Cells[1, 6 + 1] = "#tracks";
                                                oSheet.Cells[1, 7 + 1] = "#DSBs";
                                                oSheet.Cells[1, 8 + 1] = "orientation";
                                                oSheet.Cells[1, 9 + 1] = "completion time";
                                                oSheet.Cells[1, 10 + 1] = "Algo type";
                                                oSheet.Cells[1, 11 + 1] = "File Name";

                                                //Format A1:Z1 as bold, vertical alignment = center
                                                oSheet.get_Range("A1", "Z1").Font.Bold = true;
                                                oSheet.get_Range("A1", "Z1").VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                                                oSheet.get_Range("A1", "Z1").EntireColumn.AutoFit();
                                                oSheet.get_Range("A" + 2.ToString(), "Z" + 2.ToString()).Font.Size = 14;
                                                oSheet.get_Range("A" + 1.ToString(), "M" + 1.ToString()).Font.Color = Color.DarkRed;
                                                //
                                                maxNHistOld = maxNHist;
                                                nDSBaverOld = nDSBaver;
                                                maxNHist = 0;
                                                nDSBaver = 0.0;
                                                k = 0;
                                                Array.Clear(saNames, 0, saNames.Length);
                                            }
                                        }
                                        saNames[k, 1] = (k + 1).ToString(); //  (inputData[5] == "Parallel" ? inputData[6] : inputData[5]);
                                        saNames[k, 2] = ion;
                                        int charge = Ioncharge(ion);
                                        saNames[k, 3] = (charge == 0 ? "" : charge.ToString()).ToString();
                                        saNames[k, 4] = energy.ToString();
                                        saNames[k, 5] = dose.ToString();
                                        saNames[k, 6] = nTracks.ToString();
                                        saNames[k, 7] = nDSB.ToString();
                                        saNames[k, 8] = orientation;
                                        saNames[k, 9] = File.GetCreationTime(fileEntries[j]).ToString();
                                        saNames[k, 10] = "Amorphous Tracks";
                                        saNames[k, 11] = fileEntries[j];
                                        //
                                        oSheet.get_Range("A2", "N" + (nRowsMax + 1).ToString()).Value2 = saNames;
                                        oSheet.get_Range("A" + (k + 2).ToString(), "Z" + (k + 2).ToString()).Font.Size = 14;
                                        oSheet.get_Range("A" + (k + 2).ToString(), "Z" + (k + 2).ToString()).ClearContents();
                                        oSheet.Application.ScreenUpdating = false;
                                        //      
                                        //AutoFit columns A:Z
                                        oRng = oSheet.get_Range("A1", "Z1");
                                        oRng.EntireColumn.AutoFit();
                                        //                                      
                                        k++; // row number                  
                                    }
                                    catch
                                    {
                                        saNames[k++, 12] = "Data did not load properly:    \tnDSB = " + nDSBaver.ToString() + "          \t" + fileEntries[j].ToString();
                                    }
                                }
                                else
                                {
                                    saNames[k, 12] = "Empty file: " + fileEntries[j];
                                }
                            }
                            j++; // file number                                             
                            if (j > fileEntries.Length - 1)
                            {
                                saNames[k - 1, 0 + 1] = maxNHist.ToString();
                                nDSBaver /= (Convert.ToDouble(maxNHist) > 0 ? Convert.ToDouble(maxNHist) : 1.0);
                                saNames[k - 1, 1 + 1] = nDSBaver.ToString("#.###");
                                oWB.SaveAs(appPath + "\\temp\\SC_sheetNum_" + nsheetOld.ToString() + "_" + nsheet.ToString() + ".xls",
                                    Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, Type.Missing, Type.Missing, false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                                oWB.Close(false, wbPath, Type.Missing);
                                oXL.Quit();
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(oXL);
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(oWB);
                                Close();
                            }
                        }
                        catch
                        {
                            saNames[k++, 12] = "Processing failure: " + fileEntries[j];
                            j++; // file number
                            if (j > fileEntries.Length - 1)
                            {
                                oWB.SaveAs(appPath + "\\temp\\SC_sheetNum_" + nsheetOld.ToString() + "_" + nsheet.ToString() + ".xls",
                                    Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, Type.Missing, Type.Missing, false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                                oWB.Close(false, wbPath, Type.Missing);
                                oXL.Quit();
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(oXL);
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(oWB);
                                Close();
                            }
                        }
                    }
                    oWB.SaveAs(appPath + "\\temp\\SC_sheetNum_" + nsheetOld.ToString() + "_" + nsheet.ToString() + ".xls",
                        Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, Type.Missing, Type.Missing, false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                }
                catch
                {
                    saNames[k++, 12] = "Book faliure: " + fileEntries[j];
                    j++; // file number
                    if (j > fileEntries.Length - 1)
                    {
                        oWB.SaveAs(appPath + "\\temp\\SC_sheetNum_" + nsheetOld.ToString() + "_" + nsheet.ToString() + ".xls",
                            Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, Type.Missing, Type.Missing, false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                        oWB.Close(false, wbPath, Type.Missing);
                        oXL.Quit();
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oXL);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oWB);
                        Close();
                    }
                }
                oWB.Close(false, wbPath, Type.Missing);
                oXL.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oXL);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oWB);
                Close();
            }
        }

        //private void CheckBoxOnCheckedChanged2(object sender, EventArgs e)
        //{
        //    if (checkBox.Checked)
        //        boolRemoveBlankLines = true;
        //    else
        //        boolRemoveBlankLines = false;
        //    Invalidate(false);
        //}
    }
}