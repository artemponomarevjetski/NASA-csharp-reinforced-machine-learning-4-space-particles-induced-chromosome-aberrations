using System.Windows.Forms;

namespace GraficDisplay
{
    public partial class MainForm : Form
    {
        public partial class Radiation
        {
            public void OutputToPPT()
            {
                Microsoft.Office.Interop.PowerPoint.Application pptApplication = new Microsoft.Office.Interop.PowerPoint.Application();
                Microsoft.Office.Interop.PowerPoint.Slides oSlides;
                Microsoft.Office.Interop.PowerPoint._Slide oSlide;
                Microsoft.Office.Interop.PowerPoint.TextRange objText;

                // Create the Presentation File
                Microsoft.Office.Interop.PowerPoint.Presentation pptPresentation = pptApplication.Presentations.Add(Microsoft.Office.Core.MsoTriState.msoTrue);
                Microsoft.Office.Interop.PowerPoint.CustomLayout customLayout = pptPresentation.SlideMaster.CustomLayouts[Microsoft.Office.Interop.PowerPoint.PpSlideLayout.ppLayoutText];

                // Create new Slide
                oSlides = pptPresentation.Slides;
                oSlide = oSlides.AddSlide(1, customLayout);

                // Add title
                objText = oSlide.Shapes[1].TextFrame.TextRange;
                objText.Text = "Radiation Shield";
                objText.Font.Name = "Arial";
                objText.Font.Size = 32;

                objText = oSlide.Shapes[2].TextFrame.TextRange;
                objText.Text = "ICA report\nArtem L. Ponomarev,. Ph.D.\nOn the Fig. is the proposed \ncompound shield\nRed -- Hafnium\nGreen -- Aluminum\nGrey -- plastic\n\nPlay video to visualize passage of space particles through Hafnium";

                string appPath = Application.StartupPath;
                string pictureFileName = appPath + "\\temp\\compoundshield2.jpg";
                Microsoft.Office.Interop.PowerPoint.Shape oShape = oSlide.Shapes[2];
                oSlide.Shapes.AddPicture(pictureFileName, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoTrue,
                    oShape.Left + 394, oShape.Top - 140, oShape.Width / 2, oShape.Height);
                oSlide.NotesPage.Shapes[2].TextFrame.TextRange.Text = "Artem L. Ponomarev, Ph.D.";
                string movieFileName = appPath + "\\temp\\MovingParticles.avi";
                oSlide.Shapes.AddMediaObject2(movieFileName, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoTrue,
               oShape.Left + 450, oShape.Top + 120, oShape.Width / 2, oShape.Height / 2);
                ////While iterating through all slides i:
                //objShapes = objPres.Slides[i].Shapes;
                //foreach (Microsoft.Office.Interop.PowerPoint.Shape s in objShapes)
                //{
                //    if (s.Name.Contains(".wmv"))
                //    {
                //        s.AnimationSettings.PlaySettings.PlayOnEntry = MsoTriState.msoTrue;
                //    }
                //}

                // Create new Slide
                oSlides = pptPresentation.Slides;
                oSlide = oSlides.AddSlide(1, customLayout);

                // Add title
                objText = oSlide.Shapes[1].TextFrame.TextRange;
                objText.Text = "Radiation Shield";
                objText.Font.Name = "Arial";
                objText.Font.Size = 32;

                // insert shape           
                Microsoft.Office.Interop.PowerPoint.Shape shieldShape1 = oSlide.Shapes.AddShape(Microsoft.Office.Core.MsoAutoShapeType.msoShapeCube, oShape.Left + 100, oShape.Top, oShape.Width / 10, oShape.Height);
                shieldShape1.Fill.ForeColor.RGB = System.Drawing.Color.Blue.ToArgb();
                shieldShape1.TextFrame.TextRange.Text = "Hafnium";
                Microsoft.Office.Interop.PowerPoint.Shape shieldShape2 = oSlide.Shapes.AddShape(Microsoft.Office.Core.MsoAutoShapeType.msoShapeCube, oShape.Left + 300, oShape.Top, oShape.Width / 10, oShape.Height);
                shieldShape2.Fill.ForeColor.RGB = System.Drawing.Color.Green.ToArgb();
                shieldShape2.TextFrame.TextRange.Text = "Aluminum";
                Microsoft.Office.Interop.PowerPoint.Shape shieldShape3 = oSlide.Shapes.AddShape(Microsoft.Office.Core.MsoAutoShapeType.msoShapeCube, oShape.Left + 200, oShape.Top, oShape.Width / 10, oShape.Height);
                shieldShape3.Fill.ForeColor.RGB = System.Drawing.Color.Purple.ToArgb();
                shieldShape3.TextFrame.TextRange.Text = "Zirconium";
                //
                // Create new Slide
                oSlides = pptPresentation.Slides;
                oSlide = oSlides.AddSlide(1, customLayout);

                // Add title
                objText = oSlide.Shapes[1].TextFrame.TextRange;
                objText.Text = "Radiation Shield";
                objText.Font.Name = "Arial";
                objText.Font.Size = 32;
                //
                Microsoft.Office.Interop.PowerPoint.Shape oShape1;
                int iRow;
                int iColumn;

                int[,] tdata = new int[,] { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 } };

                int iRows = tdata.GetLength(0);
                int iColumns = tdata.GetLength(1);

                oShape1 = oSlide.Shapes.AddTable(iRows, iColumns, 500, 110, 160, 120);

                for (iRow = 1; iRow <= oShape1.Table.Rows.Count; iRow++)
                {
                    for (iColumn = 1; iColumn <= oShape1.Table.Columns.Count; iColumn++)
                    {
                        oShape1.Table.Cell(iRow, iColumn).Shape.TextFrame.TextRange.Text = tdata.GetValue(iRow - 1, iColumn - 1).ToString();
                        oShape1.Table.Cell(iRow, iColumn).Shape.TextFrame.TextRange.Font.Name = "Verdana";
                        oShape1.Table.Cell(iRow, iColumn).Shape.TextFrame.TextRange.Font.Size = 8;
                    }
                }
                //
                // Create new Slide
                oSlides = pptPresentation.Slides;
                oSlide = oSlides.AddSlide(1, customLayout);

                // Add title
                objText = oSlide.Shapes[1].TextFrame.TextRange;
                objText.Text = "Radiation Shield";
                objText.Font.Name = "Arial";
                objText.Font.Size = 32;
                //
                pptPresentation.SaveAs(appPath + "\\temp\\RadShield.pptx", Microsoft.Office.Interop.PowerPoint.PpSaveAsFileType.ppSaveAsDefault, Microsoft.Office.Core.MsoTriState.msoTrue);
                //
                //pptPresentation.Close();
                //pptApplication.Quit();
            }
        }
    }
}