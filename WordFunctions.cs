using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Word = Microsoft.Office.Interop.Word;

namespace GraficDisplay
{
    internal static partial class Visualize
    {
        public static int InsertBrokenChromos1(Word.Application oWord, Word.Document oDocument, List<Object> listObjs, int currentpage)
        {
            try
            {
                int isizeMin = 50;
                int isizeMax = 2 * isizeMin;
                List<Object> list_temp = new List<Object>();
                object missing = System.Reflection.Missing.Value;
                int k = 0;
                foreach (Object o in listObjs)
                    if (o.md.O_type != MetaData.ObjectType.intact_chromo)
                    {
                        list_temp.Add(o);
                        k++;
                    }
                int npages = k / 10 + 1;
                int page = 0;
                for (int obj = 0; obj < list_temp.Count; obj++)
                {
                    if (obj % 10 == 0)
                    {
                        if (page > 0)
                        {
                            oDocument.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                            //Goto some specific page and insert a blank page or page break               
                            object gotoPage = Word.WdGoToItem.wdGoToPage;
                            object gotoNext = Word.WdGoToDirection.wdGoToNext;
                            object gotoCount = null;
                            object gotoName = (currentpage + page + 2).ToString();
                            oWord.Selection.GoTo(ref gotoPage, ref gotoNext, ref gotoCount, ref gotoName);
                        }
                        page++;
                        isizeMax = 2 * isizeMin;
                        for (int obj1 = obj; obj1 < obj + (page < npages ? 10 : Math.Min(10, list_temp.Count % 10)); obj1++)
                        {
                            Object o1 = list_temp[obj1];
                            int fragSize = 0;
                            foreach (Band b in o1.md.chromo_bands)
                            {
                                fragSize += b.Size;
                            }
                            if (isizeMax < fragSize)
                                isizeMax = fragSize;
                        }
                    }
                    Object o = list_temp[obj];
                    int Xtrans = 100;
                    for (int j = 0; j <= obj; j++)
                    {
                        if (j % 10 == 0)
                            Xtrans = 100;
                        else
                        {
                            if (j % 2 == 0)
                                Xtrans += 50;
                            else
                                Xtrans += 40;
                        }
                    }
                    int Ytrans = 250;
                    int isize_old = 0;
                    foreach (Band b in o.md.chromo_bands)
                    {
                        Ytrans += isize_old;
                        int isize = Convert.ToInt32(300.0 * Convert.ToDouble(b.Size) / Convert.ToDouble(isizeMax));
                        isize_old = isize;
                        Word.Shape bandShape = oDocument.Shapes.AddShape((o.md.f_e.Count == 0 ? 18 : 13), Xtrans, Ytrans, 30, (o.md.f_e.Count == 0 ? 2 * isize : isize), ref missing); // bands and rings or just rings
                        bandShape.Fill.Solid();
                        if (o.md.O_type == MetaData.ObjectType.fully_repaired) // these need to be counted as intact chromosomes
                            bandShape.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Magenta);
                        else
                            bandShape.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(b.Color);
                        if (o.md.f_e.Count == 0)
                            bandShape.TextFrame.TextRange.Text = ChromoLabel(b.Chromo_num) + " b" + b.Size.ToString();
                        else
                            bandShape.TextFrame.TextRange.Text = ChromoLabel(b.Chromo_num) + "\nb" + b.Ordinal_number.ToString();
                        bandShape.TextFrame.TextRange.Font.Size = 10;
                        if (b.Color == System.Drawing.Color.Yellow || o.md.f_e.Count == 0)
                            if (o.md.O_type != MetaData.ObjectType.fully_repaired)
                                bandShape.TextFrame.TextRange.Font.Color = Word.WdColor.wdColorBlack; // for contrast
                        foreach (Centromere c in o.md.c_l)
                        {
                            int pos = (b.Upstream_end_position - c.Position) / 1000;
                            //Microsoft.Office.Interop.Word.Shape centromereShape = oDocument.Shapes.AddShape(6, Xtrans, Ytrans, 30, Ytrans + pos, ref missing); // centromere // which centromere?
                        }
                    }
                }
                currentpage += npages;
            }
            catch
            {
                MessageBox.Show("Generation of graphics has failed!");
            }
            return currentpage;
        }
        //
        public static void InsertIntactChromos(int i, Word.Document oDocument)
        {
            object missing = System.Reflection.Missing.Value;
            int isize = Convert.ToInt32(Convert.ToDouble(IntactHumanGenome.NC[i]) / IntactHumanGenome.monomerSize) / 800;
            int Xtrans = 100;
            for (int j = 0; j <= i; j++)
            {
                if (j % 10 == 0)
                    Xtrans = 100;
                else
                {
                    if (j % 2 == 0)
                        Xtrans += 50;
                    else
                        Xtrans += 40;
                }
            }
            int Ytrans = 200;
            switch (i / 10)
            {
                case 1:
                    Ytrans = 200 + 180;
                    break;
                case 2:
                    Ytrans = 200 + 180 + 120;
                    break;
                case 3:
                    Ytrans = 200 + 180 + 120 + 100;
                    break;
                case 4:
                    Ytrans = 200 + 180 + 120 + 100 + 80;
                    break;
                default:
                    break;
            }
            Word.Shape chromoShape = oDocument.Shapes.AddShape(13, Xtrans, Ytrans, 30, isize, ref missing); // chromosome           
            chromoShape.TextFrame.TextRange.Text = ChromoLabel(i);
            switch (i)
            {
                case (1 - 1):
                case (2 - 1):
                    {
                        chromoShape.Fill.Solid();
                        chromoShape.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
                    }
                    break;
                case (3 - 1):
                case (4 - 1):
                    {
                        chromoShape.Fill.Solid();
                        chromoShape.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Green);
                    }
                    break;
                case (7 - 1):
                case (8 - 1):
                    {
                        chromoShape.Fill.Solid();
                        chromoShape.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Yellow);
                        chromoShape.TextFrame.TextRange.Font.Color = Word.WdColor.wdColorBlack; // for contrast
                    }
                    break;
                default:
                    {
                        chromoShape.Fill.Solid();
                        chromoShape.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.DarkBlue);
                    }
                    break;
            }
        }
        //
        public static void InsertIntactFragment(Word.Document oDocument, List<Object> listObjs)
        {
            object missing = System.Reflection.Missing.Value;
            int i = 0;
            foreach (Object o in listObjs)
            {
                if (o.md.O_type == MetaData.ObjectType.intact_chromo)
                {
                    int Xtrans = 100;
                    for (int j = 0; j <= i; j++)
                    {
                        if (j % 10 == 0)
                            Xtrans = 100;
                        else
                        {
                            if (j % 2 == 0)
                                Xtrans += 50;
                            else
                                Xtrans += 40;
                        }
                    }
                    int Ytrans = 200;
                    switch (i / 10)
                    {
                        case 1:
                            Ytrans = 200 + 180;
                            break;
                        case 2:
                            Ytrans = 200 + 180 + 120;
                            break;
                        case 3:
                            Ytrans = 200 + 180 + 120 + 100;
                            break;
                        case 4:
                            Ytrans = 200 + 180 + 120 + 100 + 80;
                            break;
                        default:
                            break;
                    }
                    foreach (Band b in o.md.chromo_bands)
                    {
                        int isize = b.Size / 1000;
                        Word.Shape bandShape = oDocument.Shapes.AddShape(13, Xtrans, Ytrans, 30, isize, ref missing); // bands and rings
                        bandShape.Fill.Solid();
                        bandShape.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(b.Color);
                        bandShape.TextFrame.TextRange.Text = ChromoLabel(b.Chromo_num) + "\nb" + b.Ordinal_number.ToString();
                        bandShape.TextFrame.TextRange.Font.Size = 10;
                        if (b.Color == System.Drawing.Color.Yellow)
                            bandShape.TextFrame.TextRange.Font.Color = Word.WdColor.wdColorBlack; // for contrast
                        foreach (Centromere c in o.md.c_l)
                        {
                            int pos = (b.Upstream_end_position - c.Position) / 1000;
                            //Microsoft.Office.Interop.Word.Shape centromereShape = oDocument.Shapes.AddShape(6, Xtrans, Ytrans, 30, Ytrans + pos, ref missing); // centromere // which centromere?
                        }
                        Ytrans += isize;
                    }
                    i++;
                }
            }
        }
    }
}
