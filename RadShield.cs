using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace GraficDisplay
{
    // using InsertChart; // zzz -- incompatible lib for charting, fix it for visual output

    public partial class MainForm : Form
    {
        public partial class Radiation
        {
            public enum p_type { photon, neutron, proton, He, Ne, Ti, Fe, N, C, O, Li, Si, Ar, not_known }; // etc.
            public readonly int nParticlesInitial = Convert.ToInt32(100.0); // zzz
            int nShields;
            RadiationShield[] radShields;
            public Radiation()
            {
                nShields = 10; // analyze nShields configurations
                radShields = new RadiationShield[nShields];
            }
            public Radiation(int i)
            {
                nShields = i;
                radShields = new RadiationShield[nShields];
            }

            public string TransportThroughShield()
            {
                string str = "\t";
                for (int i = 0; i < nShields; i++)
                {
                    radShields[i] = new RadiationShield(i);
                    radShields[i].Initialize();
                    radShields[i].RunTransport(i);
                    radShields[i].md.results.ShieldEfficiency = radShields[i].CalculateBeamAttenuation(); // final data         
                    str += radShields[i].md.results.ShieldEfficiency.ToString();
                    str += "\t";
                }
                return str;
            }

            public void Finish()
            {
                // Movie();
                //        PowerPointChart ppc = new PowerPointChart();
                //    ppc.DrawChart();
                //      OutputToPPT();
                OutputToExcel(radShields);
                Application.Exit();
            }
        }

        public class Particle : Radiation
        {
            public readonly double P = 5.0; // microns  
            public double lambda;
            public int nParticles;
            public p_type p_t;
            public double A, Z, E;
            public Particle()
            { // protons  
                A = 1.0;
                Z = 1.0;
                E = 150.0; // MeV/n
                p_t = p_type.proton;
            }
            public Particle(p_type p_t)
            {
                switch (p_t)
                {
                    case p_type.photon:
                        { // photon
                            A = 0.0;
                            Z = 0.0;
                            E = 10.0;
                        }
                        break;
                    case p_type.neutron:
                        { // neutron
                            A = 1.0;
                            Z = 0.0;
                            E = 10.0;
                        }
                        break;
                    case p_type.proton:
                        { // proton 
                            A = 1.0;
                            Z = 1.0;
                            E = 150.0;
                        }
                        break;
                    case p_type.Si:
                        { // Si ion
                            A = 16.0;
                            Z = 8.0;
                            E = 55.0;
                        }
                        break;
                    case p_type.Fe:
                        { // Fe ion
                            A = 56.0;
                            Z = 26.0;
                            E = 450.0;
                        }
                        break;
                    default:
                        { // protons  
                            A = 1.0;
                            Z = 1.0;
                            E = 150.0; // MeV/n
                            p_t = p_type.proton;
                        }
                        break;
                }
            }

            public void Initialize() // overwrites constructor
            { // protons  
                A = 1.0;
                Z = 1.0;
                E = 150.0; // MeV/n
                p_t = p_type.proton;
            }

            public void Initialize(double a, double z, double e)  // overwrites constructor
            {
                A = a;
                Z = z;
                E = e;
                p_t = GetPtype(a, z, e);
            }

            public void Initialize(p_type p_t, double e)  // overwrites constructor
            {
                E = e;
                ParticleAZ(p_t);
            }

            public void ParticleAZ(p_type p_t)
            {
                double m_dMass = 0.0, m_dCharge = 0.0;
                switch (p_t)
                {
                    case p_type.C:
                        { m_dMass = 12.0; m_dCharge = 6.0; }
                        break;  // C ions                           
                    case p_type.O:
                        { m_dMass = 16.0; m_dCharge = 8.0; }
                        break;  // O ions                  
                    case p_type.Ne:
                        { m_dMass = 20.0; m_dCharge = 10.0; }
                        break;  // Ne ions                                  
                    case p_type.Si:
                        { m_dMass = 28.0; m_dCharge = 14.0; }
                        break;  // Si ions                   
                    case p_type.Ar:
                        { m_dMass = 40.0; m_dCharge = 18.0; }
                        break;  // Ar ions                 
                    case p_type.Ti:
                        { m_dMass = 48.0; m_dCharge = 22.0; }
                        break;  // Ti ions                                   
                    case p_type.proton:
                        { m_dMass = 1.00; m_dCharge = 1.00; }
                        break;  // protons
                    case p_type.Fe:
                        { m_dMass = 56.0; m_dCharge = 26.0; }
                        break;  // Fe ions                  
                    case p_type.He:
                        { m_dMass = 4.00; m_dCharge = 2.00; }
                        break;  // He ions                
                    default:
                        break;
                }
                A = m_dMass;
                Z = m_dCharge;
            }
            public p_type GetPtype(double a, double z, double e)
            {
                p_type p_t;
                switch (z)
                {
                    case 0:
                        {
                            if (a == 1.0)
                                p_t = p_type.neutron;
                            else
                            {
                                if (a == 0.0)
                                    p_t = p_type.photon;
                                else
                                    p_t = p_type.not_known;
                            }
                        }
                        break;
                    case 1:
                        p_t = p_type.proton;
                        break;
                    case 2:
                        p_t = p_type.He;
                        break;
                    case 6:
                        p_t = p_type.C;
                        break;
                    case 8:
                        p_t = p_type.O;
                        break;
                    case 10:
                        p_t = p_type.Ne;
                        break;
                    case 14:
                        p_t = p_type.Si;
                        break;
                    case 18:
                        p_t = p_type.Ar;
                        break;
                    case 22:
                        p_t = p_type.Ti;
                        break;
                    case 26:
                        p_t = p_type.Fe;
                        break;
                    default:
                        {
                            p_t = p_type.not_known;
                            A = a;
                            Z = z;
                            E = e;
                        }
                        break;
                }
                return p_t;
            }
            public struct Tracks
            {
                public int mass, charge, energy, ntracks;
                public const int grid = 100000;
                public List<int> X, Y;
                public List<double> distance, dose;
                public Tracks(int m, int c, int e, List<int> x, List<int> y, List<double> dis, List<double> dos, int n, int g)
                {
                    mass = m;
                    charge = c;
                    energy = e;
                    X = x;
                    Y = y;
                    distance = dis;
                    dose = dos;
                    ntracks = n;
                }
            };
            public Tracks track_struct;
        }

        public class RadiationShield : Radiation
        {
            public double[] AttenuationOfEnergy_at_depth, Depth;
            public MetaData md;
            public List<Particle> list_initialParticles, list_allParticles, list_removeParticles, list_newParticles;
            public RadiationShield()
            {
            }
            public RadiationShield(int i)
            {
                md = new MetaData(i);
                list_allParticles = new List<Particle>();
                list_removeParticles = new List<Particle>();
                list_initialParticles = new List<Particle>();
                list_newParticles = new List<Particle>();
            }

            public void Initialize()
            {
                for (int i = 0; i < nParticlesInitial; i++)
                {
                    Particle p = new Particle(); // all identical for now
                    list_initialParticles.Add(p);
                }
                for (int i = 0; i < nParticlesInitial; i++)
                {
                    Particle p = new Particle(); // all identical for now
                    list_allParticles.Add(p);
                }
            }

            public void RunTransport(int i)
            {
                const int istepMax = 1000;
                int istep = 0; // step in the material          
                double deltaDepth = MetaData.StructuralMetaData.ShieldThickness / Convert.ToDouble(istepMax); // g/cm^2
                AttenuationOfEnergy_at_depth = new double[istepMax];
                Depth = new double[istepMax];
                while (istep < istepMax) // depth 
                {
                    foreach (Particle p in list_allParticles)
                        OneDepthStepWithinMaterial(p, deltaDepth, i);
                    double startEnergy = 0;
                    foreach (Particle p in list_initialParticles)
                        startEnergy += p.E;
                    double endEnergy = 0;
                    foreach (Particle p in list_allParticles)
                        //if (p.p_t == p_type.gamma)
                        //    endEnergy += p.E / 2.0; // because of isotropy of gamma radiation
                        //else // zzz
                        endEnergy += p.E;
                    Depth[istep] = istep * deltaDepth;
                    AttenuationOfEnergy_at_depth[istep] = endEnergy / startEnergy; // final result
                    istep++;
                }
            }

            public void OneDepthStepWithinMaterial(Particle p, double deltaDepth, int i)
            {
                DeltaEnergy(p, deltaDepth); // due to ionization; gradual slowing down // zzz
                //     NewParticles(p, deltaDepth, i); // these are the secondaries, the modified primary and the modified target produced in fragmentation
                //                                // isomer emitted gamma ray
                //    IsomerGammaEmission(Particle p, double deltaDepth, int i) // zzz
                //Random random = new Random();
                //double pp = SimpleRNG.GetUniform();
                //if (pp < md.shieldStructure.concentrationOfHafnium)
                //    EnergyTransferToIsomer(p, deltaDepth, i); // abrupt velocity loss due to excitation of the isomer
                //  ConsolidateAllSpectraAtDepthD(); // all inflows and outflows during on depth step
                DeltaAllParticles(p, deltaDepth);
            }

            //public void OneDepthStepWithoutIsomer(Particle p, double deltaDepth, int i)
            //{
            //    DeltaEnergy(p, deltaDepth);   // due to ionization; gradual slowing down
            //    DeltaAllParticles(p, deltaDepth);
            //    NewParticles(p, deltaDepth, i); // these are the secondaries, the modified primary and the modified target produced in fragmentation
            //    ConsolidateAllSpectraAtDepthD(); // all inflows and outflows during on depth step
            //}

            public void DeltaEnergy(Particle p, double deltaDepth)
            {
                double rho = md.physicalMetaData.densityOfAl; // zzz
                rho *= (1.0e-3 / Math.Pow(1.0e-2, 3));
                //      p.E -= (((deltaDepth * (1.0e-3 / Math.Pow(1.0e-2, 2))) / rho) * BetheBlochFormula(p));
                p.E -= ((deltaDepth * (1.0e-3 / Math.Pow(1.0e-2, 2))) / rho) * 1.0e6 * (0.5 * 1.0e-3); // just depth in microns times LET
                if (p.E < 30.0) // MeV/n -- immobile particle threshold
                    p.E = 0.0;
            }

            public double BetheBlochFormula(Particle p) // https://en.wikipedia.org/wiki/Bethe_formula
            {
                //        const double m_e = 9.10938356e-31; // kilograms
                const double c = 299792458.0; //  m / s
                const double N_a = 6.022140857e23;
                const double M_u = 1.0; // g/mol
                const double e = 1.60217662e-19; //  oulombs
                const double ZHf = 72.0; // Hf
                const double I = 10.0 * ZHf; // I = (10~\mathrm { eV} )\cdot Z
                const double epsilon0 = 8.854187817e-12; //  F⋅m−1(farads per metre)
                const double m_p = 1.6726219e-27; // kilograms
                const double MeV2J = 1.60218e-13; // MeV/J
                double rho = md.shieldStructure.concentrationOfHafnium * md.physicalMetaData.densityOfHf + (1 - md.shieldStructure.concentrationOfHafnium) * md.physicalMetaData.densityOfAl;
                rho *= (1.0e-3 / Math.Pow(1.0e-2, 3));
                double n = N_a * ZHf * rho / 178.49 / M_u;
                //   double beta = 1 - 0.5 * (938.0 / p.E) * (938.0 / p.E); // https://physics.stackexchange.com/questions/716/relativistic-speed-energy-relation-is-this-correct
                double beta = Math.Sqrt(2.0 * p.E * MeV2J / m_p) / c;
                // p.E is in MeV 
                return (4.0 * Math.PI / (MeV2J * 938.0e6)) * (n * p.Z * p.Z / beta / beta) * Math.Pow((e * e / 4.0 / Math.PI / epsilon0), 2) * (Math.Log(2.0 * 938.0e6 * beta * beta / I / (1 - beta * beta)) - beta * beta);
            }

            public void DeltaAllParticles(Particle p, double deltaDepth)
            { // simple model
                if (p.E < 30.0) // MeV/n -- immobile particle threshold
                    list_removeParticles.Add(p); // this should depend on the particle and material properties       
            }

            public void EnergyTransferToIsomer(Particle p, double deltaDepth, int i)
            {
                // density of Hafnium = 13.1 g/cm^2
                // Hafnium cross-section = 500 mb
                // mean free path =  Mean free path lambda = 1/(cross_section* number_density)=
                //        1 / (500 * e ^ -3 * 1e ^ -28 * e ^ 4 * (13.1 g / cm ^ 3 * 6.023 x10 ^ 23 * (1.0 / 178.49)(moles / g))
                // = 4.5e-4 cm.
                // what spin change corresponds to E loss of the incident particle? = 2.45 MeV
                // ion-isomer cross-section model 
                // proton-isomer is precise 
                // put Hf nuclei on a cubic lattice with the right density -- future work
                //
                //const double densityOfHf = 13.1; // g/cm^3
                //const double converionFactorOfBarn2m2 = 1.0e-28; // m^2
                //const double m2cm = 100.0; // 1 m = 100 cm
                //const double avogadroConst = 6.022140857e23; // N_a
                //const double HfgramsPerMole = 178.49; //  g / mol
                const double crosssectionNeutron2Hf = 104.0; // b
                double hypotheticalCrossSection = md.physicalMetaData.sensitivityFactor * crosssectionNeutron2Hf; // b , or 500.0e-3 b? // zzz -- future isomer excitation model
                double crosssectionProton2Hf = hypotheticalCrossSection * md.physicalMetaData.converionFactorOfBarn2m2 * md.physicalMetaData.m2cm * md.physicalMetaData.m2cm; // cm^2                                                                                                                                                         
                double number_density = md.physicalMetaData.densityOfHf * md.physicalMetaData.avogadroConst * (1.0 / md.physicalMetaData.HfgramsPerMole); // moles/cm^3
                double meanFreePath = 1.0 / (crosssectionProton2Hf * number_density); // cm
                double T = Math.Exp(-(deltaDepth / md.physicalMetaData.densityOfHf) / meanFreePath);
                //double numberOfCollisions = deltaDepth / meanFreePath / md.physicalMetaData.densityOfHf;
                //double totalNumberOfCollisions = MetaData.StructuralMetaData.ShieldThickness / meanFreePath / md.physicalMetaData.densityOfHf;
                double sensitivityFactor = 1.0; // zzz -- for Fe ions and other heavy ions, it varies from 1 to 100 of protons' crosssection
                                                //Random random = new Random();
                                                //double pp = SimpleRNG.GetUniform();
                                                //if (pp < 1 - T) // zzz-- check if a more detailed Poisson model is necessary
                                                //{
                if (p.p_t == p_type.proton)
                    p.E -= ((1 - T) * 2.45); // MeV loss due to excitation              
                else
                    p.E -= (sensitivityFactor * ((1 - T) * 2.45)); // sensitivityFactor is for Fe ions and other heavy ions, it varies from 1 to 100 of protons' crosssection                                                                            
                                                                   // }
            }

            public void NewParticles(Particle p, double deltaDepth, int i)
            {
                if (p.p_t != p_type.neutron || p.p_t != p_type.proton)
                {
                    Random random = new Random();
                    double pp = SimpleRNG.GetUniform();
                    if (pp < 0.01) // improve the prob. of fragmentation
                    {
                        list_removeParticles.Add(p); // the primary disappears
                        int n_products = 10; // should be a random number of secondaries
                        for (int j = 0; j < n_products / 2; j++)
                        {
                            Particle p_born = new Particle(p_type.proton); // a constructor for a proton; later add more secondaries, like neutrons, light ions, etc.
                            list_newParticles.Add(p_born);
                        }
                        for (int j = 0; j < n_products / 2; j++)
                        {
                            Particle p_born = new Particle(p_type.neutron); // a constructor for a proton; later add more secondaries, like neutrons, light ions, etc.
                            list_newParticles.Add(p_born);
                        }
                        //double z = p.Z - 2; // crude model of fragmentation    // zzz         
                        //double e = 0.8 * p.E;
                        //double a = p.A - n_products / 2;
                        //Particle p_primary_frag;
                        //if (p.GetPtype(a, z, e) == p_type.not_known)
                        //{
                        //    p_primary_frag = new Particle();
                        //    p_primary_frag.Initialize(a, z, e);
                        //}
                        //else
                        //    p_primary_frag = new Particle(p.GetPtype(a, z, e)); // primary fragment
                        //list_newParticles.Add(p_primary_frag);
                        //Particle p_target_frag = new Particle();
                        //p_target_frag.Initialize(14 + 13 - n_products / 2, 13 - 2, 0.2 * p.E); // give it Al A, Z, E  for now // target fragment
                        //list_newParticles.Add(p_target_frag);
                    }
                }
            }

            public void IsomerGammaEmission(Particle p, double deltaDepth, int i)
            {
                Random random = new Random();
                double pp = SimpleRNG.GetUniform();
                if (pp < pp_IsomerGammaEmission(deltaDepth, i))
                {
                    Particle p_gamma = new Particle(p_type.photon);
                    list_newParticles.Add(p_gamma);
                }
            }

            public double pp_IsomerGammaEmission(double deltaDepth, int i)
            {
                double half_time = 31.0; // years
                double time_experiment = 3.0; // Mars mission duration
                double pp = 0.5 * Math.Exp(-time_experiment / half_time) / Math.Exp(1.0); // half time prob.
                                                                                          //double product = 0.0;
                                                                                          //for (int j = 0; j < md.shieldStructure.densityOfHafnium[i] * Math.Pow(deltaDepth, 3); j++)
                                                                                          //    product *= (1.0 - pp);
                                                                                          //return (1.0 - product); // zzz -- check this
                return pp;
            }

            public void ConsolidateAllSpectraAtDepthD()
            {
                list_allParticles = list_allParticles.Except(list_removeParticles).ToList(); // remove the list of stopped particles from all particles 
                list_allParticles = list_allParticles.Concat(list_newParticles).ToList(); // add the new particles
                list_removeParticles.Clear();
                list_newParticles.Clear();
            }

            public double CalculateBeamAttenuation()
            {
                double startEnergy = 0.0;
                foreach (Particle p in list_initialParticles)
                    startEnergy += p.E;
                double endEnergy = 0.0;
                foreach (Particle p in list_allParticles)
                    if (p.p_t == p_type.photon)
                        endEnergy += p.E / 2.0; // because of isotropy of gamma radiation
                    else
                        endEnergy += p.E;
                return endEnergy / startEnergy; // final result
            }
        }

        // shield structure
        public class MetaData : RadiationShield
        {
            public StatisticalMetaData results;
            public PhysicalMetaData physicalMetaData;
            public StructuralMetaData shieldStructure;
            public MetaData()
            {
            }
            public MetaData(int i)
            {
                results = new StatisticalMetaData();
                physicalMetaData = new PhysicalMetaData(i);
                shieldStructure = new StructuralMetaData(i, physicalMetaData);

            }

            public class StructuralMetaData
            {
                public DescriptiveMetaData.ShieldType sh_t;
                public static readonly double ShieldThickness = 30.0; // g/cm^2 vehicle thickness
                public double concentrationOfHafnium;
                public int nlayers;
                public enum Materials { aluminum, hafnium, zirconium, plasic, water, liquid_h };
                public double density;
                public StructuralMetaData()
                {
                }
                public StructuralMetaData(int i, PhysicalMetaData physicalMetaData)
                {
                    switch (i)
                    {
                        case 1:
                            {
                                sh_t = DescriptiveMetaData.ShieldType.hafnium_aluminum;
                                concentrationOfHafnium = 0.1;
                                density = physicalMetaData.densityOfHf * concentrationOfHafnium + physicalMetaData.densityOfAl * (1.0 - concentrationOfHafnium);
                                nlayers = 1;
                            }
                            break;
                        case 2:
                            {
                                sh_t = DescriptiveMetaData.ShieldType.hafnium_aluminum;
                                concentrationOfHafnium = 0.3;
                                density = physicalMetaData.densityOfHf * concentrationOfHafnium + physicalMetaData.densityOfAl * (1.0 - concentrationOfHafnium);
                                nlayers = 1;
                            }
                            break;
                        case 3:
                            {
                                sh_t = DescriptiveMetaData.ShieldType.hafnium_aluminum;
                                concentrationOfHafnium = 0.5;
                                density = physicalMetaData.densityOfHf * concentrationOfHafnium + physicalMetaData.densityOfAl * (1.0 - concentrationOfHafnium);
                                nlayers = 1;
                            }
                            break;
                        case 4:
                            {
                                sh_t = DescriptiveMetaData.ShieldType.hafnium_aluminum;
                                concentrationOfHafnium = 0.7;
                                density = physicalMetaData.densityOfHf * concentrationOfHafnium + physicalMetaData.densityOfAl * (1.0 - concentrationOfHafnium);
                                nlayers = 1;
                            }
                            break;
                        case 5:
                            {
                                sh_t = DescriptiveMetaData.ShieldType.hafnium_aluminum;
                                concentrationOfHafnium = 1.0;
                                density = physicalMetaData.densityOfHf * concentrationOfHafnium + physicalMetaData.densityOfAl * (1.0 - concentrationOfHafnium);
                                nlayers = 1;
                            }
                            break;
                        case 6:
                            {
                                sh_t = DescriptiveMetaData.ShieldType.monolayer;
                                concentrationOfHafnium = 0.0;
                                density = 2.7;
                                nlayers = 1;
                            }
                            break;
                        case 7:
                            {
                                sh_t = DescriptiveMetaData.ShieldType.layered;
                                concentrationOfHafnium = 1.0;
                                density = physicalMetaData.densityOfHf;
                                nlayers = 2;
                            }
                            break;
                        case 8:
                            {
                                sh_t = DescriptiveMetaData.ShieldType.hafnium;
                                concentrationOfHafnium = 1.0;
                                density = physicalMetaData.densityOfHf; // g/cm^2
                                nlayers = 1;
                            }
                            break;
                        case 9:
                            {
                                sh_t = DescriptiveMetaData.ShieldType.water;
                                concentrationOfHafnium = 0.0;
                                density = physicalMetaData.densityOfWater; // water
                                nlayers = 1;
                            }
                            break;
                        case 10:
                            {
                                sh_t = DescriptiveMetaData.ShieldType.aluminum;
                                concentrationOfHafnium = 0.0;
                                density = physicalMetaData.densityOfAl; // g/cm^2
                                nlayers = 1;
                            }
                            break;
                        default:
                            {
                                sh_t = DescriptiveMetaData.ShieldType.monolayer;
                                concentrationOfHafnium = 0.0;
                                density = physicalMetaData.densityOfAl; // g/cm^2
                                nlayers = 1;
                            }
                            break;
                    }
                }
            }

            public class PhysicalMetaData
            {
                private readonly AdministrativeMetaData.DataBaseType db_t;
                public readonly double densityOfHf = 13.1; // g/cm^3
                public readonly double densityOfAl = 2.7; // g/cm^3
                public readonly double densityOfWater = 1.0; // g/cm^3
                public readonly double converionFactorOfBarn2m2 = 1.0e-28; // m^2
                public readonly double m2cm = 100.0; // 1 m = 100 cm
                public readonly double avogadroConst = 6.022140857e23; // N_a
                public readonly double HfgramsPerMole = 178.49; //  g / mol
                public readonly double crosssectionProton2Hf = 500e-3; // b
                public int nlayers;
                public double number_density;
                public double meanFreePath;
                public double sensitivityFactor = 0.0; // zzz -- for Fe ions and other heavy ions, it varies from 1 to 100 of protons' crosssection           
                public enum CrosssectionsType
                {
                    ionizations,
                    fragmentation,
                    isomer_excitation
                }
                public PhysicalMetaData()
                {

                }
                public PhysicalMetaData(int i)
                {
                    crosssectionProton2Hf = 500.0e-3 * converionFactorOfBarn2m2 * m2cm * m2cm; // 500 mb // zzz
                    number_density = densityOfHf * avogadroConst * (1.0 / HfgramsPerMole); // moles/cm^3
                    meanFreePath = 1.0 / (crosssectionProton2Hf * number_density); // cm
                    switch (i)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            break;
                        case 5:
                            {
                                nlayers = 3;
                            }
                            break;
                        default:
                            {
                                db_t = AdministrativeMetaData.DataBaseType.in_code;
                            }
                            break;
                    }
                    switch (db_t)
                    {
                        case AdministrativeMetaData.DataBaseType.SQL:
                            getSigmasfromSQL();
                            break;
                        case AdministrativeMetaData.DataBaseType.online:
                            getSigmasonLine();
                            break;
                        case AdministrativeMetaData.DataBaseType.ASCII:
                            getSigmasfromASCII();
                            break;
                        case AdministrativeMetaData.DataBaseType.in_code:
                            {
                                switch (nlayers)
                                {
                                    case 1:
                                        {
                                            for (int j = 0; j < nlayers; j++)
                                            {
                                                //if (shieldStructure.Materials.aluminum // still have to develop layered shield
                                                //    Theoretical.Method1();
                                                //                    if (layer[i].Material == StructuralMetaData.Materials.hafnium)
                                                //                        Theoretical.Method2();
                                                //                    if (layer[i].Material == StructuralMetaData.Materials.zirconium)
                                                //                        Theoretical.Method3();
                                            }
                                        }
                                        break;
                                    case 2:
                                        {
                                            for (int j = 0; j < nlayers; j++)
                                            {
                                                //if (shieldStructure.Materials.aluminum // still have to develop layered shield
                                                //    Theoretical.Method1();
                                                //                    if (layer[i].Material == StructuralMetaData.Materials.hafnium)
                                                //                        Theoretical.Method2();
                                                //                    if (layer[i].Material == StructuralMetaData.Materials.zirconium)
                                                //                        Theoretical.Method3();
                                                Theoretical.Method4();
                                            }
                                        }
                                        break;
                                    default:
                                        {
                                            Theoretical.SigmasAluminum();  // Al
                                        }
                                        break;
                                }
                            }
                            break;
                        default:
                            {
                                Theoretical.SigmasAluminum();  // Al
                            }
                            break;
                    }
                }
                public void getSigmasfromSQL() { }
                public void getSigmasonLine() { }
                public void getSigmasfromASCII() { }
            }

            static public class AdministrativeMetaData
            {
                public enum DataBaseType
                {
                    SQL,
                    online,
                    ASCII,
                    in_code
                }
            }

            static public class DescriptiveMetaData
            {
                public enum ShieldType
                {
                    aluminum,
                    layered,
                    monolayer,
                    alloy, // other than alloys below
                    hafnium,
                    hafnium_aluminum,
                    hafnium_zirconium_aluminum,
                    water,
                    hydrogen,
                    solid, // other than the solids here
                    liquid, // other than the liquids here
                    liquid_hydrogen
                }
            }

            public class StatisticalMetaData
            {
                public double ShieldEfficiency { get; set; }
            }

            internal static class Theoretical
            {
                public static void Method1()  // Hf
                { }
                public static void Method2()
                { // High thermal neutron absorption cross section (~600 imes zirconia)
                }
                public static void Method3()
                { }
                public static void Method4()
                {
                    SigmasAluminum();  // Al
                }
                public static void SigmasAluminum()
                {
                    // Al
                }
            }
        }
    }
}