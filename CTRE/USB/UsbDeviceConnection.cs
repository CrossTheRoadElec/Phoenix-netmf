using System;

namespace CTRE.Phoenix
{
    public enum UsbDeviceConnection /* rename the enum so old projects fail to build */
    {
        NotConnected,
        Connected,
        /* TODO: Types that we want to implement */
        /* TODO: everywhere we use *Connected, instead use the types for watchdog. */
        //HID_Generic
        //Xbox
        //NotConnected
    }
}
