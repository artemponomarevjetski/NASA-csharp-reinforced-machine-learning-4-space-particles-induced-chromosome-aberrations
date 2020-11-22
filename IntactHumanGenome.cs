using System;

namespace GraficDisplay
{
    internal static class IntactHumanGenome
    {
        public static readonly double monomerSize = 0.002; // Mbp
        public static readonly int nObjs = 46; // intact genome
        public static readonly double[] NC = // chromosome lengths in Mbp (q+p)   
        {
            128 + 135,  128 + 135,  // #1 chromo pair,                                    
            99 + 156,   99 + 156,   // #2                        
            99 + 115,   99 + 115,   // #3                     
            56 + 147,   56 + 147,   // #4         
            52 + 142,   52 + 142,   // #5           
            65 + 118,   65 + 118,   // #6          
            65 + 106,   65 + 106,   // #7          
            50 + 105,   50 + 105,   // #8         
            51 + 94,    51 + 94,    // #9 
            44 + 100,   44 + 100,   // #10     
            58 + 86,    58 + 86,    // #11        
            39 + 104,   39 + 104,   // #12        
            16 + 98,    16 + 98,    // #13         
            16 + 93,    16 + 93,    // #14         
            17 + 89,    17 + 89,    // #15         
            39 + 59,    39 + 59,    // #16         
            28 + 64,    28 + 64,    // #17          
            20 + 65,    20 + 65,    // #18        
            30 + 37,    30 + 37,    // #19         
            31 + 41,    31 + 41,    // #20      
            11 + 39,    11 + 39,    // #21         
            13 + 43,    13 + 43,    // #22         
            62 + 102,    // X chromosome
            13 + 46     // Y chromosome          
        };

        public static readonly double[] CM = // positions of centromeres in Mbp    
        {
            128,    128,    // #1 chromo pair             
            99,     99,     // #2                        
            99,     99,     // #3                      
            56,     56,     // #4                
            52,     52,     // #5              
            65,     65,     // #6                
            65,     65,     // #7                
            50,     50,     // #8                
            51,     51,     // #9        
            44,     44,     // #10            
            58,     58,     // #11               
            39,     39,     // #12               
            16,     16,     // #13                
            16,     16,     // #14                
            17,     17,     // #15                
            39,     39,     // #16                
            28,     28,     // #17                 
            20,     20,     // #18               
            30,     30,     // #19                
            31,     31,     // #20             
            11,     11,     // #21                
            13,     13,     // #22           
            62,             // X chromosome  
            13              // Y chromosome           
        };
        //
        public static double DownstreamEndPosition(int chromonubmer)
        {
            double dtemp = 0.0;
            for (int i = 0; i < chromonubmer; i++)
                dtemp += NC[i];
            return dtemp;
        }
        //
        public static int WholeGenome()
        {
            double dtemp = 0.0;
            for (int i = 0; i < nObjs; i++)
                dtemp += Convert.ToDouble(NC[i]);
            return Convert.ToInt32(dtemp / monomerSize);
        }
    }
}

