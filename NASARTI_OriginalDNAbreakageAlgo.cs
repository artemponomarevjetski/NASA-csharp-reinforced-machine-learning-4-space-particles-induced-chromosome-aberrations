using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Threading;

namespace GraficDisplay
{
    internal partial class Genome
    {
        internal partial class NASARTI_original : Genome
        {
            public static readonly double R = 6.0; // radius of nucleus in microns
            public static readonly double lattice_dim = 0.02; // microns
            public static double Dconst = 0.0;
            public static MainForm.Particle[] amorphous_partile_tracks = null;
            public int thread = 0;
            public NASARTI_original()
            {
            }
            public NASARTI_original(RadiationSchema rs)
            {
                amorphous_partile_tracks = new MainForm.Particle[rs.nBeams];
                SimpleRNG.SetSeedFromSystemTime(); //  setting seed from computer time                 
                for (int nb = 0; nb < rs.nBeams; nb++) // iterate over all beams
                {
                    amorphous_partile_tracks[nb] = new MainForm.Particle(rs.pt[nb])
                    {
                        track_struct = new MainForm.Particle.Tracks()
                    };
                    amorphous_partile_tracks[nb].track_struct.distance = new List<double>();
                    amorphous_partile_tracks[nb].track_struct.dose = new List<double>();
                    string appPath = Application.StartupPath;
                    string trackData;
                    string[] RDinput;
                    double LET;
                    double LET1;
                    double fraction_of_energy_lost = 0.0;
                    if (rs.pt[nb] == MainForm.Radiation.p_type.photon)
                    {
                        Dconst += rs.D[nb];
                    }
                    else
                    {
                        if (File.Exists(appPath + @"\RD\RD.dat"))
                        {
                            trackData = File.ReadAllText(appPath + @"\RD\RD.dat");
                            RDinput = trackData.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                            // define particles with track properties   
                            LET = Convert.ToDouble(RDinput[0]);
                            LET1 = Convert.ToDouble(RDinput[1]);
                            fraction_of_energy_lost = Convert.ToDouble(RDinput[2]);
                            Dconst += rs.D[nb] * fraction_of_energy_lost;
                            for (int i = 0; i < RDinput.Length - 3; i++)
                            {
                                switch (i % 5)
                                {
                                    case 0:
                                        {
                                            amorphous_partile_tracks[nb].E = amorphous_partile_tracks[nb].track_struct.energy = Convert.ToInt32(Convert.ToDouble(RDinput[i + 3]));
                                        }
                                        break;
                                    case 1:
                                        {
                                            amorphous_partile_tracks[nb].A = amorphous_partile_tracks[nb].track_struct.mass = Convert.ToInt32(Convert.ToDouble(RDinput[i + 3]));
                                        }
                                        break;
                                    case 2:
                                        {
                                            amorphous_partile_tracks[nb].Z = amorphous_partile_tracks[nb].track_struct.charge = Convert.ToInt32(Convert.ToDouble(RDinput[i + 3]));
                                        }
                                        break;
                                    case 3:
                                        {
                                            amorphous_partile_tracks[nb].track_struct.distance.Add(Convert.ToDouble(RDinput[i + 3]));
                                        }
                                        break;
                                    case 4:
                                        {
                                            amorphous_partile_tracks[nb].track_struct.dose.Add(Convert.ToDouble(RDinput[i + 3]));
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            string source = appPath + @"\RD\RD.dat";
                            string destination = appPath + @"\RD\RD_" + Convert.ToInt32(amorphous_partile_tracks[nb].track_struct.mass).ToString() + "_"
                                + Convert.ToInt32(amorphous_partile_tracks[nb].track_struct.charge).ToString() + "_"
                                + Convert.ToInt32(amorphous_partile_tracks[nb].track_struct.energy).ToString() + ".dat";
                            try
                            {
                                File.Copy(source, destination, true);
                                File.Delete(source);
                            }
                            catch
                            {
                                MessageBox.Show("File RD.dat with the amorphous track structure was not processed!");
                            }
                        }
                        else
                        {
                            try
                            {  // parse file name   
                                trackData = File.ReadAllText(appPath + @"\RD\RD_" + Convert.ToInt32(amorphous_partile_tracks[nb].A).ToString() + "_"
                                    + Convert.ToInt32(amorphous_partile_tracks[nb].Z).ToString() + "_" + Convert.ToInt32(amorphous_partile_tracks[nb].E).ToString() + ".dat");
                                RDinput = trackData.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                                // define particles with track properties   
                                LET = Convert.ToDouble(RDinput[0]);
                                LET1 = Convert.ToDouble(RDinput[1]);
                                fraction_of_energy_lost = Convert.ToDouble(RDinput[2]);
                                Dconst += rs.D[nb] * fraction_of_energy_lost;
                                for (int i = 0; i < RDinput.Length - 3; i++)
                                {
                                    switch (i % 5)
                                    {
                                        case 0:
                                            {
                                                amorphous_partile_tracks[nb].E = amorphous_partile_tracks[nb].track_struct.energy = Convert.ToInt32(Convert.ToDouble(RDinput[i + 3]));
                                            }
                                            break;
                                        case 1:
                                            {
                                                amorphous_partile_tracks[nb].A = amorphous_partile_tracks[nb].track_struct.mass = Convert.ToInt32(Convert.ToDouble(RDinput[i + 3]));
                                            }
                                            break;
                                        case 2:
                                            {
                                                amorphous_partile_tracks[nb].Z = amorphous_partile_tracks[nb].track_struct.charge = Convert.ToInt32(Convert.ToDouble(RDinput[i + 3]));
                                            }
                                            break;
                                        case 3:
                                            {
                                                amorphous_partile_tracks[nb].track_struct.distance.Add(Convert.ToDouble(RDinput[i + 3]));
                                            }
                                            break;
                                        case 4:
                                            {
                                                amorphous_partile_tracks[nb].track_struct.dose.Add(Convert.ToDouble(RDinput[i + 3]));
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                amorphous_partile_tracks[nb].p_t = GetPtype(amorphous_partile_tracks[nb].A, amorphous_partile_tracks[nb].Z, amorphous_partile_tracks[nb].E);
                                amorphous_partile_tracks[nb].track_struct.X = new List<int>();
                                amorphous_partile_tracks[nb].track_struct.Y = new List<int>();
                                amorphous_partile_tracks[nb].lambda = rs.D[nb] / (LET * 1.6021 / 10.0) * (Math.PI * (R + RadiationSchema.Pr) * (R + RadiationSchema.Pr));
                                amorphous_partile_tracks[nb].nParticles = Convert.ToInt32(amorphous_partile_tracks[nb].lambda);
                                amorphous_partile_tracks[nb].track_struct.ntracks = SimpleRNG.GetPoisson(amorphous_partile_tracks[nb].lambda); //  Poisson dist. ntracks                               
                                Random random = new Random();
                                for (int i = 0; i < amorphous_partile_tracks[nb].track_struct.ntracks; i++)
                                {
                                    int cell_r = Convert.ToInt32(R / lattice_dim);
                                    amorphous_partile_tracks[nb].track_struct.X.Add(random.Next(0, 2 * cell_r) - cell_r);
                                    amorphous_partile_tracks[nb].track_struct.Y.Add(random.Next(0, 2 * cell_r) - cell_r);
                                }
                            }
                            catch
                            {
                                MessageBox.Show("Input file with the radial dose could not be found!");
                            }
                        }
                    }
                }
            }
            //
            public MainForm.Radiation.p_type GetPtype(double a, double z, double e)
            {
                MainForm.Radiation.p_type p_t;
                switch (z)
                {
                    case 0:
                        {
                            if (a == 1.0)
                                p_t = MainForm.Radiation.p_type.neutron;
                            else
                            {
                                if (a == 0.0)
                                    p_t = MainForm.Radiation.p_type.photon;
                                else
                                    p_t = MainForm.Radiation.p_type.not_known;
                            }
                        }
                        break;
                    case 1:
                        p_t = MainForm.Radiation.p_type.proton;
                        break;
                    case 2:
                        p_t = MainForm.Radiation.p_type.He;
                        break;
                    case 6:
                        p_t = MainForm.Radiation.p_type.C;
                        break;
                    case 8:
                        p_t = MainForm.Radiation.p_type.O;
                        break;
                    case 10:
                        p_t = MainForm.Radiation.p_type.Ne;
                        break;
                    case 14:
                        p_t = MainForm.Radiation.p_type.Si;
                        break;
                    case 18:
                        p_t = MainForm.Radiation.p_type.Ar;
                        break;
                    case 22:
                        p_t = MainForm.Radiation.p_type.Ti;
                        break;
                    case 26:
                        p_t = MainForm.Radiation.p_type.Fe;
                        break;
                    default:
                        {
                            p_t = MainForm.Radiation.p_type.not_known;
                            //amorphous_partile_tracks[nb].A = a;
                            //amorphous_partile_tracks[nb].Z = z;
                            //amorphous_partile_tracks[nb].E = e;
                            //A = a;
                            //Z = z;
                            //E = e;
                        }
                        break;
                }
                return p_t;
            }
            //
            public static bool InsideEllipsoid(Location l)
            {
                double a = 1.0 / 2.0, b = 1.0 / 2.0, c = 4.0; // nucleus parameters for elliptic nucleus, GUI input
                int rnucleus = Convert.ToInt32(R / lattice_dim);
                if (a * (l.X - rnucleus) * (l.X - rnucleus) + b * (l.Y - rnucleus) * (l.Y - rnucleus) + c * (l.Z - rnucleus) * (l.Z - rnucleus) < rnucleus * rnucleus) // elliptic nucleus            
                    return true;
                else
                    return false;
            }
            //
            public bool PrepairDSBpositionsInGenome(RadiationSchema rs, DSBs DSBdata)
            {
                bool bParallel = false; 
                try
                {
                    RandomWalk rw = new RandomWalk();
                    int nmonomers = IntactHumanGenome.WholeGenome();
                    rw.LL = new Location[nmonomers];
                    rw.nmonomer = new int[nmonomers];
                    Random random = new Random();
                    int rnucleus = Convert.ToInt32(R / lattice_dim);
                    int j = 0;
                    if (!bParallel)
                    {
                        for (int chn = 0; chn < IntactHumanGenome.nObjs; chn++) // split these into subtasks
                        {
                            while (true)
                            {
                                rw.LL[j].X = random.Next(0, 2 * rnucleus) - rnucleus; // RW random origin
                                rw.LL[j].Y = random.Next(0, 2 * rnucleus) - rnucleus;
                                rw.LL[j].Z = random.Next(0, 2 * rnucleus) - rnucleus;
                                if (InsideEllipsoid(rw.LL[j])) break;
                            } // these RWs don't have loops or domains yet
                            rw.nmonomer[j] = j;
                            j++;
                            for (int i = 0; i < Convert.ToInt32(IntactHumanGenome.NC[chn] / IntactHumanGenome.monomerSize) - 1; i++)
                            {
                                Location l_temp;
                                switch (random.Next(0, 3))
                                {
                                    case 0:
                                        {
                                            l_temp.X = rw.LL[j - 1].X + (2 * random.Next(0, 2) - 1);
                                            l_temp.Y = rw.LL[j - 1].Y;
                                            l_temp.Z = rw.LL[j - 1].Z;
                                            if (InsideEllipsoid(l_temp))
                                                rw.LL[j] = l_temp;
                                        }
                                        break;
                                    case 1:
                                        {
                                            l_temp.X = rw.LL[j - 1].X;
                                            l_temp.Y = rw.LL[j - 1].Y + (2 * random.Next(0, 2) - 1);
                                            l_temp.Z = rw.LL[j - 1].Z;
                                            if (InsideEllipsoid(l_temp))
                                                rw.LL[j] = l_temp;
                                        }
                                        break;
                                    case 2:
                                        {
                                            l_temp.X = rw.LL[j - 1].X;
                                            l_temp.Y = rw.LL[j - 1].Y;
                                            l_temp.Z = rw.LL[j - 1].Z + (2 * random.Next(0, 2) - 1);
                                            if (InsideEllipsoid(l_temp))
                                                rw.LL[j] = l_temp;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                rw.nmonomer[j] = j;
                                j++;
                            }
                        }
                        if (ApplyRad2RWs(rs, rw, DSBdata)) // collect all sets of DSBs and put them into a list 
                            return true;
                        else return false;
                    }
                    else
                    {
                        //foreach (var item in new ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get())
                        //{
                        //    Console.WriteLine("Number Of Physical Processors: {0} ", item["NumberOfProcessors"]);
                        //}
                        int coreCount = 0;
                        foreach (var item in new ManagementObjectSearcher("Select * from Win32_Processor").Get())
                        {
                            coreCount += int.Parse(item["NumberOfCores"].ToString());
                        }
                        //Console.WriteLine("Number Of Cores: {0}", coreCount);
                        //Console.WriteLine("Number Of Logical Processors: {0}", Environment.ProcessorCount);
                        int processorCount = Environment.ProcessorCount;
                        int logicalprocessorCount = 0;
                        foreach (var item in new ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get())
                        {
                            //Console.WriteLine("Number Of Logical Processors: {0}", item["NumberOfLogicalProcessors"]);
                            logicalprocessorCount += int.Parse(item["NumberOfLogicalProcessors"].ToString());
                        }
                        int nThreads = coreCount; // ? or logicalprocessorCount?
                        Thread[] oThread = null;
                        for (thread = 0; thread < nThreads; thread++)
                        {
                            Work w = new Work(thread, nThreads, rs, DSBdata);
                            ThreadStart threadDelegate = new ThreadStart(Work.DoWork);
                            oThread[thread] = new Thread(threadDelegate);
                            oThread[thread].Start();
                        }
                    }
                    return true;
                    // run subtask all the way here, so that every CPU would produce DSBdata for each own RW, which has 46/nCPU  chromosomes; meaure time with and w/o parallelization                
                }
                catch { return false; }
            }
            //
            class Work : NASARTI_original
            {
                public static int nThread, nThreads;
                public static RadiationSchema rs;
                public static DSBs DSBdata;
                public Work() { }
                public Work(int thread, int n, RadiationSchema r, DSBs dd)
                {
                    nThread = thread;
                    nThreads = n;
                    rs = r;
                    DSBdata = dd;
                }
                public static void DoWork()
                {
                    RandomWalk rw = new RandomWalk();
                    int nmonomers = IntactHumanGenome.WholeGenome();
                    rw.LL = new Location[nmonomers];
                    rw.nmonomer = new int[nmonomers];
                    Random random = new Random();
                    int rnucleus = Convert.ToInt32(R / lattice_dim);
                    int j = 0;
                    for (int chn = nThread * IntactHumanGenome.nObjs / nThreads; chn < (nThread + 1) * IntactHumanGenome.nObjs / nThreads; chn++)
                    {
                        while (true)
                        {
                            rw.LL[j].X = random.Next(0, 2 * rnucleus) - rnucleus; // RW random origin
                            rw.LL[j].Y = random.Next(0, 2 * rnucleus) - rnucleus;
                            rw.LL[j].Z = random.Next(0, 2 * rnucleus) - rnucleus;
                            if (InsideEllipsoid(rw.LL[j])) break;
                        } // these RWs don't have loops or domains yet
                        rw.nmonomer[j] = j;
                        j++;
                        for (int i = 0; i < Convert.ToInt32(IntactHumanGenome.NC[chn] / IntactHumanGenome.monomerSize) - 1; i++)
                        {
                            Location l_temp;
                            switch (random.Next(0, 3))
                            {
                                case 0:
                                    {
                                        l_temp.X = rw.LL[j - 1].X + (2 * random.Next(0, 2) - 1);
                                        l_temp.Y = rw.LL[j - 1].Y;
                                        l_temp.Z = rw.LL[j - 1].Z;
                                        if (InsideEllipsoid(l_temp))
                                            rw.LL[j] = l_temp;
                                    }
                                    break;
                                case 1:
                                    {
                                        l_temp.X = rw.LL[j - 1].X;
                                        l_temp.Y = rw.LL[j - 1].Y + (2 * random.Next(0, 2) - 1);
                                        l_temp.Z = rw.LL[j - 1].Z;
                                        if (InsideEllipsoid(l_temp))
                                            rw.LL[j] = l_temp;
                                    }
                                    break;
                                case 2:
                                    {
                                        l_temp.X = rw.LL[j - 1].X;
                                        l_temp.Y = rw.LL[j - 1].Y;
                                        l_temp.Z = rw.LL[j - 1].Z + (2 * random.Next(0, 2) - 1);
                                        if (InsideEllipsoid(l_temp))
                                            rw.LL[j] = l_temp;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            rw.nmonomer[j] = j;
                            j++;
                        }
                    }
                    ApplyRad2RWs(rs, rw, DSBdata); // collect all sets of DSBs and put them into a list               
                }
            }
            //
            public static bool ApplyRad2RWs(RadiationSchema rs, RandomWalk rw, DSBs DSBdata)
            {
                int ndsb = 0;
                try
                {
                    double dose;
                    double dose_total = 0.0;
                    for (int nb = 0; nb < rs.nBeams; nb++) // iterate over all beams
                    {
                        foreach (int j in rw.nmonomer)
                        {
                            dose = Dconst;
                            dose_total += Dconst;
                            try
                            {
                                for (int i = 0; i < amorphous_partile_tracks[nb].track_struct.ntracks; i++)
                                {
                                    int x = amorphous_partile_tracks[nb].track_struct.X[i];
                                    int y = amorphous_partile_tracks[nb].track_struct.Y[i];
                                    int dist2 = (x - rw.LL[j].X) * (x - rw.LL[j].X) + (y - rw.LL[j].Y) * (y - rw.LL[j].Y);
                                    if (dist2 < Convert.ToInt32((RadiationSchema.Pr / lattice_dim) * (RadiationSchema.Pr / lattice_dim)))
                                    {
                                        int t = Convert.ToInt32(MainForm.Particle.Tracks.grid * lattice_dim * Math.Sqrt(dist2) / RadiationSchema.Pr); // microns
                                        dose += amorphous_partile_tracks[nb].track_struct.dose[t];
                                        dose_total += amorphous_partile_tracks[nb].track_struct.dose[t];
                                    }
                                }
                            }
                            catch { return false; }
                            double Q = 0.812 * 35.0 / 25.0; // multiply by 35./25. for high/low LET; also 1e-5 is factored out                          
                            if (amorphous_partile_tracks[nb].p_t == MainForm.Radiation.p_type.photon)
                            {
                                Q = 0.812; // multiply by 35./25. for high LET                               
                            }
                            else
                            {
                                Q = 0.812 * 35.0 / 25.0; // multiply by 35./25. for high LET                            
                            }
                            Q *= 1.0e-5; // Q is determined from PFGE experiments
                            Random random = new Random();
                            try
                            {
                                if (SimpleRNG.GetUniform() < 1.0 - Math.Exp(-Q * dose))
                                {
                                    ndsb++;
                                    DSBs.DSBstruct new_dsb = new DSBs.DSBstruct
                                    {
                                        ndsb = ndsb,
                                        L = rw.LL[j],
                                        position = rw.nmonomer[j],
                                        entry_time = rs.entryTime[nb],
                                        exit_time = rs.exitTime[nb]
                                    };
                                    new_dsb.random_time = SimpleRNG.GetUniform() * (new_dsb.exit_time - new_dsb.entry_time) + new_dsb.entry_time; // a DSB is created sometime during the fractionation interval
                                    double r = SimpleRNG.GetUniform();
                                    if (r < DSBs.DSBcomplP[0])
                                    {
                                        new_dsb.DSBcmplx = DSBs.DSBstruct.DSBcomplexity.simpleDSB;
                                    }
                                    else
                                    {
                                        if (r >= DSBs.DSBcomplP[0] && r < DSBs.DSBcomplP[1]) // can a DSB appear more than 1 time at the same monomer
                                            new_dsb.DSBcmplx = DSBs.DSBstruct.DSBcomplexity.DSBplus;
                                        else
                                            new_dsb.DSBcmplx = DSBs.DSBstruct.DSBcomplexity.DSBplusplus;
                                    }
                                    DSBdata.listDSBs.Add(new_dsb);
                                }
                            }
                            catch { return false; }
                        }
                    }
                    dose_total /= Convert.ToDouble(rw.nmonomer.Length); // total dose integrated over all monomers // might wanna output somewhere
                    return true;
                }
                catch { return false; }
            }
            //
            public struct RandomWalk
            {
                public int[] nmonomer;
                public Location[] LL;
                public RandomWalk(Location[] ll, int[] n)
                {
                    LL = ll;
                    nmonomer = n;
                }
            };
        }
    }

    /// <summary>
    /// SimpleRNG is a simple random number generator based on 
    /// George Marsaglia's MWC (multiply with carry) generator.
    /// Although it is very simple, it passes Marsaglia's DIEHARD
    /// series of random number generator tests.
    /// 
    /// Written by John D. Cook 
    /// http://www.johndcook.com
    /// </summary>
    public class SimpleRNG
    {
        private static uint m_w;
        private static uint m_z;

        static SimpleRNG()
        {
            // These values are not magical, just the default values Marsaglia used.
            // Any pair of unsigned integers should be fine.
            m_w = 521288629;
            m_z = 362436069;
        }

        // The random generator seed can be set three ways:
        // 1) specifying two non-zero unsigned integers
        // 2) specifying one non-zero unsigned integer and taking a default value for the second
        // 3) setting the seed from the system time
        public static int GetPoisson(double lambda)
        {
            return (lambda < 30.0) ? PoissonSmall(lambda) : PoissonLarge(lambda);
        }

        private static int PoissonSmall(double lambda)
        {
            // Algorithm due to Donald Knuth, 1969.
            double p = 1.0, L = Math.Exp(-lambda);
            int k = 0;
            do
            {
                k++;
                p *= GetUniform();
            }
            while (p > L);
            return k - 1;
        }

        private static int PoissonLarge(double lambda)
        {
            // "Rejection method PA" from "The Computer Generation of 
            // Poisson Random Variables" by A. C. Atkinson,
            // Journal of the Royal Statistical Society Series C 
            // (Applied Statistics) Vol. 28, No. 1. (1979)
            // The article is on pages 29-35. 
            // The algorithm given here is on page 32.

            double c = 0.767 - 3.36 / lambda;
            double beta = Math.PI / Math.Sqrt(3.0 * lambda);
            double alpha = beta * lambda;
            double k = Math.Log(c) - lambda - Math.Log(beta);

            for (; ; )
            {
                double u = GetUniform();
                double x = (alpha - Math.Log((1.0 - u) / u)) / beta;
                int n = (int)Math.Floor(x + 0.5);
                if (n < 0)
                    continue;
                double v = GetUniform();
                double y = alpha - beta * x;
                double temp = 1.0 + Math.Exp(y);
                double lhs = y + Math.Log(v / (temp * temp));
                double rhs = k + n * Math.Log(lambda) - LogFactorial(n);
                if (lhs <= rhs)
                    return n;
            }
        }

        public static void SetSeed(uint u, uint v)
        {
            if (u != 0) m_w = u;
            if (v != 0) m_z = v;
        }

        public static void SetSeed(uint u)
        {
            m_w = u;
        }

        public static void SetSeedFromSystemTime()
        {
            DateTime dt = DateTime.Now;
            long x = dt.ToFileTime();
            SetSeed((uint)(x >> 16), (uint)(x % 4294967296));
        }

        // Produce a uniform random sample from the open interval (0, 1).
        // The method will not return either end point.
        public static double GetUniform()
        {
            // 0 <= u < 2^32
            uint u = GetUint();
            // The magic number below is 1/(2^32 + 2).
            // The result is strictly between 0 and 1.
            return (u + 1.0) * 2.328306435454494e-10;
        }

        // This is the heart of the generator.
        // It uses George Marsaglia's MWC algorithm to produce an unsigned integer.
        // See http://www.bobwheeler.com/statistics/Password/MarsagliaPost.txt
        private static uint GetUint()
        {
            m_z = 36969 * (m_z & 65535) + (m_z >> 16);
            m_w = 18000 * (m_w & 65535) + (m_w >> 16);
            return (m_z << 16) + m_w;
        }

        // Get normal (Gaussian) random sample with mean 0 and standard deviation 1
        public static double GetNormal()
        {
            // Use Box-Muller algorithm
            double u1 = GetUniform();
            double u2 = GetUniform();
            double r = Math.Sqrt(-2.0 * Math.Log(u1));
            double theta = 2.0 * Math.PI * u2;
            return r * Math.Sin(theta);
        }

        // Get normal (Gaussian) random sample with specified mean and standard deviation
        public static double GetNormal(double mean, double standardDeviation)
        {
            if (standardDeviation <= 0.0)
            {
                string msg = string.Format("Shape must be positive. Received {0}.", standardDeviation);
                throw new ArgumentOutOfRangeException(msg);
            }
            return mean + standardDeviation * GetNormal();
        }

        // Get exponential random sample with mean 1
        public static double GetExponential()
        {
            return -Math.Log(GetUniform());
        }

        // Get exponential random sample with specified mean
        public static double GetExponential(double mean)
        {
            if (mean <= 0.0)
            {
                string msg = string.Format("Mean must be positive. Received {0}.", mean);
                throw new ArgumentOutOfRangeException(msg);
            }
            return mean * GetExponential();
        }

        public static double GetGamma(double shape, double scale)
        {
            // Implementation based on "A Simple Method for Generating Gamma Variables"
            // by George Marsaglia and Wai Wan Tsang.  ACM Transactions on Mathematical Software
            // Vol 26, No 3, September 2000, pages 363-372.

            double d, c, x, xsquared, v, u;

            if (shape >= 1.0)
            {
                d = shape - 1.0 / 3.0;
                c = 1.0 / Math.Sqrt(9.0 * d);
                for (; ; )
                {
                    do
                    {
                        x = GetNormal();
                        v = 1.0 + c * x;
                    }
                    while (v <= 0.0);
                    v = v * v * v;
                    u = GetUniform();
                    xsquared = x * x;
                    if (u < 1.0 - 0.0331 * xsquared * xsquared || Math.Log(u) < 0.5 * xsquared + d * (1.0 - v + Math.Log(v)))
                        return scale * d * v;
                }
            }
            else if (shape <= 0.0)
            {
                string msg = string.Format("Shape must be positive. Received {0}.", shape);
                throw new ArgumentOutOfRangeException(msg);
            }
            else
            {
                double g = GetGamma(shape + 1.0, 1.0);
                double w = GetUniform();
                return scale * g * Math.Pow(w, 1.0 / shape);
            }
        }

        public static double GetChiSquare(double degreesOfFreedom)
        {
            // A chi squared distribution with n degrees of freedom
            // is a gamma distribution with shape n/2 and scale 2.
            return GetGamma(0.5 * degreesOfFreedom, 2.0);
        }

        public static double GetInverseGamma(double shape, double scale)
        {
            // If X is gamma(shape, scale) then
            // 1/Y is inverse gamma(shape, 1/scale)
            return 1.0 / GetGamma(shape, 1.0 / scale);
        }

        public static double GetWeibull(double shape, double scale)
        {
            if (shape <= 0.0 || scale <= 0.0)
            {
                string msg = string.Format("Shape and scale parameters must be positive. Recieved shape {0} and scale{1}.", shape, scale);
                throw new ArgumentOutOfRangeException(msg);
            }
            return scale * Math.Pow(-Math.Log(GetUniform()), 1.0 / shape);
        }

        public static double GetCauchy(double median, double scale)
        {
            if (scale <= 0)
            {
                string msg = string.Format("Scale must be positive. Received {0}.", scale);
                throw new ArgumentException(msg);
            }

            double p = GetUniform();

            // Apply inverse of the Cauchy distribution function to a uniform
            return median + scale * Math.Tan(Math.PI * (p - 0.5));
        }

        public static double GetStudentT(double degreesOfFreedom)
        {
            if (degreesOfFreedom <= 0)
            {
                string msg = string.Format("Degrees of freedom must be positive. Received {0}.", degreesOfFreedom);
                throw new ArgumentException(msg);
            }

            // See Seminumerical Algorithms by Knuth
            double y1 = GetNormal();
            double y2 = GetChiSquare(degreesOfFreedom);
            return y1 / Math.Sqrt(y2 / degreesOfFreedom);
        }

        // The Laplace distribution is also known as the double exponential distribution.
        public static double GetLaplace(double mean, double scale)
        {
            double u = GetUniform();
            return (u < 0.5) ?
                mean + scale * Math.Log(2.0 * u) :
                mean - scale * Math.Log(2 * (1 - u)); // why not 2.0? This needs to be resolved, only if a Laplace dist. is used...
        }

        public static double GetLogNormal(double mu, double sigma)
        {
            return Math.Exp(GetNormal(mu, sigma));
        }

        public static double GetBeta(double a, double b)
        {
            if (a <= 0.0 || b <= 0.0)
            {
                string msg = string.Format("Beta parameters must be positive. Received {0} and {1}.", a, b);
                throw new ArgumentOutOfRangeException(msg);
            }

            // There are more efficient methods for generating beta samples.
            // However such methods are a little more efficient and much more complicated.
            // For an explanation of why the following method works, see
            // http://www.johndcook.com/distribution_chart.html#gamma_beta

            double u = GetGamma(a, 1.0);
            double v = GetGamma(b, 1.0);
            return u / (u + v);
        }

        public static double LogFactorial(int n)
        {
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            else if (n > 254)
            {
                double x = n + 1;
                return (x - 0.5) * Math.Log(x) - x + 0.5 * Math.Log(2 * Math.PI) + 1.0 / (12.0 * x);
            }
            else
            {
                double[] lf =
                    {
                    0.000000000000000,
                    0.000000000000000,
                    0.693147180559945,
                    1.791759469228055,
                    3.178053830347946,
                    4.787491742782046,
                    6.579251212010101,
                    8.525161361065415,
                    10.604602902745251,
                    12.801827480081469,
                    15.104412573075516,
                    17.502307845873887,
                    19.987214495661885,
                    22.552163853123421,
                    25.191221182738683,
                    27.899271383840894,
                    30.671860106080675,
                    33.505073450136891,
                    36.395445208033053,
                    39.339884187199495,
                    42.335616460753485,
                    45.380138898476908,
                    48.471181351835227,
                    51.606675567764377,
                    54.784729398112319,
                    58.003605222980518,
                    61.261701761002001,
                    64.557538627006323,
                    67.889743137181526,
                    71.257038967168000,
                    74.658236348830158,
                    78.092223553315307,
                    81.557959456115029,
                    85.054467017581516,
                    88.580827542197682,
                    92.136175603687079,
                    95.719694542143202,
                    99.330612454787428,
                    102.968198614513810,
                    106.631760260643450,
                    110.320639714757390,
                    114.034211781461690,
                    117.771881399745060,
                    121.533081515438640,
                    125.317271149356880,
                    129.123933639127240,
                    132.952575035616290,
                    136.802722637326350,
                    140.673923648234250,
                    144.565743946344900,
                    148.477766951773020,
                    152.409592584497350,
                    156.360836303078800,
                    160.331128216630930,
                    164.320112263195170,
                    168.327445448427650,
                    172.352797139162820,
                    176.395848406997370,
                    180.456291417543780,
                    184.533828861449510,
                    188.628173423671600,
                    192.739047287844900,
                    196.866181672889980,
                    201.009316399281570,
                    205.168199482641200,
                    209.342586752536820,
                    213.532241494563270,
                    217.736934113954250,
                    221.956441819130360,
                    226.190548323727570,
                    230.439043565776930,
                    234.701723442818260,
                    238.978389561834350,
                    243.268849002982730,
                    247.572914096186910,
                    251.890402209723190,
                    256.221135550009480,
                    260.564940971863220,
                    264.921649798552780,
                    269.291097651019810,
                    273.673124285693690,
                    278.067573440366120,
                    282.474292687630400,
                    286.893133295426990,
                    291.323950094270290,
                    295.766601350760600,
                    300.220948647014100,
                    304.686856765668720,
                    309.164193580146900,
                    313.652829949878990,
                    318.152639620209300,
                    322.663499126726210,
                    327.185287703775200,
                    331.717887196928470,
                    336.261181979198450,
                    340.815058870798960,
                    345.379407062266860,
                    349.954118040770250,
                    354.539085519440790,
                    359.134205369575340,
                    363.739375555563470,
                    368.354496072404690,
                    372.979468885689020,
                    377.614197873918670,
                    382.258588773060010,
                    386.912549123217560,
                    391.575988217329610,
                    396.248817051791490,
                    400.930948278915760,
                    405.622296161144900,
                    410.322776526937280,
                    415.032306728249580,
                    419.750805599544780,
                    424.478193418257090,
                    429.214391866651570,
                    433.959323995014870,
                    438.712914186121170,
                    443.475088120918940,
                    448.245772745384610,
                    453.024896238496130,
                    457.812387981278110,
                    462.608178526874890,
                    467.412199571608080,
                    472.224383926980520,
                    477.044665492585580,
                    481.872979229887900,
                    486.709261136839360,
                    491.553448223298010,
                    496.405478487217580,
                    501.265290891579240,
                    506.132825342034830,
                    511.008022665236070,
                    515.890824587822520,
                    520.781173716044240,
                    525.679013515995050,
                    530.584288294433580,
                    535.496943180169520,
                    540.416924105997740,
                    545.344177791154950,
                    550.278651724285620,
                    555.220294146894960,
                    560.169054037273100,
                    565.124881094874350,
                    570.087725725134190,
                    575.057539024710200,
                    580.034272767130800,
                    585.017879388839220,
                    590.008311975617860,
                    595.005524249382010,
                    600.009470555327430,
                    605.020105849423770,
                    610.037385686238740,
                    615.061266207084940,
                    620.091704128477430,
                    625.128656730891070,
                    630.172081847810200,
                    635.221937855059760,
                    640.278183660408100,
                    645.340778693435030,
                    650.409682895655240,
                    655.484856710889060,
                    660.566261075873510,
                    665.653857411105950,
                    670.747607611912710,
                    675.847474039736880,
                    680.953419513637530,
                    686.065407301994010,
                    691.183401114410800,
                    696.307365093814040,
                    701.437263808737160,
                    706.573062245787470,
                    711.714725802289990,
                    716.862220279103440,
                    722.015511873601330,
                    727.174567172815840,
                    732.339353146739310,
                    737.509837141777440,
                    742.685986874351220,
                    747.867770424643370,
                    753.055156230484160,
                    758.248113081374300,
                    763.446610112640200,
                    768.650616799717000,
                    773.860102952558460,
                    779.075038710167410,
                    784.295394535245690,
                    789.521141208958970,
                    794.752249825813460,
                    799.988691788643450,
                    805.230438803703120,
                    810.477462875863580,
                    815.729736303910160,
                    820.987231675937890,
                    826.249921864842800,
                    831.517780023906310,
                    836.790779582469900,
                    842.068894241700490,
                    847.352097970438420,
                    852.640365001133090,
                    857.933669825857460,
                    863.231987192405430,
                    868.535292100464630,
                    873.843559797865740,
                    879.156765776907600,
                    884.474885770751830,
                    889.797895749890240,
                    895.125771918679900,
                    900.458490711945270,
                    905.796028791646340,
                    911.138363043611210,
                    916.485470574328820,
                    921.837328707804890,
                    927.193914982476710,
                    932.555207148186240,
                    937.921183163208070,
                    943.291821191335660,
                    948.667099599019820,
                    954.046996952560450,
                    959.431492015349480,
                    964.820563745165940,
                    970.214191291518320,
                    975.612353993036210,
                    981.015031374908400,
                    986.422203146368590,
                    991.833849198223450,
                    997.249949600427840,
                    1002.670484599700300,
                    1008.095434617181700,
                    1013.524780246136200,
                    1018.958502249690200,
                    1024.396581558613400,
                    1029.838999269135500,
                    1035.285736640801600,
                    1040.736775094367400,
                    1046.192096209724900,
                    1051.651681723869200,
                    1057.115513528895000,
                    1062.583573670030100,
                    1068.055844343701400,
                    1073.532307895632800,
                    1079.012946818975000,
                    1084.497743752465600,
                    1089.986681478622400,
                    1095.479742921962700,
                    1100.976911147256000,
                    1106.478169357800900,
                    1111.983500893733000,
                    1117.492889230361000,
                    1123.006317976526100,
                    1128.523770872990800,
                    1134.045231790853000,
                    1139.570684729984800,
                    1145.100113817496100,
                    1150.633503306223700,
                    1156.170837573242400
                };
                return lf[n];
            }
        }
    }
}
