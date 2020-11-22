using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GraficDisplay
{
    internal static partial class Visualize
    {
        //
        public static string AberrationAnalysis(List<Object> listObjs)
        {
            string str = "";
            if (listObjs.Count != 0)
            {
                int i = 0;
                Object[] o_temp = new Object[listObjs.Count];
                foreach (Object o in listObjs)
                {
                    if (o.md.O_type == MetaData.ObjectType.intact_chromo || IsDAPI(o))
                    {
                        o_temp[i++] = o;
                    }
                }

                for (int j = 0; j < i; j++)
                {
                    listObjs.Remove(o_temp[j]); // correct procedure for multiple object removal    
                }

                if (listObjs.Count != 0)
                {
                    int[] idx_obj = new int[listObjs.Count];
                    int[] idx_chromo = new int[3];
                    foreach (Object o in listObjs)
                    {
                        foreach (Band b in o.md.chromo_bands)
                        {
                            if (IsChromoPainted(b.Chromo_num))
                            {
                                idx_chromo[WhichColor(b.Chromo_num)] = 1;
                            }
                        }
                    }

                    int k = 0;
                    while (true) // this executes once in this function
                    {
                        if (idx_chromo[k] != 0)
                        {
                            idx_obj = Recursion(idx_obj, k, idx_chromo, listObjs);
                            break;
                        }
                        k++;
                    }
                    if (interactingFragmentSet != null)
                    {
                        interactingFragmentSet = null;
                    }

                    o_temp = null;
                    o_temp = new Object[listObjs.Count];
                    interactingFragmentSet = new List<Object>();
                    i = 0;
                    for (int j = 0; j < idx_obj.Length; j++)
                    {
                        if (idx_obj[j] == 1)
                        {
                            interactingFragmentSet.Add(listObjs[j]);
                            o_temp[i++] = listObjs[j];
                        }
                    }
                    // remove the set from the main list                   
                    for (int j = 0; j < i; j++)
                    {
                        listObjs.Remove(o_temp[j]); // correct procedure for multiple object removal
                    }

                    str += AnalyzeSet(interactingFragmentSet) + "\t";
                }
            }
            return str;
        }
        //
        public static int[] Recursion(int[] idx_obj, int k, int[] idx_chromo, List<Object> listObjs)
        {
            for (int j = 0; j < 3; j++)
            {
                idx_chromo[j] = 0;
            }

            idx_chromo[k] = 1;
            for (int j = 0; j < 3; j++)
            {
                if (idx_chromo[j] != 0)
                {
                    foreach (Object o in listObjs)
                    {
                        if (idx_obj[listObjs.IndexOf(o)] == 0)
                        {
                            foreach (Band b in o.md.chromo_bands)
                            {
                                if (IsChromoPainted(b.Chromo_num))
                                {
                                    if (WhichColor(b.Chromo_num) == k)
                                    {
                                        idx_obj[listObjs.IndexOf(o)] = 1;
                                        foreach (Band b1 in o.md.chromo_bands)
                                        {
                                            int nbandsofsamecolor = 1;
                                            if (b != b1)
                                            {
                                                if (IsChromoPainted(b1.Chromo_num))
                                                {
                                                    if (WhichColor(b.Chromo_num) != k)
                                                    {
                                                        if (idx_chromo[WhichColor(b1.Chromo_num)] == 0)
                                                        {
                                                            idx_chromo[WhichColor(b1.Chromo_num)] = 1;
                                                            idx_obj = Recursion(idx_obj, WhichColor(b1.Chromo_num), idx_chromo, listObjs);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        nbandsofsamecolor++;
                                                        if (nbandsofsamecolor == o.md.chromo_bands.Count)
                                                        {
                                                            idx_obj[listObjs.IndexOf(o)] = 1;
                                                            return idx_obj;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return idx_obj;
        }
        //
        public static int WhichColor(int ch)
        {
            switch (ch)
            {
                case (1 - 1):
                case (2 - 1):
                    {
                        return 0; // first pair, red
                    }
                //          break;
                case (3 - 1):
                case (4 - 1):
                    {
                        return 1; // second pair, green
                    }
                //         break;
                case (7 - 1):
                case (8 - 1):
                    {
                        return 2; // 3rd pair, yellow
                    }
                //    break;
                default:
                    {
                        return 3; // DAPI
                    }
                    //  break;
            }
        }
        //
        public static bool IsChromoPainted(int ch)
        {
            switch (ch)
            {
                case (1 - 1):
                case (2 - 1):
                    {
                        return true; // first pair, red
                    }
                //          break;
                case (3 - 1):
                case (4 - 1):
                    {
                        return true; // second pair, green
                    }
                //         break;
                case (7 - 1):
                case (8 - 1):
                    {
                        return true; // 3rd pair, yellow
                    }
                //    break;
                default:
                    {
                        return false; // DAPI
                    }
                    //  break;
            }
        }
        //
        public static bool IsDAPI(Object o)
        {
            foreach (Band b in o.md.chromo_bands)
            {
                switch (b.Chromo_num)
                {
                    case (1 - 1):
                    case (2 - 1):
                        {
                            return false; // first pair, red
                        }
                    //          break;
                    case (3 - 1):
                    case (4 - 1):
                        {
                            return false; // second pair, green
                        }
                    //         break;
                    case (7 - 1):
                    case (8 - 1):
                        {
                            return false; // 3rd pair, yellow
                        }
                        //    break;       
                }
            }

            return true;// zz666            
        }
        //
        public static string AnalyzeSet(List<Object> interactingFragmentSet)  // CA classification
        {
            // An interacting set can have  4 visible colors of objects: red, green, yellow and blue.It does not matter how many  actual constituent chromosomes are in it.The anonymity  of chromosomes  are of two types, a bunch of indistinguishable DAPI chromosomes and the painted pairs.For example, one read chromosome  is indistinguishable from the other red.Anonymity of chromosomes obscures the real count, so the program has to count like the experimentalist.
            // An object is either a ring or a linear chromosome.
            // All objects  in an interacting set have  to be interlinked by sharing pieces of at least one chromosome between at least two objects in the set.
            // Both the algo and a human can count the number of color-constituent fragments  in such set.  There are 4 possible colors in  color - constituent  fragments: read, yellow, green and blue.
            // The number of complex exchanges  in one interacting set is, thus,  the number of visible colors among the color-constituent fragments,  minus one, only if there is the blue color in the set.
            // If an interacting set is relatively simple or the  non - DAPI fragments are connected to each other only through DAPI fragments, the set produced counts toward simple exchanges. 
            // That’s just the way she counts.
            // It’s not unique.The algo can be adjusted to other experimentalists as well.
            // Conclusions, it appears that the complex exchanges, as defined by the rule “at least 2 chromosomes and at least 3 DSBs”, are more frequent than previously thought . In a M_FISH experiment, complex exchanges always lose counts because of DAPI fragments. But most importantly the interacting sets “fall apart” often, and instead are counted as  0, 1, 2, etc.simple exchanges. We believe the g -factor used to extrapolate the data to the whole genome is underestimated and should be  higher for complex exchanges than for the simple exchanges, and higher than its traditional value.We observed that  complex exchanges  are more sensitive to the presence of  “anonymity” in  this 4 - color scheme, as the interlinked sets are missed in a  higher proportion than just the proportion of DAPI stained chromosomes  in the cell culture.
            //
            string str = "";
            int j = 0;
            int[] ii = new int[3]; // indexation of constituent colors, DAPI is not a constituent color
            int nbands = 0;
            foreach (Object o in interactingFragmentSet)
            {
                foreach (Band b in o.md.chromo_bands)
                {
                    switch (b.Chromo_num)
                    {
                        case (1 - 1):
                        case (2 - 1):
                            {
                                ii[0] = 1; // first pair, red
                            }
                            break;
                        case (3 - 1):
                        case (4 - 1):
                            {
                                ii[1] = 1; // second pair, green
                            }
                            break;
                        case (7 - 1):
                        case (8 - 1):
                            {
                                ii[2] = 1; // 3rd pair, yellow
                            }
                            break;
                        default:
                            break;
                    }
                    if (IsChromoPainted(b.Chromo_num))
                    {
                        nbands++;
                    }
                }
            }
            // annotate
            str += "\n-- constituent colors: ";
            int l = 0;
            for (int i = 0; i < ii.Length; i++)
            {
                if (ii[i] != 0)
                {
                    str += ConstituentColor(i) + ", ";
                    l++;
                }
            }
            str += "total = " + l.ToString() + "\t";
            if (l == 0)
            {
                str += "because DAPI fragments are indistinguishable\n";
            }
            //
            switch (ii.Sum()) // by the number of color-constituent fragments
            {
                case 0:
                    str += "+0 breaks\n";
                    break;
                case 1: // one constituent color
                    {
                        switch (interactingFragmentSet.Count) // by the number of fragments in the set
                        {
                            case 1:
                                {
                                    if (interactingFragmentSet[0].md.O_type == MetaData.ObjectType.ring) // a ring
                                    {
                                        str += "\timproperly repaired fragment\t\t+1 ring\n";
                                    }
                                    else
                                    {
                                        if (interactingFragmentSet[0].md.O_type == MetaData.ObjectType.fully_repaired)
                                        {
                                            str += "\tproperly repaired fragment\t\t+1 intact chromosome\n";
                                        }
                                    }
                                }
                                break;
                            case 2:
                                {
                                    if ((interactingFragmentSet[0].md.O_type == MetaData.ObjectType.ring && interactingFragmentSet[1].md.O_type != MetaData.ObjectType.ring)
                                        || (interactingFragmentSet[0].md.O_type != MetaData.ObjectType.ring && interactingFragmentSet[1].md.O_type == MetaData.ObjectType.ring)) // a deletion and a ring
                                    {
                                        str += "\t+1 simple exchange\n";
                                    }
                                    else
                                    {
                                        if (AreAllFragmentsPainted(interactingFragmentSet))
                                        {
                                            str += "\t+1 break\n";
                                        }
                                        else
                                        {
                                            if (nbands == 2)
                                            {
                                                str += "\t+" + ii.Sum() + " simple exchanges\n";
                                            }
                                            else
                                            {
                                                str += "\t+" + ii.Sum() + " complex exchanges" + "\n";
                                            }
                                        }
                                    }
                                }
                                break;
                            default:
                                {
                                    if (AreAllFragmentsPainted(interactingFragmentSet))
                                    {
                                        str += "\t+" + (interactingFragmentSet.Count - 1).ToString() + " breaks\n";
                                    }
                                    else
                                    {
                                        if (nbands == 2)
                                        {
                                            str += "\t+" + ii.Sum() + " simple exchanges\n";
                                        }
                                        else
                                        {
                                            str += "\t+" + ii.Sum() + " complex exchanges" + "\n";
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    break;
                case 2: // two constituent colors
                    {
                        switch (interactingFragmentSet.Count) // by the number of fragments in the set
                        {
                            case 1:
                                {
                                    MessageBox.Show("Not posisble to have 2 constituent colors and only one fragment in a set!");
                                }
                                break;
                            case 2:
                                {
                                    if (AreAllFragmentsPainted(interactingFragmentSet))
                                    {
                                        str += "\t+" + ii.Sum() + " simple exchanges\n";
                                    }
                                    else
                                    {
                                        if (nbands == 4)
                                        {
                                            str += "\t+" + ii.Sum() + " simple exchanges\n";
                                        }
                                        else
                                        {
                                            str += "\t+" + ii.Sum() + " complex exchanges" + "\n";
                                        }
                                    }
                                }
                                break;
                            default:
                                {
                                    if (AreAllFragmentsPainted(interactingFragmentSet))
                                    {
                                        str += "\t+" + ii.Sum() + " simple exchanges\t";
                                    }
                                    else
                                    {
                                        if (nbands == 4)
                                        {
                                            str += "\t+" + ii.Sum() + " simple exchanges\n";
                                        }
                                        else
                                        {
                                            str += "\t+" + ii.Sum() + " complex exchanges" + "\n";
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    break;
                default: // >2 constituent colors
                    {
                        switch (interactingFragmentSet.Count) // by the number of fragments in the set
                        {
                            case 1:
                                {
                                    MessageBox.Show("Not posisble to have >2 constituent colors and only one fragment in a set!");
                                }
                                break;
                            case 2:
                                {
                                    if (AreAllFragmentsPainted(interactingFragmentSet))
                                    {
                                        str += "\t+" + ii.Sum() + " simple exchanges\n";
                                    }
                                    else
                                    {
                                        if (nbands == 2 * ii.Sum())
                                        {
                                            str += "\t+" + ii.Sum() + " simple exchanges\n";
                                        }
                                        else
                                        {
                                            str += "\t+" + ii.Sum() + " complex exchanges" + "\n";
                                        }
                                    }
                                }
                                break;
                            default:
                                {
                                    if (AreAllFragmentsPainted(interactingFragmentSet))
                                    {
                                        str += "\t+" + ii.Sum() + " simple exchanges\n";
                                    }
                                    else
                                    {
                                        if (nbands == 2 * ii.Sum()) // zzz666
                                        {
                                            str += "\t+" + ii.Sum() + " simple exchanges\n";
                                        }
                                        else
                                        {
                                            str += "\t+" + ii.Sum() + " complex exchanges" + "\n";
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    break;
            }
            //
            foreach (Object o in interactingFragmentSet)
            {
                if (j > 0)
                {
                    str += "+";
                }

                str += "(";
                int i = 0;
                foreach (Band b in o.md.chromo_bands)
                {
                    if (i > 0)
                    {
                        str += "+";
                    }

                    if (b.Chromo_num + 1 == 45)
                    {
                        str += "X/" + b.Ordinal_number.ToString();
                    }
                    else
                    {
                        if (b.Chromo_num + 1 == 46)
                        {
                            str += "Y/" + b.Ordinal_number.ToString();
                        }
                        else
                        {
                            str += (b.Chromo_num + 1).ToString() + "/" + b.Ordinal_number.ToString();
                        }
                    }
                    i++;
                }
                str += ")";
                j++;
            }
            return str + Environment.NewLine;
        }
        //
        public static string ConstituentColor(int i)
        {
            switch (i)
            {
                case 0:
                    return "red";
                //         break;
                case 1:
                    return "green";
                //           break;
                case 2:
                    return "yellow";
                //         break;
                case 3:
                    return "DAPI";
                //      break;
                default:
                    return "";
                    //      break;                        
            }
        }
        //public static int NumberOfDAPIstained(List<Object> interactingFragmentSet)
        //{
        //    int[] ii = new int[46];
        //    foreach (Object o in interactingFragmentSet)
        //        foreach (Band b in o.md.chromo_bands)
        //            if (b.Color == System.Drawing.Color.DarkBlue)
        //                ii[b.Chromo_num] = 1;
        //    return ii.Sum();
        //}
        //
        public static bool AreAllFragmentsPainted(List<Object> interactingFragmentSet)
        {
            foreach (Object o in interactingFragmentSet)
            {
                foreach (Band b in o.md.chromo_bands)
                {
                    if (b.Color == System.Drawing.Color.DarkBlue)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        //
        public static string ChromoLabel(int i)
        {
            string str;
            if (i != 45 - 1 && i != 46 - 1)
            {
                str = (i + 1).ToString();
            }
            else
            {
                if (i == 45 - 1)
                {
                    str = "X";
                }
                else
                {
                    str = "Y";
                }
            }
            return str;
        }
    }
}