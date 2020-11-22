using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;

namespace GraficDisplay
{
    internal static partial class Visualize
    {
        public static List<Object> interactingFragmentSet = null;
        public static void CreateWordDoc(List<Object> listObjs, string message)
        {
            try
            {
                int currentpage = 0;
                string appPath = Application.StartupPath;
                string str = File.ReadAllText(appPath + "\\nonce.dat");
                int nonce = Convert.ToInt32(str);
                nonce++;
                File.WriteAllText(appPath + "\\nonce.dat", nonce.ToString());
                //
                //Create an instance for word app
                Word.Application oWord = new Word.Application
                {

                    //Set animation status for word application
                    //         ShowAnimation = false, // zzz

                    //Set status for word application is to be visible or not.
                    Visible = false
                };

                //Create a missing variable for missing value
                object missing = System.Reflection.Missing.Value;

                //Create a new document
                Word.Document oDocument = oWord.Documents.Add(ref missing, ref missing, ref missing, ref missing);

                //Add header into the document
                foreach (Word.Section section in oDocument.Sections)
                {
                    //Get the header range and add the header details.
                    Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.Fields.Add(headerRange, Word.WdFieldType.wdFieldPage);
                    headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    headerRange.Font.ColorIndex = Word.WdColorIndex.wdBlack;
                    headerRange.Font.Size = 20;
                    headerRange.Text = "Chromosome aberrations classification\n";
                }

                //Add paragraph with Heading 1 style
                Word.Paragraph para1 = oDocument.Content.Paragraphs.Add(ref missing);
                object styleHeading1 = "Heading 1";
                para1.Range.set_Style(ref styleHeading1);
                para1.Range.Text = "Table 1. Radiated Chromosome Map Legend.";
                para1.Range.InsertParagraphAfter();
                //
                //Create a table and insert chromo's definition
                Word.Table oTable = oDocument.Tables.Add(para1.Range, 2, 5, ref missing, ref missing);
                oTable.Borders.Enable = 1;
                foreach (Word.Row row in oTable.Rows)
                {
                    foreach (Word.Cell cell in row.Cells)
                    {
                        //Header row
                        if (cell.RowIndex == 1)
                        {
                            switch (cell.ColumnIndex)
                            {
                                case 1:
                                    cell.Range.Text = "Chromo body";
                                    break;
                                case 2:
                                    cell.Range.Text = "Unrepaired DSB";
                                    break;
                                case 3:
                                    cell.Range.Text = "Repaired DSB";
                                    break;
                                case 4:
                                    cell.Range.Text = "Centromere";
                                    break;
                                case 5:
                                    cell.Range.Text = "Telomere";
                                    break;
                                default:
                                    cell.Range.Text = "Not known";
                                    break;
                            }
                            cell.Range.Font.Bold = 1;
                            //other format properties goes here
                            cell.Range.Font.Name = "verdana";
                            cell.Range.Font.Size = 10;
                            //cell.Range.Font.ColorIndex = WdColorIndex.wdGray25;                            
                            cell.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray25;
                            //Center alignment for the Header cells
                            cell.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                            cell.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        }
                        //Data row
                        else
                        {
                            cell.Row.Height = 250;
                            switch (cell.ColumnIndex)
                            {
                                case 1:
                                    //
                                    // define a chromosome
                                    //
                                    {
                                        Word.Range shapeAnchor = oTable.Cell(cell.RowIndex, cell.ColumnIndex).Range; // zzz -- has no impact
                                        //chromoShape.WrapFormat.Type = Microsoft.Office.Interop.Word.WdWrapType.wdWrapInline;
                                        Word.Shape chromoShape = oDocument.Shapes.AddShape(13, 110, 150, 20, 200, shapeAnchor); // chromosome
                                        chromoShape.Fill.Solid();
                                        chromoShape.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DarkBlue);
                                    }
                                    break;
                                case 2:
                                    {
                                        Word.Shape chromoShape1 = oDocument.Shapes.AddShape(18, 200, 230, 20, 20, ref missing); // open DSB
                                        chromoShape1.Fill.Solid();
                                        chromoShape1.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DarkBlue);
                                    }
                                    break;
                                case 3:
                                    {
                                        Word.Shape chromoShape2 = oDocument.Shapes.AddShape(19, 300, 230, 20, 20, ref missing); // repaired DSB   
                                        chromoShape2.Fill.Solid();
                                        chromoShape2.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DarkBlue);
                                    }
                                    break;
                                case 4:
                                    {
                                        Word.Shape chromoShape3 = oDocument.Shapes.AddShape(6, 380, 230, 20, 20, ref missing); // centromere  
                                        chromoShape3.Fill.Solid();
                                        chromoShape3.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DarkBlue);
                                    }
                                    break;
                                case 5:
                                    {
                                        Word.Shape chromoShape4 = oDocument.Shapes.AddShape(1, 480, 230, 20, 20, ref missing); // telomere
                                        chromoShape4.Fill.Solid();
                                        chromoShape4.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DarkBlue);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                //
                oDocument.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                //Goto some specific page and insert a blank page or page break   
                object gotoPage = Word.WdGoToItem.wdGoToPage;
                object gotoNext = Word.WdGoToDirection.wdGoToNext;
                object gotoCount = null;
                object gotoName = "2";
                oWord.Selection.GoTo(ref gotoPage, ref gotoNext, ref gotoCount, ref gotoName);
                //
                //Add paragraph with Heading 2 style
                Word.Paragraph para2 = oDocument.Content.Paragraphs.Add(ref missing);
                para2.Range.set_Style(ref styleHeading1);
                para2.Range.Text = Environment.NewLine + "Intact Genome before Irradiation: there are 3 painted pairs of chromosomes in the genome; the rest are DAPI-stained.";
                //para2.Range.Font.Size= 10; 
                para2.Range.InsertParagraphAfter();
                //
                // draw the intact genome                        
                //   
                for (int i1 = 0; i1 < IntactHumanGenome.nObjs; i1++)
                {
                    InsertIntactChromos(i1, oDocument);
                }
                //
                oDocument.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                //Goto some specific page and insert a blank page or page break   
                gotoPage = Word.WdGoToItem.wdGoToPage;
                gotoNext = Word.WdGoToDirection.wdGoToNext;
                gotoCount = null;
                gotoName = "3";
                oWord.Selection.GoTo(ref gotoPage, ref gotoNext, ref gotoCount, ref gotoName);
                //
                //Add paragraph with Heading 2 style
                Word.Paragraph para3 = oDocument.Content.Paragraphs.Add(ref missing);
                para3.Range.set_Style(ref styleHeading1);
                int i3 = 0;
                foreach (Object o in listObjs)
                {
                    if (o.md.O_type == MetaData.ObjectType.intact_chromo)
                    {
                        i3++;
                    }
                }

                para3.Range.Text = message + "-- all chromosomes (" + i3.ToString() + "), which remained intact (and not repaired) after radiation:";
                para3.Range.InsertParagraphAfter();
                //
                InsertIntactFragment(oDocument, listObjs);
                //
                oDocument.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                //Goto some specific page and insert a blank page or page break   
                gotoPage = Word.WdGoToItem.wdGoToPage;
                gotoNext = Word.WdGoToDirection.wdGoToNext;
                gotoCount = null;
                gotoName = "4";
                currentpage = Convert.ToInt32(gotoName);
                oWord.Selection.GoTo(ref gotoPage, ref gotoNext, ref gotoCount, ref gotoName);
                //             
                //Add paragraph with Heading 2 style
                Word.Paragraph para4 = oDocument.Content.Paragraphs.Add(ref missing);
                para4.Range.set_Style(ref styleHeading1);
                int fr = 0, ur = 0;
                foreach (Object o in listObjs)
                {
                    if (o.md.O_type == MetaData.ObjectType.fully_repaired)
                    {
                        fr++; // zzz666 this has to go toward the final count of intact chromosomes
                    }
                    else
                    {
                        if (o.md.O_type != MetaData.ObjectType.intact_chromo)
                        {
                            ur++;
                        }
                    }
                }
                para4.Range.Text = message + "-- recombined (magenta, count " + fr.ToString() + ") chromosomes and unrepaired/misrepaired (DAPI blue and the painted pairs colors, count "
                    + ur.ToString() + ") fragments at " + TimeOperator.expTime.ToString() + " hours";
                para4.Range.InsertParagraphAfter();
                //    
                if (listObjs.Count != 0)
                {
                    currentpage = InsertBrokenChromos1(oWord, oDocument, listObjs, currentpage);
                }
                //
                oDocument.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                //Goto some specific page and insert a blank page or page break   
                gotoPage = Word.WdGoToItem.wdGoToPage;
                gotoNext = Word.WdGoToDirection.wdGoToNext;
                gotoCount = null;
                currentpage++;
                gotoName = currentpage.ToString();
                currentpage = Convert.ToInt32(gotoName);
                oWord.Selection.GoTo(ref gotoPage, ref gotoNext, ref gotoCount, ref gotoName);
                //             
                //Add paragraph with Heading 2 style
                Word.Paragraph para5 = oDocument.Content.Paragraphs.Add(ref missing);
                para5.Range.set_Style(ref styleHeading1);
                // purge the object list from fully repaired and only DAPI blue fragments
                List<Object> list_temp = new List<Object>();
                foreach (Object o in listObjs)
                {
                    if (o.md.O_type == MetaData.ObjectType.fully_repaired)
                    {
                        list_temp.Add(o);
                    }
                }
                foreach (Object o in list_temp)
                {
                    listObjs.Remove(o);
                }
                List<Object> list_temp1 = new List<Object>();
                foreach (Object o in listObjs)
                {
                    if (IsDAPI(o))
                    {
                        list_temp1.Add(o);
                    }
                }
                foreach (Object o in list_temp1)
                {
                    listObjs.Remove(o);
                }
                if (listObjs.Count != 0 && listObjs != null)
                {
                    para5.Range.Text = message + "-- unrepaired/misrepaired/open fragments containing the painted pairs colors, count "
                        + listObjs.Count.ToString() + ", at " + TimeOperator.expTime.ToString() + " hours";
                    para5.Range.InsertParagraphAfter();
                    //           
                    currentpage = InsertBrokenChromos1(oWord, oDocument, listObjs, currentpage);
                    while (true)
                    {
                        string str3 = AberrationAnalysis(listObjs);
                        //
                        oDocument.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                        //Goto some specific page and insert a blank page or page break   
                        gotoPage = Word.WdGoToItem.wdGoToPage;
                        gotoNext = Word.WdGoToDirection.wdGoToNext;
                        gotoCount = null;
                        currentpage++;
                        gotoName = currentpage.ToString();
                        oWord.Selection.GoTo(ref gotoPage, ref gotoNext, ref gotoCount, ref gotoName);
                        //
                        //Add paragraph with Heading 2 style
                        Word.Paragraph para6 = oDocument.Content.Paragraphs.Add(ref missing);
                        para6.Range.set_Style(ref styleHeading1);
                        para6.Range.Text = message + "-- interacting set\t\t" + str3;
                        para6.Range.InsertParagraphAfter();
                        //               
                        if (interactingFragmentSet != null)
                        {
                            currentpage = InsertBrokenChromos1(oWord, oDocument, interactingFragmentSet, currentpage);
                        }
                        //
                        oDocument.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                        //Goto some specific page and insert a blank page or page break   
                        gotoPage = Word.WdGoToItem.wdGoToPage;
                        gotoNext = Word.WdGoToDirection.wdGoToNext;
                        currentpage++;
                        gotoName = currentpage.ToString();
                        oWord.Selection.GoTo(ref gotoPage, ref gotoNext, ref gotoCount, ref gotoName);
                        //
                        string str4 = "";
                        int i1 = 0;
                        if (interactingFragmentSet != null)
                        {
                            foreach (Object o in interactingFragmentSet)
                            {
                                i1++;
                                str4 += "fragment #" + i1.ToString() + "\t\t"
                                    + "number of bands: " + o.md.chromo_bands.Count + Environment.NewLine;
                                int i2 = 0;
                                foreach (Band b in o.md.chromo_bands)
                                {
                                    i2++;
                                    str4 += "band #" + i2.ToString() + "\t\t band's size: " + b.Size.ToString()
                                        + "\t\t band's location in the frag.: " + b.Position_within_object.ToString() + "\t\t band's constituent chromo: " + (b.Chromo_num + 1).ToString()
                                        + "\t\t band's color: " + (b.Color == System.Drawing.Color.DarkBlue ? "DAPI" : b.Color.Name.ToString()) + Environment.NewLine;
                                }
                                str4 += "Number of free ends is " + o.md.f_e.Count + ":\t";
                                foreach (Free_end fe in o.md.f_e)
                                {
                                    str4 += fe.FE_type.ToString() + "\t";
                                }
                                str4 += Environment.NewLine;
                            }
                        }
                        //
                        //Add paragraph with Heading 2 style
                        Word.Paragraph para7 = oDocument.Content.Paragraphs.Add(ref missing);
                        para7.Range.set_Style(ref styleHeading1);
                        string trailingStringIfTextCut = "................";
                        int trailLength = trailingStringIfTextCut.StartsWith("&") ? 1 : trailingStringIfTextCut.Length;
                        int maxCharacters = 1000;
                        maxCharacters = maxCharacters - trailLength >= 0 ? maxCharacters - trailLength : 0;
                        if (str.Length > maxCharacters) // zzz666 what does this do?
                        {
                            int pos = str4.LastIndexOf(" ", maxCharacters);
                            str4 = str4.Substring(0, pos) + trailingStringIfTextCut;
                        }
                        para7.Range.Text = message + "-- interacting fragment set details:" + Environment.NewLine + str4;
                        para7.Range.InsertParagraphAfter();
                        //
                        if (listObjs.Count == 0)
                        {
                            break;
                        }
                        //
                        for (int i = 0; i < str4.Length / 844; i++) // zzz666
                        {
                            oDocument.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                            //Goto some specific page and insert a blank page or page break   
                            gotoPage = Word.WdGoToItem.wdGoToPage;
                            gotoNext = Word.WdGoToDirection.wdGoToNext;
                            currentpage++;
                            gotoName = currentpage.ToString();
                            oWord.Selection.GoTo(ref gotoPage, ref gotoNext, ref gotoCount, ref gotoName);
                        }
                        //                     
                        //Add paragraph with Heading 2 style
                        Word.Paragraph para8 = oDocument.Content.Paragraphs.Add(ref missing);
                        para8.Range.set_Style(ref styleHeading1);
                        para8.Range.Text = message + "-- remaining fragments to be analyzed (with uncounted DAPI fragments removed)";
                        para8.Range.InsertParagraphAfter();
                        //
                        if (listObjs.Count != 0)
                        {
                            InsertBrokenChromos1(oWord, oDocument, listObjs, currentpage);   // insert the remaining fragments to be analyzed                                           
                        }
                    }
                }
                // Save the document         
                string wbPath = appPath + "\\temp\\";
                string str1 = null;
                if (message == "Incremental step in the final state analysis: \n")
                {
                    str1 = "_after_repair";
                }
                object filename = wbPath + @"ChromosomeAberrationsRecord&Analysis" + nonce.ToString() + str1 + ".docx";
                oDocument.SaveAs2(ref filename);
                oDocument.Close(ref missing, ref missing, ref missing);
                oDocument = null;
                oWord.Quit(ref missing, ref missing, ref missing);
                oWord = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}