using System;

namespace GraficDisplay
{
    internal partial class Object : Genome
    {
        public Object(int chromonumber) // obj = intact chromo
        {
            try
            {
                Length = Convert.ToInt32(IntactHumanGenome.NC[chromonumber] / IntactHumanGenome.monomerSize);
                md = new MetaData();
                int itemp = Convert.ToInt32(IntactHumanGenome.CM[0] / IntactHumanGenome.monomerSize);
                for (int i = 0; i < chromonumber; i++)
                    itemp += Convert.ToInt32(IntactHumanGenome.CM[chromonumber] / IntactHumanGenome.monomerSize);
                md.c_l.Add(new Centromere() { Position = itemp, Relative_position = PositionWithRespectToReferencePoint.not_known });
                md.f_e.Add(new Free_end()
                {
                    Position = Convert.ToInt32(IntactHumanGenome.DownstreamEndPosition(chromonumber) / IntactHumanGenome.monomerSize),
                    FE_type = MetaData.FreeEndType.telomeric,
                    Reacting = false,
                    Relative_position = PositionWithRespectToReferencePoint.not_known // TBD later
                    // relative_location has to be defined with respect to some point of reference, like a DSB; this var. will used later in the algo
                });
                md.f_e.Add(new Free_end()
                {
                    Position = Convert.ToInt32(IntactHumanGenome.DownstreamEndPosition(chromonumber) / IntactHumanGenome.monomerSize) + Length,
                    FE_type = MetaData.FreeEndType.telomeric,
                    Reacting = false,
                    Relative_position = PositionWithRespectToReferencePoint.not_known
                    // relative_location has to be defined with respect to some point of reference, like a DSB, or a CM; this var. will used later in the algo
                });
                md.O_type = MetaData.ObjectType.intact_chromo;
                md.chromo_bands.AddLast(new Band()
                {
                    Size = Length,
                    Position_within_object = 0,
                    Chromo_num = chromonumber,
                    Downstream_end_position = md.f_e[0].Position,
                    Upstream_end_position = md.f_e[1].Position
                });
                NumberOfBands = md.chromo_bands.Count;
                foreach (Band b in md.chromo_bands)
                {
                    switch (chromonumber)
                    {
                        case (1 - 1):
                        case (2 - 1):
                            {
                                b.Color = System.Drawing.Color.Red;
                            }
                            break;
                        case (3 - 1):
                        case (4 - 1):
                            {
                                b.Color = System.Drawing.Color.Green;
                            }
                            break;
                        case (7 - 1):
                        case (8 - 1):
                            {
                                b.Color = System.Drawing.Color.Yellow;
                            }
                            break;
                        default:
                            {
                                b.Color = System.Drawing.Color.DarkBlue;
                            }
                            break;
                    }
                }
            }
            catch
            {
                md = null; // if normal object definition is not possible, the MetaData are null
            }
        }
    }
}