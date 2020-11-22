using System.Collections.Generic;

namespace GraficDisplay
{
    internal partial class Object : Genome
    {
        public bool FillObj(Object o_new, Object o, DSBs.DSBstruct dsb, PositionWithRespectToReferencePoint relative_location) // add attributes to an empty object
        {
            try
            {
                o_new.md = new MetaData
                {
                    chromo_bands = new LinkedList<Band>()
                };
                if (relative_location == PositionWithRespectToReferencePoint.downstream) // downstream from the DSB
                {
                    for (int i = 0; i < o.md.f_e.Count; i++)
                    {
                        if (o.md.f_e[i].Position <= dsb.position)
                        {
                            o_new.md.f_e.Add(new Free_end()
                            {
                                Position = o.md.f_e[i].Position,
                                FE_type = o.md.f_e[i].FE_type,
                                Reacting = o.md.f_e[i].Reacting,
                                L = o.md.f_e[i].L, // input X, Y, Z, location of a free end, only if it can interact with other free ends                            
                                Relative_position = PositionWithRespectToReferencePoint.not_known   // since this is a new fragment, there is no point of reference yet for this reactive end; this var. is always for later use 
                            });
                            foreach (Band b in o.md.chromo_bands)
                            {
                                if (!(b.Position_within_object <= dsb.position - o.md.f_e[i].Position && b.Position_within_object + b.Size >= dsb.position - o.md.f_e[i].Position))
                                {
                                    var b1 = (Band)b.MemberwiseClone();
                                    o_new.md.chromo_bands.AddLast(b1);
                                }
                                else
                                {
                                    var b1 = (Band)b.MemberwiseClone();
                                    o_new.md.chromo_bands.AddLast(b1);
                                    o_new.md.chromo_bands.Last.Value.Size = dsb.position - o.md.f_e[i].Position - b.Position_within_object;
                                    o_new.md.chromo_bands.Last.Value.Upstream_end_position = dsb.position;
                                    break;
                                }
                            }
                            o_new.NumberOfBands = o_new.md.chromo_bands.Count;
                            o_new.Length = dsb.position - o.md.f_e[i].Position;
                        }
                    }
                    for (int i = 0; i < o.md.c_l.Count; i++)
                        if (o.md.c_l[i].Position < dsb.position)
                            o_new.md.c_l.Add(new Centromere() { Position = o.md.c_l[i].Position, Relative_position = PositionWithRespectToReferencePoint.not_known });
                    o_new.md.f_e.Add(new Free_end()
                    {
                        Position = dsb.position,
                        FE_type = MetaData.FreeEndType.reactive,
                        Reacting = false,
                        L = dsb.L,
                        Relative_position = PositionWithRespectToReferencePoint.not_known // TBD later
                        // since this is a new fragment, there is no point of reference (like a new DSB) yet for this reactive end; this var. is always for later use 
                    });
                    if (o_new.md.f_e[0].FE_type == MetaData.FreeEndType.reactive)
                        o_new.md.O_type = MetaData.ObjectType.open_fragement;
                    else
                        o_new.md.O_type = MetaData.ObjectType.frag_with_one_telomere;
                }
                else
                {
                    for (int i = 0; i < o.md.f_e.Count; i++)
                    {
                        if (o.md.f_e[i].Position >= dsb.position)
                        {
                            o_new.md.f_e.Add(new Free_end()
                            {
                                Position = o.md.f_e[i].Position,
                                FE_type = o.md.f_e[i].FE_type,
                                Reacting = o.md.f_e[i].Reacting,
                                L = o.md.f_e[i].L,
                                Relative_position = PositionWithRespectToReferencePoint.not_known
                                // since this is a new fragment, there is no point of reference yet for this reactive end; this var. is always for later use 
                            });
                            foreach (Band b in o.md.chromo_bands)
                            {
                                if (!(b.Position_within_object <= o.Length - (o.md.f_e[i].Position - dsb.position) && b.Position_within_object + b.Size >= o.Length - (o.md.f_e[i].Position - dsb.position)))
                                {
                                    var b1 = (Band)b.MemberwiseClone();
                                    o_new.md.chromo_bands.AddLast(b1);
                                    o_new.md.chromo_bands.Last.Value.Position_within_object = b.Position_within_object - (o.Length - (o.md.f_e[i].Position - dsb.position));
                                }
                                else
                                {
                                    var b1 = (Band)b.MemberwiseClone();
                                    o_new.md.chromo_bands.AddFirst(b1); // first, add a band to the new obj., then adjust its size and position
                                    o_new.md.chromo_bands.First.Value.Size = b.Size - ((o.Length - b.Position_within_object) - (o.md.f_e[i].Position - dsb.position));
                                    o_new.md.chromo_bands.First.Value.Position_within_object = 0;
                                    o_new.md.chromo_bands.First.Value.Downstream_end_position = dsb.position;
                                    break;
                                }
                            }
                            o_new.NumberOfBands = o_new.md.chromo_bands.Count;
                            o_new.Length = -dsb.position + o.md.f_e[i].Position;
                        }
                    }
                    for (int i = 0; i < o.md.c_l.Count; i++)
                        if (o.md.c_l[i].Position >= dsb.position)
                            o_new.md.c_l.Add(new Centromere() { Position = o.md.c_l[i].Position, Relative_position = PositionWithRespectToReferencePoint.not_known });
                    o_new.md.f_e.Add(new Free_end()
                    {
                        Position = dsb.position,
                        FE_type = MetaData.FreeEndType.reactive,
                        Reacting = false,
                        L = dsb.L,
                        Relative_position = PositionWithRespectToReferencePoint.not_known // TBD later
                        // since this is a new fragment, there is no point of reference (like a new DSB) yet for this reactive end; this var. is always for later use 
                    });
                    if (o_new.md.f_e[0].FE_type == MetaData.FreeEndType.reactive)
                        o_new.md.O_type = MetaData.ObjectType.open_fragement;
                    else
                        o_new.md.O_type = MetaData.ObjectType.frag_with_one_telomere;
                }
                foreach (Band b in o_new.md.chromo_bands)
                    b.Size = b.Upstream_end_position - b.Downstream_end_position;
                int bsize = 0;
                foreach (Band b in o_new.md.chromo_bands)
                {
                    b.Position_within_object += bsize;
                    bsize = b.Size;
                }
                if (o_new.md.f_e.Count == 2)
                    return true;
                else
                    return false; // >= and <= should be for all conditions related  to a DSB, and >= and < for a centromere  
            }
            catch
            {
                return false;
            }
        }
    }
}