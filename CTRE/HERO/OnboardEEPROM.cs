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
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace CTRE.HERO
{
    public class OnboardEEPROM
    {
        private const Cpu.Pin _ChipSelFlash = (Cpu.Pin)0x25;
        public const int CapacityBytes = 16 * 1024 * 1024 / 8; /* 16MBIT */

        const int kMaxByteProgramTimeMs = 3;
        const int kMaxByteErase4KTimeMs = 25;

        //byte kWriteStatusRegValue = 0x40; /* enable AAI */
        byte kWriteStatusRegValue = 0x00; /* enable byte mode*/

        private SPI _SPIDevice;
        private SPI.Configuration _Configuration;

        private static OnboardEEPROM _instance = null;

        byte[] _temp_4b = new byte[4];
        byte[] _temp_5b = new byte[5];
        byte[] _temp_6b = new byte[6];

        public static OnboardEEPROM Instance
        {
            get
            {
                if (_instance == null) { _instance = new OnboardEEPROM(); }
                return _instance;
            }
        }

        //Configure SPI for the Onboard EEPROM
        private OnboardEEPROM()
        {
            _Configuration = new SPI.Configuration(_ChipSelFlash, false, 0, 0, false, true, 8000, SPI.SPI_module.SPI1);
            _SPIDevice = new SPI(_Configuration);
        }

        private enum StatusRegisterBits
        {
            BUSY_BIT0 = 0x1,
            WriteEnabled_WEL_BIT1 = 0x02,
            BlockWriteProtect_BP0_BIT2 = 0x04,
            BlockWriteProtect_BP1_BIT3 = 0x08,
            BlockWriteProtect_BP2_BIT4 = 0x10,
            BlockWriteProtect_BP3_BIT5 = 0x20,
            AutoAddressIncrementProgrammingStatus_AAI_BIT6 = 0x40,
            BPReadOnly_BIT7 = 0x80,
        }

        private enum OpCode //Instuctions from SST25VF016B-75-4I-QAF
        {
            Read = 0x03,
            HighSpeedRead = 0x0b,
            _4KBErase = 0x20,
            _32KBErase = 0x52,
            _64KBErase = 0xd8,
            _ChipErase = 0x60,
            ByteProgram = 0x02,
            AAIWordProgram = 0xad,
            RDSR_ReadStatusReg = 0x05,
            EWSR_EnableWriteStatReg = 0x50,
            WRSR_WriteStatusReg = 0x01,
            WREN_WriteEn = 0x06,
            WRDI_WriteDisable = 0x04,
            RDID_ReadId = 0xab,
            JEDECID = 0x9f,
        };
        //---------------------- Registers ---------------------//
        //Read the status register and returns the byte
        private byte ReadStatusRegister()
        {
            byte[] Temp = new byte[1];
            byte[] TempIn = new byte[1];
            Temp[0] = (byte)OpCode.RDSR_ReadStatusReg;
            _SPIDevice.WriteRead(Temp, 0, 1, TempIn, 0, 1, 1);
            return TempIn[0];
        }

        //Enables Write Status Register
        private void EnableWriteStatusRegister()
        {
            byte[] Temp = new byte[1];
            Temp[0] = (byte)OpCode.EWSR_EnableWriteStatReg;
            _SPIDevice.Write(Temp);
        }

        //Writes a byte to the status register
        private void WriteStatusRegister(byte StatusIn)
        {
            byte[] Temp = new byte[2];
            Temp[0] = (byte)OpCode.WRSR_WriteStatusReg;
            Temp[1] = StatusIn;
            _SPIDevice.Write(Temp);
        }

        //Enables the Write Enable latch. Can also be used in place of EWSR
        private void WriteEnableLatch()
        {
            byte[] Temp = new byte[1];
            Temp[0] = (byte)OpCode.WREN_WriteEn;
            _SPIDevice.Write(Temp);
        }

        ////Disables the Write Enable latch
        //private void WriteDisable()
        //{
        //    byte[] Temp = new byte[1];
        //    Temp[0] = (byte)OpCode.WRDI_WriteDisable;
        //    _SPIDevice.Write(Temp);
        //}


        //---------------------- Info routines ---------------------//
        public ErrorCode ReadInfo(out int jedecID_BF_25_41, out byte ManufactureId_BF, out byte DeviceID_41)
        {
            _temp_4b[0] = (byte)OpCode.JEDECID;
            _temp_4b[1] = 0;
            _temp_4b[2] = 0;
            _temp_4b[3] = 0;
            _SPIDevice.WriteRead(_temp_4b, _temp_4b);

            jedecID_BF_25_41 = _temp_4b[1];
            jedecID_BF_25_41 <<= 8;
            jedecID_BF_25_41 |= _temp_4b[2];
            jedecID_BF_25_41 <<= 8;
            jedecID_BF_25_41 |= _temp_4b[3];

            _temp_6b[0] = (byte)OpCode.RDID_ReadId;
            _temp_6b[1] = 0;
            _temp_6b[2] = 0;
            _temp_6b[3] = 0;
            _temp_6b[4] = 0;
            _temp_6b[5] = 0;
            _SPIDevice.WriteRead(_temp_6b, _temp_6b);

            ManufactureId_BF = _temp_6b[4];
            DeviceID_41 = _temp_6b[5];

            if (jedecID_BF_25_41 != 0xBF2541)
                return ErrorCode.EEPROM_ERROR;
            if (ManufactureId_BF != 0xBF)
                return ErrorCode.EEPROM_ERROR;
            if (DeviceID_41 != 0x41)
                return ErrorCode.EEPROM_ERROR;

            return ErrorCode.OK;
        }
        public ErrorCode CheckHealth()
        {
            int jedecID_BF_25_41;
            byte ManufactureId_BF;
            byte DeviceID_41;
            return ReadInfo(out jedecID_BF_25_41, out ManufactureId_BF, out DeviceID_41);
        }
        //---------------------- Read routines ---------------------//

        public ErrorCode Read(ulong Address, byte[] toFill)
        {
            return Read(Address, toFill, toFill.Length);
        }
        public ErrorCode Read(ulong Address, byte[] toFill, int numBytesToRead)
        {
            byte[] Temp = new byte[4];
            byte Byte1 = (byte)((Address & 0xFFFFFF) >> 16);
            byte Byte2 = (byte)((Address & 0xFFFF) >> 8);
            byte Byte3 = (byte)(Address & 0xFF);
            Temp[0] = (byte)OpCode.Read;
            Temp[1] = Byte1;
            Temp[2] = Byte2;
            Temp[3] = Byte3;
            _SPIDevice.WriteRead(Temp, 0, 4, toFill, 0, numBytesToRead, 4);
            return ErrorCode.OK;
        }

        //---------------------- Erase routines ---------------------//
        public ErrorCode Erase4KB(ulong Address)
        {
            byte read = 0;
            /* check inputs */
            if ((Address + 4096) > CapacityBytes)
                return ErrorCode.CAN_INVALID_PARAM;
            /* must enable write first */
            WriteEnableLatch();
            /* make sure AAI and BPs are correct */
            WriteStatusRegister(kWriteStatusRegValue);
            /* must enable write first */
            WriteEnableLatch();
            /* read to make sure WEL is set */
            read = ReadStatusRegister();
            if ((read & (int)StatusRegisterBits.WriteEnabled_WEL_BIT1) == 0)
            {
                /* device is not in memory write enabled mode, something went wrong */
                return ErrorCode.EEPROM_ERROR;
            }
            /* clock erase command */
            byte[] Temp = new byte[4];
            Temp[0] = (byte)OpCode._4KBErase;
            Temp[1] = (byte)(Address >> 16);
            Temp[2] = (byte)(Address >> 8);
            Temp[3] = (byte)(Address);
            _SPIDevice.Write(Temp);
            /* wait for busy bit to clear */
            for (int i = 0; i < kMaxByteErase4KTimeMs; ++i)
            {
                /* small yield */
                Thread.Sleep(1);
                /* read status reg */
                read = ReadStatusRegister();
                if ((read & (int)StatusRegisterBits.BUSY_BIT0) != 0)
                {
                    /* busy bit is set, write is in progress */
                }
                else
                {
                    /* busy bit is cleared, NO write is in progress */
                    break;
                }
            }
            /* is it clear */
            if ((read & (int)StatusRegisterBits.BUSY_BIT0) == 0)
            {
                /* busy bit is cleared, NO write is in progress */
                return ErrorCode.OK;
            }
            return ErrorCode.EEPROM_ERROR;
        }

        //---------------------- Write routines ---------------------//
        public ErrorCode Write(ulong Address, params byte[] toWrite)
        {
            ErrorCode retval = ErrorCode.OK;
            byte read;
            /* check inputs */
            if ((Address + (ulong)toWrite.Length) > CapacityBytes)
                return ErrorCode.CAN_INVALID_PARAM;
            /* for each byte */
            foreach (byte byteValueToWrite in toWrite)
            {
                /* must enable write first */
                WriteEnableLatch();
                /* make sure AAI and BPs are correct */
                WriteStatusRegister(kWriteStatusRegValue);
                /* must enable write first */
                WriteEnableLatch();
                /* read to make sure WEL is set */
                read = ReadStatusRegister();
                if ((read & (int)StatusRegisterBits.WriteEnabled_WEL_BIT1) == 0)
                {
                    /* device is not in memory write enabled mode, something went wrong */
                    return ErrorCode.EEPROM_ERROR;
                }
                /* clock erase command */
                _temp_5b[0] = (byte)OpCode.ByteProgram;
                _temp_5b[1] = (byte)(Address >> 16);
                _temp_5b[2] = (byte)(Address >> 8);
                _temp_5b[3] = (byte)(Address);
                _temp_5b[4] = (byte)(byteValueToWrite);
                _SPIDevice.Write(_temp_5b);

                /* wait for busy bit to clear */
                for (int i = 0; i < kMaxByteProgramTimeMs; ++i)
                {
                    /* small yield */
                    Thread.Sleep(1);
                    /* read status reg */
                    read = ReadStatusRegister();
                    if ((read & (int)StatusRegisterBits.BUSY_BIT0) != 0)
                    {
                        /* busy bit is set, write is in progress */
                    }
                    else
                    {
                        /* busy bit is cleared, NO write is in progress */
                        break;
                    }
                }
                /* is it clear */
                if ((read & (int)StatusRegisterBits.BUSY_BIT0) != 0)
                {
                    /* busy bit is set, we've timed out */
                    return ErrorCode.EEPROM_TIMED_OUT;
                }
                /* increment address */
                ++Address;
            }
            return retval;
        }


#if false
        private void Auto_Add_IncA(ulong Address, byte Data1, byte Data2)
        {
            byte[] Temp = new byte[6];
            byte Byte1 = (byte)((Address & 0xFFFFFF) >> 16);
            byte Byte2 = (byte)((Address & 0xFFFF) >> 8);
            byte Byte3 = (byte)(Address & 0xFF);
            Wait_Busy();
            WREN();
            Temp[0] = (byte)OpCode.AAIWordProgram;
            Temp[1] = Byte1;
            Temp[2] = Byte2;
            Temp[3] = Byte3;
            Temp[4] = Data1;
            Temp[5] = Data2;
            _SPIDevice.Write(Temp);
            Delay();
        }

        private void Auto_Add_IncB(byte Data1, byte Data2)
        {
            WREN_AAI_Check();
            byte[] Temp = new byte[3];
            Temp[0] = (byte)OpCode.AAIWordProgram;
            Temp[1] = Data1;
            Temp[2] = Data2;
            _SPIDevice.Write(Temp);
            Delay();
        }
        //Must be first write and writes a single 4 bytes or int
        public void WriteHisto(ulong Address, uint FirstData, uint[] DataToWrite, int length, uint Checksum)
        {
            byte[] Datas = new byte[4];
            //Int Dat Conveted
            Datas[0] = (byte)((FirstData >> 24) & 0xFF);
            Datas[1] = (byte)((FirstData >> 16) & 0xFF);
            Datas[2] = (byte)((FirstData >> 8) & 0xFF);
            Datas[3] = (byte)(FirstData & 0xFF);

            //Clear the Block protection bits to allow writes
            WREN();
            Delay();
            WRSR(0x00);
            Delay();
            //WREN again and clear the sector...
            Sector_Erase(Address);//Sector must be deleted prior to writing to it
            Auto_Add_IncA(Address, Datas[0], Datas[1]);
            Auto_Add_IncB(Datas[2], Datas[3]);

            //public void WriteHistoData(uint[] DataToWrite, int length)        
            for (int i = 0; i < length; i++)
            {
                uint CurrentData = DataToWrite[i];
                byte[] temp = new byte[4];
                temp[0] = (byte)((CurrentData >> 24) & 0xFF);
                temp[1] = (byte)((CurrentData >> 16) & 0xFF);
                temp[2] = (byte)((CurrentData >> 8) & 0xFF);
                temp[3] = (byte)(CurrentData & 0xFF);
                Auto_Add_IncB(temp[0], temp[1]);
                Auto_Add_IncB(temp[2], temp[3]);
            }

            //public void WriteHistoFinish(uint Checksum)          
            byte[] Data4 = new byte[4];
            Data4[0] = (byte)((Checksum >> 24) & 0xFF);
            Data4[1] = (byte)((Checksum >> 16) & 0xFF);
            Data4[2] = (byte)((Checksum >> 8) & 0xFF);
            Data4[3] = (byte)(Checksum & 0xFF);
            Auto_Add_IncB(Data4[0], Data4[1]);
            Auto_Add_IncB(Data4[2], Data4[3]);
            //This is the reason why this function is differnt
            WRDI();
        }

        //Checks the status register before proceeding
        private void Wait_Busy(int timeout = 100)
        {
            while ((ReadStatusRegister() & 0x03) == 0x03) //Waste time until not busy
            {
                System.Threading.Thread.Sleep(10);

                timeout -= 10;
                if (timeout < 0)
                {
                    //Autobot.Instrum.FailEepromEvent(-1);
                    break;
                }
            }
        }

        private void WREN_AAI_Check()
        {
            byte Byte;
            Byte = ReadStatusRegister();  /* read the status register */
            if (Byte != 0x42)       /* verify that AAI and WEL bit is set */
            {
                //while (true) // todo remove this, only print once.
                {
                    Debug.Print("Restart? Need to change check");
                }
            }
        }

        void DelayErase()
        {
            Thread.Sleep(100);
        }
        void Delay()
        {
            Thread.Sleep(1);
        }
#endif
    }
}