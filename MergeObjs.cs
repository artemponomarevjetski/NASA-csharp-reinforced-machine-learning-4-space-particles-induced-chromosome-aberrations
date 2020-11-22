using System.Collections.Generic;
using System.Windows.Forms;
using System;

namespace GraficDisplay
{
    internal partial class TimeOperator : Genome
    {
        public bool CreateMergedObj(Object o_new, List<Object> listObjs)
        {
            try
            {
                Object o1 = null, o2 = null;
                foreach (Object o in listObjs)
                {
                    foreach (Free_end fe1 in o.md.f_e)
                    {
                        if (fe1.Reacting)
                        {
                            if (o1 == null)
                                o1 = o;
                            else
                                o2 = o;
                        }
                    }
                }
                if (o1 == o2) // ring condition            
                { // a ring can't have free ends
                    listObjs.Remove(o2); // delete =1 obj. from the list                   
                    o1.md.f_e.Clear();
                    o1.md.O_type = MetaData.ObjectType.ring;
                    listObjs.Add(o1);
                    return false; // no 2 obj's were merged
                }
                else
                {
                    // transfer non-reacting fe's
                    o_new.Length = o1.Length + o2.Length;
                    o_new.md = new MetaData();
                    foreach (Free_end fe in o1.md.f_e)
                    {
                        if (fe.Reacting == false)
                            o_new.md.f_e.Add(new Free_end()
                            {
                                Position = fe.Position,
                                FE_type = fe.FE_type,
                                Reacting = fe.Reacting,
                                Relative_position = PositionWithRespectToReferencePoint.not_known // in the new fragment it's unknown; because of possible inverions TBD later
                            });
                    }
                    foreach (Free_end fe in o2.md.f_e)
                    {
                        if (fe.Reacting == false)
                            o_new.md.f_e.Add(new Free_end()
                            {
                                Position = fe.Position,
                                FE_type = fe.FE_type,
                                Reacting = fe.Reacting,
                                Relative_position = PositionWithRespectToReferencePoint.not_known // in the new fragment it's unknown; because of possible inverions TBD later
                            });
                    }
                    // transfer centromeres                
                    foreach (Centromere c in o1.md.c_l)
                        o_new.md.c_l.Add(new Centromere()
                        {
                            Position = c.Position,
                            Relative_position = PositionWithRespectToReferencePoint.not_known // TBD later
                        });
                    foreach (Centromere c in o2.md.c_l)
                        o_new.md.c_l.Add(new Centromere()
                        {
                            Position = c.Position,
                            Relative_position = PositionWithRespectToReferencePoint.not_known // TBD later
                        });
                    // stitch the  bands together 
                    var b1 = new Band();
                    var b2 = new Band();
                    int pos1 = int.MaxValue, pos2 = int.MinValue;
                    foreach (Band b in o1.md.chromo_bands)
                        foreach (Free_end fe in o1.md.f_e)
                            if (fe.Reacting)
                                if (b.Downstream_end_position == fe.Position || b.Upstream_end_position == fe.Position)
                                {
                                    b1 = b;
                                    pos1 = fe.Position;
                                }
                    foreach (Band b in o2.md.chromo_bands)
                        foreach (Free_end fe in o2.md.f_e)
                            if (fe.Reacting)
                                if (b.Downstream_end_position == fe.Position || b.Upstream_end_position == fe.Position)
                                {
                                    b2 = b;
                                    pos2 = fe.Position;
                                }
                    if (b1.Chromo_num == b2.Chromo_num && pos1 == pos2)
                    {
                        int end1 = 0, end2 = 0;
                        foreach (Band b in o1.md.chromo_bands)
                            foreach (Free_end fe in o1.md.f_e)
                                if (fe.Reacting)
                                {
                                    if (b.Downstream_end_position == fe.Position)
                                        end1 = b.Upstream_end_position;
                                    else
                                        end1 = b.Downstream_end_position;
                                }
                        foreach (Band b in o2.md.chromo_bands)
                            foreach (Free_end fe in o2.md.f_e)
                                if (fe.Reacting)
                                {
                                    if (b.Downstream_end_position == fe.Position)
                                        end2 = b.Upstream_end_position;
                                    else
                                        end2 = b.Downstream_end_position;
                                }
                        var b3 = new Band
                        {
                            Length = b1.Length + b2.Length,
                            Chromo_num = b1.Chromo_num,
                            Color = b1.Color,
                            Upstream_end_position = Math.Max(end1, end2),
                            Downstream_end_position = Math.Min(end1, end2)
                        };
                        if (o1.md.chromo_bands.Last.Value == b1)
                        {
                            o1.md.chromo_bands.Remove(b1);
                            foreach (Band b in o1.md.chromo_bands)
                                o_new.md.chromo_bands.AddLast(b);
                            o_new.md.chromo_bands.AddLast(b3);
                            o2.md.chromo_bands.Remove(b2);
                            foreach (Band b in o2.md.chromo_bands)
                                o_new.md.chromo_bands.AddLast(b);
                        }
                        else
                        {
                            o2.md.chromo_bands.Remove(b2);
                            foreach (Band b in o2.md.chromo_bands)
                                o_new.md.chromo_bands.AddFirst(b);
                            o_new.md.chromo_bands.AddFirst(b3);
                            o1.md.chromo_bands.Remove(b1);
                            foreach (Band b in o1.md.chromo_bands)
                                o_new.md.chromo_bands.AddFirst(b);
                        }
                    }
                    else
                    {
                        if (o1.md.chromo_bands.Last.Value == b1)
                        {
                            foreach (Band b in o1.md.chromo_bands)
                                o_new.md.chromo_bands.AddLast(b);
                            foreach (Band b in o2.md.chromo_bands)
                                o_new.md.chromo_bands.AddLast(b);
                        }
                        else
                        {
                            foreach (Band b in o2.md.chromo_bands)
                                o_new.md.chromo_bands.AddFirst(b);
                            foreach (Band b in o1.md.chromo_bands)
                                o_new.md.chromo_bands.AddFirst(b);
                        }
                    }
                    if (o_new.md.chromo_bands.Count == 1 && o_new.md.f_e[0].FE_type == MetaData.FreeEndType.telomeric && o_new.md.f_e[1].FE_type == MetaData.FreeEndType.telomeric)
                        o_new.md.O_type = MetaData.ObjectType.fully_repaired;
                    else
                        o_new.md.O_type = MetaData.ObjectType.linear;
                    o_new.NumberOfBands = o_new.md.chromo_bands.Count; // the merged obj, is ready
                    listObjs.Remove(o1); // delete 2 obj.s from the list
                    listObjs.Remove(o2);
                    foreach (Band b in o_new.md.chromo_bands)
                        b.Size = b.Upstream_end_position - b.Downstream_end_position;
                    int bsize = 0;
                    foreach (Band b in o_new.md.chromo_bands)
                    {
                        b.Position_within_object += bsize;
                        bsize = b.Size;
                    }
                    return true;
                }
            }
            catch
            {
                MessageBox.Show("Merge failure!");
                return false;
            }
        }
    }
}