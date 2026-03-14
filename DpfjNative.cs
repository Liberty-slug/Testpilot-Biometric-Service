using System;
using System.Runtime.InteropServices;

namespace BiometricService
{
    public static class DpfjNative
    {
        public const int DPFJ_SUCCESS = 0;
        public const int DPFJ_FMD_ANSI_378_2004 = 0x001B0001;

        [DllImport("dpfj.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int dpfj_compare(
            int fmd1_type,
            byte[] fmd1,
            uint fmd1_size,
            uint fmd1_view_idx,
            int fmd2_type,
            byte[] fmd2,
            uint fmd2_size,
            uint fmd2_view_idx,
            out uint score);

        public static bool TryCompare(
            byte[] fmd1,
            byte[] fmd2,
            out uint score)
        {
            score = uint.MaxValue;
            if (fmd1 == null || fmd2 == null || fmd1.Length == 0 || fmd2.Length == 0)
                return false;

            var status = dpfj_compare(
                DPFJ_FMD_ANSI_378_2004,
                fmd1,
                (uint)fmd1.Length,
                0,
                DPFJ_FMD_ANSI_378_2004,
                fmd2,
                (uint)fmd2.Length,
                0,
                out score);

            return status == DPFJ_SUCCESS;
        }
    }
}
