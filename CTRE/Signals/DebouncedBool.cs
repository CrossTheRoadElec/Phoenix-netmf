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
namespace CTRE.Phoenix.Signals
{
    /**
     * 
     */
    public class DebouncedBool : IBoolInputSignal, IBoolOutputSignal
    {
        public bool Value { get; private set; }

        public int TrueCount { get; set; }
        public int FalseCount { get; set; }
        public int Threshold { get; set; }

        public DebouncedBool(int threshold, bool initialValue = false)
        {
            Threshold = threshold;
            Value = initialValue;
        }

        void Up()
        {
            if (TrueCount < Threshold)
            {
                ++TrueCount;
                Value = true;
            }
            FalseCount = 0;
        }
        void Down()
        {
            if (FalseCount < Threshold)
            {
                ++FalseCount;
                Value = false;
            }
            TrueCount = 0;
        }
        public void Set(bool value)
        {
            if (value)
                Up();
            else
                Down();
        }
        public void Reset(bool value)
        {
            Value = value;
            FalseCount = 0;
            TrueCount = 0;
        }
    }
}
