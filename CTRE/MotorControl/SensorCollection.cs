using System;
using Microsoft.SPOT;
using CTRE.Phoenix.LowLevel;
using CTRE.Phoenix;

namespace CTRE.Phoenix.MotorControl
{
    /** Implements the sensorcollection class. */
    public class SensorCollection
    {
        /** Low level object. */
        MotController_LowLevel _ll;

        /**
         * Constructor.
         *
         * @param   ll  The ll.
         */

        internal SensorCollection(MotController_LowLevel ll)
        {
            _ll = ll;
        }

        /**
         * Get the position of whatever is in the analog pin of the Talon, regardless of
         *   whether it is actually being used for feedback.
         *
         * @param [out] param   The parameter to fill.
         *
         * @return  the 24bit analog value.  The bottom ten bits is the ADC (0 - 1023)
         *          on the analog pin of the Talon. The upper 14 bits tracks the overflows and underflows
         *          (continuous sensor).
         */

        public ErrorCode GetAnalogIn(out int param)
        {
            return _ll.GetAnalogInWithOv(out param);
        }

        /**
         * Sets analog position.
         *
         * @param   newPosition The new position.
         * @param   timeoutMs   (Optional) The timeout in milliseconds.
         *
         * @return  an ErrorCode.
         */

        public ErrorCode SetAnalogPosition(int newPosition, int timeoutMs = 0)
        {
            return _ll.ConfigSetParameter(ParamEnum.eAnalogPosition, newPosition, 0, 0, timeoutMs);
        }

        /**
         * Get the position of whatever is in the analog pin of the Talon, regardless of whether
         *   it is actually being used for feedback.
         *
         * @param [out] param   The parameter to fill.
         *
         * @return  the ADC (0 - 1023) on analog pin of the Talon.
         */

        public ErrorCode GetAnalogInRaw(out int param)
        {
            ErrorCode retval = _ll.GetAnalogInWithOv(out param);
            param &= 0x3FF;
            return retval;
        }

        /**
         * Get the position of whatever is in the analog pin of the Talon, regardless of
         *   whether it is actually being used for feedback.
         *
         * @param [out] param   The parameter to fill.
         *
         * @return  the value (0 - 1023) on the analog pin of the Talon.
         */

        public ErrorCode GetAnalogInVel(out int param)
        {
            return _ll.GetAnalogInVel(out param);
        }

        /**
         * Get the position of whatever is in the analog pin of the Talon, regardless of whether
         *   it is actually being used for feedback.
         *
         * @param [out] param   The value to fill with the Quad pos.
         *
         * @return  the Error code of the request.
         */

        public ErrorCode GetQuadraturePosition(out int param)
        {
            return _ll.GetQuadraturePosition(out param);
        }

        /**
         * Change the quadrature reported position.  Typically this is used to "zero" the
         *   sensor. This only works with Quadrature sensor.  To set the selected sensor position
         *   regardless of what type it is, see SetSelectedSensorPosition in the motor controller class.
         *
         * @param   newPosition The position value to apply to the sensor.
         * @param   timeoutMs   (Optional) How long to wait for confirmation.  Pass zero so that call
         *                      does not block.
         *
         * @return  error code.
         */

        public ErrorCode SetQuadraturePosition(int newPosition, int timeoutMs = 0)
        {
            return _ll.ConfigSetParameter(ParamEnum.eQuadraturePosition, newPosition, 0, 0, timeoutMs);
        }

        /**
         * Change the quadrature reported position based on pulse width. This can be used to 
         * effectively make quadrature absolute. For rotary mechanisms with >360 movement (such
         * as typical swerve modules) bookend0 and bookend1 can be both set to 0 and 
         * bCrossZeroOnInterval can be set to true. For mechanisms with less than 360 travel (such
         * as arms), bookend0 and bookend1 should be set to the pulse width values at the two 
         * extremes. If the interval crosses over the pulse width value of 0 (or any multiple of
         * 4096), bCrossZeroOnInterval should be true and otherwise should be false. An offset can
         * also be set.
         *
         * @param   bookend0    value at extreme 0
         * @param   bookend1    value at extreme 1
         * @param   bCrossZeroOnInterval    value at extreme 1
         * @param   offset      (Optional) Value to add to pulse width 
         * @param   timeoutMs   (Optional) How long to wait for confirmation.  Pass zero so that call
         *                      does not block.
         *
         * @return  error code.
         */

        public ErrorCode SyncQuadratureWithPulseWidth(int bookend0, int bookend1, bool bCrossZeroOnInterval, int offset = 0, int timeoutMs = 0)
        {   
            int ticksPerRevolution = 4096;
            /* Normalize bookends (should be 0 - ticksPerRevolution) */
            bookend0 &= (ticksPerRevolution - 1);
            bookend1 &= (ticksPerRevolution - 1);
          
            /* Assign greater and lesser bookend */
            int greaterBookend;
            int lesserBookend;
            
            if(bookend0 > bookend1)
            {
                greaterBookend = bookend0;
                lesserBookend = bookend1;
            }
            else
            {
                greaterBookend = bookend1;
                lesserBookend = bookend0;
            }

            int average = (greaterBookend + lesserBookend) / 2;
 
            ErrorCollection errorCollection  = new ErrorCollection();
            
            /* Get Fractional Part of Pulse Width Position (0 - ticksPerRevolution) */
            int pulseWidth;
            errorCollection.NewError(GetPulseWidthPosition(out pulseWidth));
            pulseWidth &= (ticksPerRevolution - 1);
            
            if(bCrossZeroOnInterval) 
            {
                /*
                 * If the desire is to have the *** part be the interval 
                 * (2048 - 3277 and crosses over 0): 
                 *
                 *                            
                 *                        1024
                 *                     *********    
                 *                    ***********   
                 *                   *************  
                 *                  *************** 
                 *                 *****************
                 *                 *****************
                 *                 *****************
                 *            2048 ***************** 0
                 *                         *********
                 *                         *********
                 *                         *********
                 *                         *********
                 *                        **********
                 *                        ********* 
                 *                        ********  
                 *                       ********   
                 *                       *******
                 *                     3277   
                 *
                 * The goal is to center the discontinuoity between 2048 and 3277 in the blank.
                 * So all pulse width values greater than the avg of the two bookends should be 
                 * reduced by ticksPerRevolution.
                 */
                if(pulseWidth > average)
                {            
                    pulseWidth -= ticksPerRevolution;
                }
            }
            else
            {
                /*
                 * If the desire is to have the blank part be the interval 
                 * (2048 - 3277 and crosses over 0): 
                 *
                 *                            
                 *                        1024
                 *                     *********    
                 *                    ***********   
                 *                   *************  
                 *                  *************** 
                 *                 *****************
                 *                 *****************
                 *                 *****************
                 *            2048 ***************** 0
                 *                         *********
                 *                         *********
                 *                         *********
                 *                         *********
                 *                        **********
                 *                        ********* 
                 *                        ********  
                 *                       ********   
                 *                       *******
                 *                     3277   
                 *
                 * The goal is to center the discontinuoity between 2048 and 3277 in the ***.
                 * So all pulse width values less than the (ticksPerRevolution / 2 - avg of 
                 * the two bookends) & ticksPerRevolution should be increased by 
                 * ticksPerRevolution.
                 */
                if(pulseWidth < ((ticksPerRevolution / 2 - average) & 0x0FFF))
                {            
                    pulseWidth += ticksPerRevolution;
                }
            }
           
            pulseWidth += offset;
 
            errorCollection.NewError(SetQuadraturePosition(pulseWidth, timeoutMs));

            return errorCollection._worstError;
        }

        /**
         * Get the position of whatever is in the analog pin of the Talon, regardless of whether
         *   it is actually being used for feedback.
         *
         * @param [out] param   The parameter to fill.
         *
         * @return  the value (0 - 1023) on the analog pin of the Talon.
         */

        public ErrorCode GetQuadratureVelocity(out int param)
        {
            return _ll.GetQuadratureVelocity(out param);
        }

        /**
         * Gets pulse width position.
         *
         * @param [out] param   The parameter to fill.
         *
         * @return  the pulse width position.
         */

        public ErrorCode GetPulseWidthPosition(out int param)
        {
            return _ll.GetPulseWidthPosition(out param);
        }

        /**
         * Sets pulse width position.
         *
         * @param   newPosition The position value to apply to the sensor.
         * @param   timeoutMs   (Optional) How long to wait for confirmation.  Pass zero so that call
         *                      does not block.
         *
         * @return  an ErrorCode.
         */

        public ErrorCode SetPulseWidthPosition(int newPosition, int timeoutMs = 0)
        {
            return _ll.ConfigSetParameter(ParamEnum.ePulseWidthPosition, newPosition, 0, 0, timeoutMs);
        }

        /**
         * Gets pulse width velocity.
         *
         * @param [out] param   The parameter to fill.
         *
         * @return  the pulse width velocity.
         */

        public ErrorCode GetPulseWidthVelocity(out int param)
        {
            return _ll.GetPulseWidthVelocity(out param);
        }

        /**
         * Gets pulse width rise to fall us.
         *
         * @param [out] param   The parameter to fill.
         *
         * @return  the pulse width rise to fall us.
         */

        public ErrorCode GetPulseWidthRiseToFallUs(out int param)
        {
            return _ll.GetPulseWidthRiseToFallUs(out param);
        }

        /**
         * Gets pulse width rise to rise us.
         *
         * @param [out] param   The parameter to fill.
         *
         * @return  the pulse width rise to rise us.
         */

        public ErrorCode GetPulseWidthRiseToRiseUs(out int param)
        {
            return _ll.GetPulseWidthRiseToRiseUs(out param);
        }

        /**
         * Gets pin state quad a.
         *
         * @param [out] param   The parameter to fill.
         *
         * @return  the pin state quad a.
         */

        public ErrorCode GetPinStateQuadA(out int param)
        {
            return _ll.GetPinStateQuadA(out param);
        }

        /**
         * Gets pin state quad b.
         *
         * @param [out] param   The parameter to fill.
         *
         * @return  Digital level of QUADB pin.
         */

        public ErrorCode GetPinStateQuadB(out int param)
        {
            return _ll.GetPinStateQuadB(out param);
        }

        /**
         * Gets pin state quad index.
         *
         * @param [out] param   The parameter to fill.
         *
         * @return  Digital level of QUAD Index pin.
         */

        public ErrorCode GetPinStateQuadIdx(out int param)
        {
            return _ll.GetPinStateQuadIdx(out param);
        }

        /**
         * Is forward limit switch closed.
         *
         * @param [out] param   The parameter to fill.
         *
         * @return  '1' iff forward limit switch is closed, 0 iff switch is open. This function works
         *          regardless if limit switch feature is enabled.
         */

        public ErrorCode IsFwdLimitSwitchClosed(out int param)
        {
            return _ll.IsFwdLimitSwitchClosed(out param);
        }

        /**
         * Is reverse limit switch closed.
         *
         * @param [out] param   The parameter to fill.
         *
         * @return  '1' iff reverse limit switch is closed, 0 iff switch is open. This function works
         *          regardless if limit switch feature is enabled.
         */

        public ErrorCode IsRevLimitSwitchClosed(out int param)
        {
            return _ll.IsRevLimitSwitchClosed(out param);
        }
    }
}
