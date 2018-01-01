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
namespace CTRE.Phoenix.Signals
{
    public class MovingAverage
    {
        private int _in;
        private int _ou;
        private int _cnt;
        private int _cap;

        private float _sum;
        private float _min;

        private float[] _d;

        public MovingAverage(int capacity)
        {
            _cap = capacity;
            _d = new float[_cap];
            Clear();
        }
        public float Process(float input)
        {
            Push(input);
            return _sum / (float)_cnt;
        }
        public void Clear()
        {
            _in = 0;
            _ou = 0;
            _cnt = 0;

            _sum = 0;
        }
        public void Push(float d)
        {
            /* process it */
            _sum += d;

            /* if full, pop one */
            if (_cnt >= _cap)
                Pop();

            /* push new one */
            _d[_in] = d;
            if (++_in >= _cap)
                _in = 0;
            ++_cnt;

            /* calc new min - slow */
            CalcMin();
        }
        public void Pop()
        {
            /* get the oldest */
            float d = _d[_ou];

            /* process it */
            _sum -= d;

            /* pop it */
            if (++_ou >= _cap)
                _ou = 0;
            --_cnt;
        }
        private void CalcMin()
        {
            _min = float.MaxValue;

            int ou = _ou;
            int cnt = _cnt;
            while (cnt > 0)
            {
                float d = _d[ou];

                /* process sample */
                if (_min > d)
                    _min = d;

                /* iterate */
                if (++ou >= _cnt)
                    ou = 0;
                --cnt;

            }
        }
        //-------------- Properties --------------//
        public float Sum
        {
            get
            {
                return _sum;
            }
        }
        public int Count
        {
            get
            {
                return _cnt;
            }
        }
        public float Minimum
        {
            get
            {
                return _min;
            }
        }
    }

}
