using System;
using System.Windows.Forms;

namespace GraficDisplay
{
    public partial class MainForm : Form
    {
        private void CNSRadDamageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // collaboration withn S.C. and T.W.
        }

        private void ChronicDoseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Run Chromo Aberr's from chronic rad");
            Genome g = new Genome();
            g.Initialize();
            g.Do();
            g.Finish();
        }

        private void RadiationShieldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Radiation rad = new Radiation(); // or Radiation(nShields);
            MessageBox.Show("The simulation is completed: " + rad.TransportThroughShield());
            rad.Finish();
        }

        private void RBEmaxBasedStrategyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SpaceMission sm = new SpaceMission();
            sm.SimulateSpaceMisson(SpaceMission.Destinations.Moon);
            sm.Finish();
        }

        public partial class SpaceMission
        {
            public enum Destinations { Mars, Venus, Moon, Jupiter, Asteroid, LibrationPoint };
            private Destinations destination;
            public SpaceMission() { }
            public SpaceMission(Destinations d)
            {
                destination = d;
            }
            public void SimulateSpaceMisson(Destinations destination)
            {
                RBEmaxSensitivityAnalysis RBEmax = new RBEmaxSensitivityAnalysis();
                RBEmax.GetRBEmaxFromChromoAberrations();
            }

            public void Finish() { }

            public class RBEmaxSensitivityAnalysis
            {
                public void GetRBEmaxFromChromoAberrations() { }
                public void RBEmaxAlternative() { } // this could just be a DB from literature
            }

            public class LocalSpaceEnvironment
            {
                public double fractionOfFluxAbovePlanetaryHorizon = 0.5; // may depend on mountains
                public double neutronSurfaceAlbedo, protonSurfaceAlbedo;
                public LocalSpaceEnvironment() { }
                public LocalSpaceEnvironment(Destinations d)
                {
                    switch (d)
                    {
                        case Destinations.Moon:
                            { // zzz
                                neutronSurfaceAlbedo = 0.01; //  * totalFlux; -- need total flux 
                                protonSurfaceAlbedo = 0.01; //  * totalFlux; -- need total flux 
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}