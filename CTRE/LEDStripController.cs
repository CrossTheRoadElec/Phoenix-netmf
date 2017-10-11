/*
 *  Software License Agreement
 *
 * Copyright (C) Cross The Road Electronics.  All rights
 * reserved.
 * 
 * Cross The Road Electronics (CTRE) licenses to you the right to 
 * use, publish, and distribute copies of CRF (Cross The Road) firmware files (*.crf) and Software
 *API Libraries ONLY when in use with Cross The Road Electronics hardware products.
 * 
 * THE SOFTWARE AND DOCUMENTATION ARE PROVIDED "AS IS" WITHOUT
 * WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT
 * LIMITATION, ANY WARRANTY OF MERCHANTABILITY, FITNESS FOR A
 * PARTICULAR PURPOSE, TITLE AND NON-INFRINGEMENT. IN NO EVENT SHALL
 * CROSS THE ROAD ELECTRONICS BE LIABLE FOR ANY INCIDENTAL, SPECIAL, 
 * INDIRECT OR CONSEQUENTIAL DAMAGES, LOST PROFITS OR LOST DATA, COST OF
 * PROCUREMENT OF SUBSTITUTE GOODS, TECHNOLOGY OR SERVICES, ANY CLAIMS
 * BY THIRD PARTIES (INCLUDING BUT NOT LIMITED TO ANY DEFENSE
 * THEREOF), ANY CLAIMS FOR INDEMNITY OR CONTRIBUTION, OR OTHER
 * SIMILAR COSTS, WHETHER ASSERTED ON THE BASIS OF CONTRACT, TORT
 * (INCLUDING NEGLIGENCE), BREACH OF WARRANTY, OR OTHERWISE
 */
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace CTRE.Phoenix
{
    public class LEDStripController
    {
        PWM[] _pwms = new PWM[3]; // rgb

        float[] _dutyCycle = new float[3]; // rgb
        float[] _trgDutyCycle = new float[3]; // rgb

        const uint period = 2000; //period between pulses

        public LEDStripController(CTRE.HERO.Port3Definition port3)
        {
            uint duration = 0; //duration of pulse
            //Gadgeteer Drive Module
            //PIN   J2      isPWM
            //3     P1
            //4     P2       Y
            //5     P3
            //6     P4       Y      Red
            //7     P5       Y      Grn     
            //8     P6       Y      Blu
            //9     ---      Y

            _pwms[0] = new PWM(port3.PWM_Pin6, period, duration, PWM.ScaleFactor.Microseconds, false); // p4
            _pwms[1] = new PWM(port3.PWM_Pin7, period, duration, PWM.ScaleFactor.Microseconds, false); // p5
            _pwms[2] = new PWM(port3.PWM_Pin8, period, duration, PWM.ScaleFactor.Microseconds, false); // p6

            foreach (PWM pwm in _pwms)
                pwm.Start();
        }
        private void Update()
        {
            float kStepOn = 1f; //  fast on 
            float kStepOff = 0.30f; // slow off

            for (int i = 0; i < 3; ++i)
            {
                if (_trgDutyCycle[i] >= _dutyCycle[i])
                {
                    float chunk = _trgDutyCycle[i] - _dutyCycle[i];
                    if (chunk > kStepOn)
                        chunk = kStepOn;
                    _dutyCycle[i] += chunk;
                }
                else
                {
                    float chunk = _dutyCycle[i] - _trgDutyCycle[i];
                    if (chunk > kStepOff)
                        chunk = kStepOff;
                    _dutyCycle[i] -= chunk;
                }
            }
            /* update hardware */
            for (int i = 0; i < 3; ++i)
                _pwms[i].DutyCycle = _dutyCycle[i];
        }
        public void Process()
        {
            Update();
        }
        /* Provide a method to modify all three colors at the same time?? */
        public float Red
        {
            set
            {
                float v = value;
                if (v > +1) { v = +1; }
                if (v < 0) { v = 0; }
                _trgDutyCycle[0] = v;
            }
        }
        public float Grn
        {
            set
            {
                float v = value;
                if (v > +1) { v = +1; }
                if (v < 0) { v = 0; }
                _trgDutyCycle[1] = v;
            }
        }
        public float Blue
        {
            set
            {
                float v = value;
                if (v > +1) { v = +1; }
                if (v < 0) { v = 0; }
                _trgDutyCycle[2] = v;
            }
        }
    }
}