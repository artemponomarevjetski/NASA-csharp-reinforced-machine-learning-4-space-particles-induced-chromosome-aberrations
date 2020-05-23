using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GraficDisplay
{
    public struct Location
    {
        public int X, Y, Z;
        public Location(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    };
    internal partial class Genome
    {
        public List<Object> listObjs; // this list is unordered, because any two objects can interact in a pair-wise fashion and any object can produce more objects
        public int nObjects;
        public enum PositionWithRespectToReferencePoint { downstream, upstream, not_known }
        // this TBD during run-time; location has to be defined with respect to some point of reference, like a DSB; this var. will used later in the algo
        public Genome()
        {
            listObjs = new List<Object>();
            nObjects = IntactHumanGenome.nObjs;
        }
        //
        public void Initialize()
        {
            for (int i = 0; i < nObjects; i++)
            {
                listObjs.Add(new Object(i));
            }
        }
        //
        public void Do()
        {
            RadiationSchema rs = new RadiationSchema();
            MonteCarloSimlation(rs);
        }
        //
        public void MonteCarloSimlation(RadiationSchema rs)
        {
            for (int i = 0; i < rs.MChistories; i++)
            {
                TimeOperator timeOperator = new TimeOperator();
                switch (RadiationSchema.rad_repair_action) // enable only DNA breakage
                {
                    case 0:
                        {
                            timeOperator.DoTimeEvolution(rs, listObjs); // recursion creates time loop 
                        }
                        break;
                        //    case 1:
                        //        {
                        ////            timeOperator.ApplyBreakage(rs, listObjs);
                        //        }
                        //        break;
                        //    default:
                        //        {
                        // //           timeOperator.ApplyRadAndRepair(rs, listObjs);
                        //        }
                        //        break;
                }
            }
        }
        //
        public void Finish()
        {
            MessageBox.Show("Generating graphics...");
            if (!PostProcess())
            {
                MessageBox.Show("Failed to post-process!");
            }

            OrginizeDataRepository();
            SendNotifications();
            Visualize.CreateWordDoc(listObjs, "Incremental step in the final state analysis: \n");
            //Visualize.CreateWordDoc(listObjs, "After radiation: \n"); 
            MessageBox.Show("End Of Chronic Exposure!");
            Application.Exit();
        }
        //
        public static void SendNotifications() { }
        //    
        public static bool PostProcess() { return true; }
        //
        public static void OrginizeDataRepository() { }
    }
    //
    internal class MetaData : Object
    {
        public List<Centromere> c_l;
        public List<Free_end> f_e; // not important which free end to count as "first"
        public LinkedList<Band> chromo_bands; // this one is an ordered list, since bands are consecutive in an object
        public ObjectType O_type { get; set; }
        //      
        public enum FreeEndType
        {
            not_known,
            non_reactive,
            repaired_in_lin_obj,
            repaired_in_ring,
            telomeric,
            reactive
        }
        public enum ObjectType // in ABC order
        {
            not_known, // this "type" is the default (when there is no initialization)
            acentric_ring,
            capped_fragment,
            centric_ring,
            color_junction,
            complete_exchange,
            complex_exchange,
            deletion,
            dicentric,
            frag_with_one_telomere,
            fragment_with_middle_inversion,
            fully_repaired,
            incomplete_exchange,
            intact_chromo,
            interstitial_aberration,
            intrastitial_aberration,
            inverison,
            linear,
            open_fragement,
            pericentric,
            polycentric,
            ring,
            simple_exchange,
            terminal_deletion
        }
        public MetaData()
        {
            c_l = new List<Centromere>();
            f_e = new List<Free_end>();
            chromo_bands = new LinkedList<Band>();
        }
    }
    internal class Free_end : MetaData // these free ends are object descriptors
    {
        public int Position { get; set; } // a physical location of a point (a locus point) in the genome that does not change during breakage and recombination;
        // it is measured in monomers, kbps, or Mbps
        public FreeEndType FE_type { get; set; }
        public bool Reacting { get; set; }
        public PositionWithRespectToReferencePoint Relative_position { get; set; }
        public Location L { get; set; } // Location refers to a point in the Euclidian (X, Y, Z) space and is measured in lattice sites or microns        
    }
    internal class Centromere : MetaData
    {
        public int Position { get; set; }
        public PositionWithRespectToReferencePoint Relative_position { get; set; }
    }
    internal class Band : MetaData
    {
        public int Ordinal_number { get; set; }
        public int Size { get; set; }
        public int Position_within_object { get; set; }
        public int Chromo_num { get; set; } // a band is always made of one chromosome material 
        public int Downstream_end_position { get; set; } // because a band is an intact piece of chromosome, it has a definite orientation and definite 3' and 5' ends
        public int Upstream_end_position { get; set; } // a band cannot have some other piece inside, otherwise it will be a set of bands
        // a band end can be a fragment end, or be next to another band end
        // two bands maybe adjacent, if a DSB between them got repaired
        // a collection of bands within a fragment has to be a LinkedList
        public System.Drawing.Color Color { get; set; }
    }
    internal class HPRT_gene : MetaData
    {
        // future research
    }
    //
    internal partial class Object : Genome
    {
        public MetaData md;
        public int Length { get; set; }
        public int NumberOfBands { get; set; }
        public Object() { } // empty obj
    }
    //
    internal class RadiationSchema : Genome
    {
        public static readonly double timeFractionation = 0.1; // 1 hr for one radiation installment
        public static readonly int rad_repair_action = 0; // 1 is DNA breakage only
        public static readonly int max_nIonSpecies = 1000; // up to 1,000 ion species 
        public int[] nIonSpecies = new int[max_nIonSpecies]; // up to 1,000 particle energy bins
        public static readonly int max_nIonEnergySpectrum = 1000;
        public static readonly double[] nEnergyPoints = new double[max_nIonEnergySpectrum];
        public static readonly double Pr = 5.0; // penumbra radius microns 
        //
        public int MChistories;
        public int nBeams;
        public double[] entryTime;
        public double[] exitTime;
        public double[] D; // Gy
        public MainForm.Radiation.p_type[] pt;
        //
        public RadiationSchema()
        {
            DefaultBeam(); // before XML or JSON input becomes available
        }
        public void DefaultBeam()
        {
            MChistories = 1; // introduce GUI
            nBeams = 1; // introduce GUI
            entryTime = new double[nBeams];
            exitTime = new double[nBeams];
            D = new double[nBeams];
            pt = new MainForm.Radiation.p_type[nBeams];
            for (int i = 0; i < nBeams; i++)
            {
                switch (i)
                {
                    case 0:
                        {
                            pt[i] = MainForm.Radiation.p_type.Si;
                        }
                        break;
                    default:
                        {
                            pt[i] = MainForm.Radiation.p_type.Fe;
                        }
                        break;
                }
                D[i] = 3.0 / Convert.ToDouble(nBeams); // Gy, input from GUI  
                entryTime[i] = i * timeFractionation;
                exitTime[i] = (i + 1) * timeFractionation; // so, effectively, the total beam lasts 10 hrs
            }
        }
        public void ReferenceBeam() { }
        public void LoadFromXmlFile() { }
    }
    //
    internal class DSBs : Genome
    {
        public static readonly double[] DSBcomplP = { 0.1, 0.8, 0.1 };
        public List<DSBstruct> listDSBs = new List<DSBstruct>();
        public double P = 1.0;
        public DSBs() { }
        public DSBs(string s)
        {
            Random random = new Random();
            for (int i = 0; i < 20; i++)
            {
                P = DSBcomplP[1];
                double R = 6.0;
                double lattice_dim = 0.02;
                int rnucleus = Convert.ToInt32(R / lattice_dim);
                Location l;
                while (true)
                {
                    l.X = random.Next(0, 2 * rnucleus) - rnucleus;
                    l.Y = random.Next(0, 2 * rnucleus) - rnucleus;
                    l.Z = random.Next(0, 2 * rnucleus) - rnucleus;
                    double a = 1.0 / 2.0, b = 1.0 / 2.0, c = 4.0; // nucleus parameters for elliptic nucleus // GUI input                
                    if (a * (l.X - rnucleus) * (l.X - rnucleus) + b * (l.Y - rnucleus) * (l.Y - rnucleus) + c * (l.Z - rnucleus) * (l.Z - rnucleus) < rnucleus * rnucleus) // elliptic nucleus            
                    {
                        break;
                    }
                }
                DSBstruct random_DSB = new DSBstruct()
                {
                    ndsb = i,
                    random_time = 0.0, //  SimpleRNG.GetUniform(), // hrs
                    position = random.Next(0, IntactHumanGenome.WholeGenome()),
                    L = l
                };
                listDSBs.Add(random_DSB);
            }
        }
        public struct DSBstruct
        {
            public enum DSBcomplexity { simpleDSB, DSBplus, DSBplusplus }
            public DSBcomplexity DSBcmplx;
            public double entry_time, exit_time, random_time;
            public int ndsb, position;
            public Location L;
            public DSBstruct(int n, int p, Location l, double en, double ex, double rt, DSBcomplexity c)
            {
                L = l;
                ndsb = n;
                position = p;
                entry_time = en; exit_time = ex; random_time = rt;
                DSBcmplx = c;
            }
        };
    }
    //
    internal partial class TimeOperator : Genome
    {
        public DSBs DSBdata; // DSBdata contains all DSBs from all beams (that can have many particle types and times) and their time evolution;
                             // this object contains plenty of information, 
                             // including when DSBs were created, their complexity, position in the genome and XYZ location; technically, it's possible to know what beam they came from and during which
                             // time interval they were created 
        public NASARTI_original DNAbreakage;  // call the old NASARTI model (albeit a complete re-write in C#) with RWs, amorphous tracks and DNA breakage    
        public RadiationSchema rs;
        //      
        public static readonly double P = 0.8; // rejoining probability            
        public static readonly double expTime = 24.0; // hrs 
        public readonly double tau = expTime / 10000.0; // hrs
        public readonly double Z = 50.0; // get it from fits to the experiment; Z parameter in the distance exp.
        public readonly double sigma2 = 2.0e3; //  get it from fits to the experiment; sigma2 parameter in the distance exp.                                              
        //
        public TimeOperator()
        {
            rs = new RadiationSchema();
            DNAbreakage = new NASARTI_original(rs);
            DSBdata = new DSBs();
        }
        //
        public bool DoTimeEvolution(RadiationSchema rs, List<Object> listObjs)
        {
            if (ApplyRadAndRepair(rs, listObjs))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //
        public bool ApplyRadAndRepair(RadiationSchema rs, List<Object> listObjs)
        {
            if (ApplyBreakage(rs))
            {
                if (RestitutionKinetics(listObjs))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        //
        public bool ApplyBreakage(RadiationSchema rs)
        {
            if (ApplyRadiation(rs))
            {
                if (ApplyBystanderEffect())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        //
        public bool ApplyRadiation(RadiationSchema rs)
        {
            if (CreateRadBreaks(rs))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //
        public bool ApplyBystanderEffect()
        {
            if (CreateChemicalBreaks())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //
        public bool ApplyBackgroundDamage() { return true; }
        //
        public bool CreateRadBreaks(RadiationSchema rs)
        {
            //DSBdata = new DSBs("default");
            //return true;
            if (DNAbreakage.PrepairDSBpositionsInGenome(rs, DSBdata))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //
        public bool CreateChemicalBreaks() { return true; }
        //
        public bool SplitObjects(List<Object> listObjs, double time1, double time2)
        {
            try
            {
                foreach (DSBs.DSBstruct dsb in DSBdata.listDSBs)
                {
                    if (dsb.random_time >= time1 && dsb.random_time < time2)
                    {
                        int j = 0;
                        while (j < listObjs.Count)
                        {
                            if (WithinObj(listObjs[j], dsb.position))
                            {
                                Object o1 = new Object();
                                Object o2 = new Object();
                                if (Create2newObjs(o1, o2, listObjs[j], dsb))
                                {
                                    listObjs.Remove(listObjs[j]); // delete o from list                                                          
                                    listObjs.Add(o1); // add o1 and o2 to list    
                                    listObjs.Add(o2);
                                    o1 = o2 = null;
                                    // renumber bands in the whole genome
                                    int[] b_index = new int[46];
                                    foreach (Object o in listObjs)
                                    {
                                        foreach (Band b in o.md.chromo_bands)
                                        {
                                            b_index[b.Chromo_num]++;
                                            b.Ordinal_number = b_index[b.Chromo_num];
                                        }
                                    }
                                }
                                else
                                { // failed to create 2 new frags
                                    if (CheckDSBlist(DSBdata) && CheckObjList(listObjs))
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                break;
                            }
                            j++;
                        }
                    }
                }
            }
            catch
            {
                Detailed_checkObjList(listObjs);
                Detailed_checkDSBlist(DSBdata);
                return false;
            }
            if (CheckDSBlist(DSBdata) && CheckObjList(listObjs))
            {
                return true;
            }
            else
            {
                if (Detailed_checkDSBlist(DSBdata) && Detailed_checkObjList(listObjs))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        //
        public double MisrejoiningProb(Free_end fe1, Free_end fe2)
        {
            double dist2 = (fe1.L.X - fe2.L.X) * (fe1.L.X - fe2.L.X) + (fe1.L.Y - fe2.L.Y) * (fe1.L.Y - fe2.L.Y) + (fe1.L.Z - fe2.L.Z) * (fe1.L.Z - fe2.L.Z); // Eucld. distance between free ends
            double dtemp = 0.08 * (1.0 / Z) * Math.Exp(-dist2 / sigma2); // the probability to misrejoin or create a ring      
            return dtemp;
        }
        //
        public bool CheckDSBlist(DSBs dsbs) // everytime DSBs are created or repaired, this function does some bookkeeping
        {
            //int itemp = 0;
            //foreach (Object o in listObjs)
            //    foreach (free_end fe in o.md.f_e)
            //        if (fe.fe_type == MetaData.freeEndType.reactive)
            //            itemp++;
            //if (itemp != 2 * dsbList.DSBpositions.Count)
            //    return false;
            //else // update this function
            return true;
        }
        public bool Detailed_checkDSBlist(DSBs dsbs) // everytime DSBs are created or repaired, this function does some bookkeeping
        {
            //dsbList.ndsb = dsbList.DSBpositions.Count; 
            return true; // add checks that might return false
        }
        //   
        public bool CheckObjList(List<Object> listObjs)
        {
            int ntelends = 0;
            foreach (Object o in listObjs)
            {
                foreach (Free_end fe in o.md.f_e)
                {
                    if (fe.FE_type == MetaData.FreeEndType.telomeric)
                    {
                        ntelends++;
                    }
                }
            }
            if (ntelends != IntactHumanGenome.nObjs * 2)
            {
                return false;
            }

            foreach (Object o in listObjs)
            {
                if (o.Length <= 0)
                {
                    return false;
                }

                foreach (Band b in o.md.chromo_bands)
                {
                    if (b.Downstream_end_position >= b.Upstream_end_position)
                    {
                        return false;
                    }
                }
            }
            int itemp = 0;
            foreach (Object o in listObjs)
            {
                itemp += o.Length;
            }

            if (itemp != IntactHumanGenome.WholeGenome())
            {
                return false;
            }

            return true;
        }
        //
        public bool Detailed_checkObjList(List<Object> listObjs)
        {
            int ntelends = 0;
            foreach (Object o in listObjs)
            {
                foreach (Free_end fe in o.md.f_e)
                {
                    if (fe.FE_type == MetaData.FreeEndType.telomeric)
                    {
                        ntelends++;
                    }
                }
            }
            if (ntelends != IntactHumanGenome.nObjs * 2)
            {
                return false;
            }

            foreach (Object o in listObjs)
            {
                if (o.Length <= 0)
                {
                    return false;
                }
            }

            int itemp = 0;
            foreach (Object o in listObjs)
            {
                itemp += o.Length;
            }

            if (itemp != IntactHumanGenome.WholeGenome())
            {
                return false;
            }

            foreach (Object o in listObjs)
            {
                if (o.md.f_e.Count < 2 && o.md.O_type != MetaData.ObjectType.ring)
                {
                    return false;
                }
            }

            foreach (Object o in listObjs)
            {
                foreach (Band b in o.md.chromo_bands)
                {
                    if (b.Size == 0)
                    {
                        MessageBox.Show("Band Of 0-size. Do something!");
                    }
                }
            }

            return true;
        }
        //
        public bool WithinObj(Object o, int dsbp)
        {
            if (o.md.f_e.Count == 2)
            {
                if ((dsbp >= o.md.f_e[0].Position && dsbp <= o.md.f_e[1].Position) || (dsbp >= o.md.f_e[1].Position && dsbp <= o.md.f_e[0].Position))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        //
        public bool Create2newObjs(Object o1, Object o2, Object o, DSBs.DSBstruct dsb)
        {
            if (o1.FillObj(o1, o, dsb, PositionWithRespectToReferencePoint.downstream))
            {
                if (o2.FillObj(o2, o, dsb, PositionWithRespectToReferencePoint.upstream))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        //
        public bool RestitutionKinetics(List<Object> listObjs)
        {
            // the class FreeEnds is derived from DSBs class and is based on DSBdata
            // Free_end class is for Objects (which are fragments that can have free ends)
            Random random = new Random();
            try
            {
                for (int i = 0; i < Convert.ToInt32(expTime / tau); i++) // number of elementary time steps
                {
                    if (!SplitObjects(listObjs, i * tau, (i + 1) * tau))
                    {
                        return false; // split only using DSBs avaibale in time interval timeIncrement / tau                  
                    }

                    int availableFreeEnds = 0;
                    foreach (Object o in listObjs)
                    {
                        foreach (Free_end fe in o.md.f_e)
                        {
                            if (fe.FE_type == MetaData.FreeEndType.reactive)
                            {
                                availableFreeEnds++;
                            }
                        }
                    }
                    if (availableFreeEnds != 0)
                    {
                        for (int j = 0; j < availableFreeEnds / 2; j++) // cycle through all pairs of Free Ends in one elementary time step
                        {
                            int availableFreeEnds1 = 0;
                            foreach (Object o in listObjs)
                            {
                                foreach (Free_end fe in o.md.f_e)
                                {
                                    if (fe.FE_type == MetaData.FreeEndType.reactive)
                                    {
                                        availableFreeEnds1++; // determine the number of free ends, which are still reactive
                                    }
                                }
                            }
                            int end1, end2;
                            while (true)
                            {
                                end1 = random.Next(0, availableFreeEnds1);
                                end2 = random.Next(0, availableFreeEnds1);
                                if (end1 != end2)
                                {
                                    break;
                                }
                            }
                            int itemp = 0;
                            Free_end freeend1 = null, freeend2 = null;
                            foreach (Object o in listObjs)
                            {
                                bool b = false;
                                foreach (Free_end fe in o.md.f_e)
                                {
                                    if (fe.FE_type == MetaData.FreeEndType.reactive)
                                    {
                                        if (itemp == end1)
                                        {
                                            fe.Reacting = true;
                                            freeend1 = fe;
                                            b = true;
                                            break;
                                        }
                                        itemp++;
                                    }
                                }
                                if (b)
                                {
                                    break;
                                }
                            }
                            itemp = 0;
                            foreach (Object o in listObjs)
                            {
                                bool b = false;
                                foreach (Free_end fe in o.md.f_e)
                                {
                                    if (fe.FE_type == MetaData.FreeEndType.reactive)
                                    {
                                        if (itemp == end2)
                                        {
                                            fe.Reacting = true; // this marks a free end for rejoining/misrejoining
                                            freeend2 = fe;
                                            b = true;
                                            break;
                                        }
                                        itemp++;
                                    }
                                }
                                if (b)
                                {
                                    break;
                                }
                            }
                            if (freeend1 != null && freeend2 != null)
                            {
                                if (freeend1.Position == freeend2.Position) // proper ends
                                {
                                    if (SimpleRNG.GetUniform() < P)
                                    {
                                        Object o_new = new Object();
                                        if (CreateMergedObj(o_new, listObjs)) // merge the chosen reactive ends in the chosen objects and remove the objs with this pair of free ends         
                                        {
                                            listObjs.Add(o_new);
                                            //     renumber bands in the whole genome
                                            int[] b_index = new int[46];
                                            foreach (Object o in listObjs)
                                            {
                                                foreach (Band b in o.md.chromo_bands)
                                                {
                                                    b_index[b.Chromo_num]++;
                                                    b.Ordinal_number = b_index[b.Chromo_num];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            o_new = null; // this happens when one if the members of listObjs becomes a ring               
                                        }
                                    }
                                }
                                else
                                {
                                    if (SimpleRNG.GetUniform() < MisrejoiningProb(freeend1, freeend2))
                                    {
                                        Object o_new = new Object();
                                        if (CreateMergedObj(o_new, listObjs)) // merge the chosen reactive ends in the chosen objects       
                                        {
                                            listObjs.Add(o_new);
                                            //     renumber bands in the whole genome
                                            int[] b_index = new int[46];
                                            foreach (Object o in listObjs)
                                            {
                                                foreach (Band b in o.md.chromo_bands)
                                                {
                                                    b_index[b.Chromo_num]++;
                                                    b.Ordinal_number = b_index[b.Chromo_num];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            o_new = null; // this happens when one if the members of listObjs becomes a ring
                                        }
                                    }
                                }
                                // if nothing happens de-activate free ends                                                                        
                                foreach (Object o in listObjs)
                                {
                                    foreach (Free_end fe in o.md.f_e)
                                    {
                                        if (fe.Reacting)
                                        {
                                            fe.Reacting = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (CheckDSBlist(DSBdata) && CheckObjList(listObjs))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            { // merge failure 
                Detailed_checkObjList(listObjs);
                Detailed_checkDSBlist(DSBdata);
                return false;
            }
        }
        //      
        public bool ApplyRepairAcuteDose(List<Object> listObjs) // repair w/o kinetics (no time factor); this function will not be used probably
        {
            try
            {
                Random random = new Random();
                int nDSBs_to_repair = 2;
                int nfe = 0;
                foreach (Object o in listObjs)
                {
                    foreach (Free_end fe in o.md.f_e)
                    {
                        if (fe.FE_type == MetaData.FreeEndType.reactive)
                        {
                            nfe++;
                        }
                    }
                }

                int[] l = new int[nfe];
                for (int k = 0; k < nDSBs_to_repair; k++)
                {
                    for (int i = 0; i < l.Length; i++)
                    {
                        l[i] = 0;
                    }

                    for (int m = 0; m < 2; m++)
                    {
                        int fe = 0;
                        while (true)
                        {
                            int itemp2 = random.Next(nfe);
                            if (l[itemp2] != 1)
                            {
                                fe = itemp2;
                                break;
                            }
                        }
                        l[fe] = 1;
                        int itemp = 0;
                        foreach (Object o in listObjs)
                        {
                            foreach (Free_end fe1 in o.md.f_e)
                            {
                                if (fe1.FE_type == MetaData.FreeEndType.reactive)
                                {
                                    if (itemp == fe)
                                    {
                                        fe1.Reacting = true;
                                    }
                                    itemp++;
                                }
                            }
                        }
                    }
                    int itemp1 = 0;
                    foreach (Object o in listObjs)
                    {
                        foreach (Free_end fe2 in o.md.f_e)
                        {
                            if (fe2.Reacting)
                            {
                                itemp1++;
                            }
                        }
                    }

                    if (itemp1 != 2)
                    {
                        return false;
                    }
                    else
                    { // merge 2 objects    
                        nfe -= 2;
                        Object o_new = new Object();
                        if (CreateMergedObj(o_new, listObjs)) // merge the chosen reactive ends in the chosen objects       
                        {
                            listObjs.Add(o_new);
                            //     renumber bands in the whole genome
                            int[] b_index = new int[46];
                            foreach (Object o in listObjs)
                            {
                                foreach (Band b in o.md.chromo_bands)
                                {
                                    b_index[b.Chromo_num]++;
                                    b.Ordinal_number = b_index[b.Chromo_num];
                                }
                            }
                        }
                        else
                        {
                            o_new = null; // this happens when one if the members of listObjs becomes a ring
                        }
                    }
                }
                if (CheckDSBlist(DSBdata) && CheckObjList(listObjs))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            { // merge failure 
                Detailed_checkObjList(listObjs);
                Detailed_checkDSBlist(DSBdata);
                return false;
            }
            finally { }
        }
    }
}
