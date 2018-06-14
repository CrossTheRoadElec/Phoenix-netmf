using System;
using Microsoft.SPOT;
using CTRE.Phoenix.LowLevel;

namespace CTRE.Phoenix
{
     public class CustomParamConfiguration {
        public int customParam_0;
        public int customParam_1;
        public CustomParamConfiguration() {
            customParam_0 = 0;
            customParam_1 = 0;
        }
        public string ToString(ref string prependString) {
            string retstr = prependString + ".customParam_0 = " + customParam_0.ToString() + ";\n";
            retstr += prependString + ".customParam_1 = " + customParam_1.ToString() + ";\n";
    
            return retstr;
        }
    };
}
