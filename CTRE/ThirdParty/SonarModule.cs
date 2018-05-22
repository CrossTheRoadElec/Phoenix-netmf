/*
*  Software License Agreement
*
* Copyright (C) Cross The Road Electronics.  All rights
* reserved.
* 
* Cross The Road Electronics (CTRE) licenses to you the right to 
* use, publish, and distribute copies of CRF (Cross The Road) firmware files (*.crf) and Software
* API Libraries ONLY when in use with Cross The Road Electronics hardware products.
* 
* THE SOFTWARE AND DOCUMENTATION ARE PROVIDED "AS IS" WITHOUT
* WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT
* LIMITATION, ANY WARRANTY OF MERCHANTABILITY, FITNESS FOR A
* PARTICULAR PURPOSE, TITLE AND NON-INFRINGEMENT. IN NO EVENT SHALL
* CROSS THE ROAD ELECTRONICS BE LIABLE FOR ANY INCIDENTAL, SPECIAL, 
* INDIRECT OR CONSEQUENTIAL DAMAGES, LOST PROFITS OR LOST DATA, COST OF
* PROCUREMENT OF SUBSTITUTE GOODS, TECHNOLOGY OR SERVICES, ANY CLAIMS
* BY THIRD PARTIES (INCLUDING BUT NOT LIMITED TO ANY DEFENSE
* THEREOF), ANY CLAIMS FOR INDEMNITY OR CONTRIBUTION, OR OTHER
* SIMILAR COSTS, WHETHER ASSERTED ON THE BASIS OF CONTRACT, TORT
* (INCLUDING NEGLIGENCE), BREACH OF WARRANTY, OR OTHERWISE
*/

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace CTRE.ThirdParty
{
    public class SonarModule
    {
        private I2CDevice MyI2C = null;
        private I2CDevice.I2CTransaction[] WriteCommand;
        private I2CDevice.I2CTransaction[] ReadCommand;
        int ReadCheck = 0;

        /* I2C configuration for Sonar Module */
        I2CDevice.Configuration SonarConfig;

        /* Various units for Range */
        public enum RangeType
        {
            Inches = 0x50,
            Centimeters = 0x51,
            MicroSeconds = 0x52     /* Time it tackes to get the frequency back */
        }

        /* Sonar I2C write function
         * 
         * @param   Address     Address written to
         * @param   Data        Data being written
         */
        private void I2CWrite(byte Address, byte Data)
        {
            /* make sure our config is selected */
            MyI2C.Config = SonarConfig;
            WriteCommand = new I2CDevice.I2CTransaction[1];
            WriteCommand[0] = I2CDevice.CreateWriteTransaction(new byte[2]);
            WriteCommand[0].Buffer[0] = Address;
            WriteCommand[0].Buffer[1] = Data;
            MyI2C.Execute(WriteCommand, 100);
        }

        /* Sonar I2C read function
         * 
         * @param   Address     Address being read from
         * @param   Data        2 bytes of data pulled from Address
         * @return  bool        bool that that tells us if we got data or not
         */
        private bool I2CRead(byte Address, byte[] Data)
        {
            /* make sure our config is selected */
            MyI2C.Config = SonarConfig;
            ReadCommand = new I2CDevice.I2CTransaction[2];
            ReadCommand[0] = I2CDevice.CreateWriteTransaction(new byte[] { Address });
            ReadCommand[1] = I2CDevice.CreateReadTransaction(Data);
            ReadCheck = MyI2C.Execute(ReadCommand, 100);
            return (ReadCheck == ReadCommand.Length) ? true : false;
        }

        /* Sonar Module constructor
         * 
         * @param   i2cdev          Current I2C device
         * @param   DeviceAddress   Address of I2C device
         * @param   ClockRate       I2C clockrate in Khz
         */
        public SonarModule(I2CDevice i2cdev, byte DeviceAddress, int ClockRate)
        {
            SonarConfig = new I2CDevice.Configuration(DeviceAddress, ClockRate);
            MyI2C = i2cdev;
        }

        //Set the analogue gain. If SRF10, Gain should be 0-5. If SRF08, Gain should be 0-20.
        public void SetGain(byte Gain)
        {
            byte GainAddress = 0x01;
            I2CWrite(GainAddress, Gain);
        }

        //Set the Range Distance once prior to ranging
        // 24 = 1m
        // 48 = 2m
        // 93 = 4m
        // 255 = 11m (Max Distance)
        // ((Distance x 43mm) + 43mm)
        public void SetDistance(byte Distance)
        {
            byte RangeAddress = 0x02;
            I2CWrite(RangeAddress, Distance);
        }

        //Call this to range once
        public void InitRanging(RangeType RangeType)
        {
            byte CommandAddress = 0x00;
            byte type = (byte)RangeType;
            I2CWrite(CommandAddress, type);
        }

        //Call This to read the range once
        public bool ReadRange(ref uint sample)
        {
            bool retval = true;
            //int Count = 0;
            uint SonarSample = 0;
            byte HighAddress = 0x02;
            byte LowAddress = 0x03;
            byte[] HighByte = new byte[1];
            byte[] LowByte = new byte[1];

            retval &= I2CRead(HighAddress, HighByte);
            retval &= I2CRead(LowAddress, LowByte);

            SonarSample = HighByte[0];
            SonarSample <<= 8;
            SonarSample |= LowByte[0];

            sample = SonarSample;

            return retval;
        }

        public void DiscardI2CDev()
        {
            MyI2C = null;
        }

        public void ReInitI2CDev(I2CDevice i2cdev)
        {
            MyI2C = i2cdev;
        }
    }
}
