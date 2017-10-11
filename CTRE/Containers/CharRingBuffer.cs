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

namespace CTRE.Phoenix.Containers
{
    /// <summary>
    /// General ring buffer for bytes.
    /// </summary>
    public class CharRingBuffer
    {
        private uint _in;
        private uint _ou;
        private uint _cnt;
        private uint _cap;

        private char[] _d;

        public CharRingBuffer(uint cap)
        {
            _cap = cap;
            _d = new char[_cap];
            Clear();
        }
        public void Clear()
        {
            _in = 0;
            _ou = 0;
            _cnt = 0;
        }
        public void Push(byte d)
        {
            Push((char)d);
        }
        public void Push(char d)
        {
            /* push new one */
            _d[_in] = d;
            if (++_in >= _cap)
                _in = 0;
            ++_cnt;
        }
        public void Push(char[] d, int numBytes)
        {
            for (int i = 0; i < numBytes; ++i)
                Push(d[i]);
        }
        public void Push(char[] d)
        {
            Push(d, d.Length);
        }
        public char Pop()
        {
            char ret = _d[_ou];
            if (Empty)
                return (char)0;
            /* pop it */
            if (++_ou >= _cap)
                _ou = 0;
            --_cnt;
            return ret;
        }
        public char[] Pop(uint numBytes)
        {
            if (numBytes > Count)
                numBytes = Count;
            char[] ret = new char[numBytes];
            for (int i = 0; i < numBytes; i++)
            {
                if (_cnt <= 0) break;
                ret[i] = _d[_ou];
                _ou++;
                if (_ou >= _cap)
                    _ou = 0;
                _cnt--;
            }
            return ret;
        }
        //-------------- Properties --------------//
        public bool Empty
        {
            get { return _cnt < 1; }
        }
        public bool Full
        {
            get { return _cnt >= _cap; }
        }
        public char Front
        {
            get
            {
                if (Empty)
                    return '\0';
                return _d[_ou];
            }
        }
        public uint Count
        {
            get
            {
                return _cnt;
            }
        }
        public uint Capacity
        {
            get
            {
                return _cap;
            }
        }
        public uint RemainingCapacity
        {
            get
            {
                return _cap - _cnt;
            }
        }
    }
}