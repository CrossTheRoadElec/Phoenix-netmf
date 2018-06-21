using System;
using CTRE.Phoenix.LowLevel;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControl.CAN
{
    public class VictorSPXPIDSetConfiguration : BasePIDSetConfiguration {
        public RemoteFeedbackDevice selectedFeedbackSensor;
    
        public VictorSPXPIDSetConfiguration() {
            selectedFeedbackSensor = RemoteFeedbackDevice.RemoteFeedbackDevice_RemoteSensor0;
            //NOTE: while the factory default value is 0, this value can't 
            //be set by the API. Thus, RemoteSensor0 is the default

        }
        public string ToString(string prependString) {
    
            string retstr = prependString + ".selectedFeedbackSensor = " + FeedbackDeviceRoutines.ToString(selectedFeedbackSensor) + ";\n";
            retstr += base.ToString(ref prependString);
            return retstr;
        }
    
    };
    
    public class VictorSPXConfiguration : BaseMotorControllerConfiguration {
        public VictorSPXPIDSetConfiguration primaryPID;
        public VictorSPXPIDSetConfiguration auxilaryPID;
        public RemoteLimitSwitchSource forwardLimitSwitchSource;
        public RemoteLimitSwitchSource reverseLimitSwitchSource;
        public RemoteFeedbackDevice sum_0;
        public RemoteFeedbackDevice sum_1;
        public RemoteFeedbackDevice diff_0;
        public RemoteFeedbackDevice diff_1;
    
        public VictorSPXConfiguration() {
            primaryPID = new  VictorSPXPIDSetConfiguration();
            auxilaryPID = new VictorSPXPIDSetConfiguration();
            
            forwardLimitSwitchSource = RemoteLimitSwitchSource.Deactivated;
            reverseLimitSwitchSource = RemoteLimitSwitchSource.Deactivated;
            sum_0  = RemoteFeedbackDevice.RemoteFeedbackDevice_RemoteSensor0; 
            sum_1  = RemoteFeedbackDevice.RemoteFeedbackDevice_RemoteSensor0; 
            diff_0 = RemoteFeedbackDevice.RemoteFeedbackDevice_RemoteSensor0;
            diff_1 = RemoteFeedbackDevice.RemoteFeedbackDevice_RemoteSensor0;
            //NOTE: while the factory default value is 0, this value can't 
            //be set by the API. Thus, RemoteSensor0 is the default
        }
        public new string ToString(string prependString) {
            string retstr = primaryPID.ToString(prependString + ".primaryPID");
            retstr += auxilaryPID.ToString(prependString + ".auxilaryPID");
            retstr += prependString + ".forwardLimitSwitchSource = " + LimitSwitchRoutines.ToString(forwardLimitSwitchSource) + ";\n";
            retstr += prependString + ".reverseLimitSwitchSource = " + LimitSwitchRoutines.ToString(reverseLimitSwitchSource) + ";\n";
            retstr += prependString + ".sum_0 = " + FeedbackDeviceRoutines.ToString(sum_0) + ";\n";
            retstr += prependString + ".sum_1 = " + FeedbackDeviceRoutines.ToString(sum_1) + ";\n";
            retstr += prependString + ".diff_0 = " + FeedbackDeviceRoutines.ToString(diff_0) + ";\n";
            retstr += prependString + ".diff_1 = " + FeedbackDeviceRoutines.ToString(diff_1) + ";\n";
            retstr += base.ToString(prependString);
    
            return retstr;
        }
    };
    
    public class VictorSPX : BaseMotorController, IMotorController
    {
        public VictorSPX(int deviceNumber, bool externalEnable = false) : base(deviceNumber | 0x01040000, externalEnable)
        {
            
        }
        /**
         * Gets all PID set persistant settings.
         *
         * @param pid               Object with all of the PID set persistant settings
         * @param pidIdx            0 for Primary closed-loop. 1 for auxiliary closed-loop.
         * @param timeoutMs
         *              Timeout value in ms. If nonzero, function will wait for
         *              config success and report an error if it times out.
         *              If zero, no blocking or checking is performed.
         */
        public ErrorCode ConfigurePID(VictorSPXPIDSetConfiguration pid, int pidIdx = 0, int timeoutMs = 50) {
            ErrorCollection errorCollection = new ErrorCollection();
        
            //------ sensor selection ----------//      
        
            errorCollection.NewError(BaseConfigurePID(pid, pidIdx, timeoutMs));
            errorCollection.NewError(ConfigSelectedFeedbackSensor(pid.selectedFeedbackSensor, pidIdx, timeoutMs));
        
        
            return errorCollection._worstError;
        }
        /**
         * Gets all PID set persistant settings (overloaded so timeoutMs is 50 ms
         * and pidIdx is 0).
         *
         * @param pid               Object with all of the PID set persistant settings
         */
        public void GetPIDConfigs(out VictorSPXPIDSetConfiguration pid, int pidIdx = 0, int timeoutMs = 50)
        {
            pid = new VictorSPXPIDSetConfiguration();
            BaseGetPIDConfigs(pid, pidIdx, timeoutMs);
            pid.selectedFeedbackSensor = (RemoteFeedbackDevice) ConfigGetParameter(ParamEnum.eFeedbackSensorType, pidIdx, timeoutMs);
        
        }
        
        /**
         * Configures all peristant settings.
         *
         * @param allConfigs        Object with all of the persistant settings
         * @param timeoutMs
         *              Timeout value in ms. If nonzero, function will wait for
         *              config success and report an error if it times out.
         *              If zero, no blocking or checking is performed.
         *
         * @return Error Code generated by function. 0 indicates no error. 
         */
        public ErrorCode ConfigAllSettings(VictorSPXConfiguration allConfigs, int timeoutMs = 50) {
            ErrorCollection errorCollection = new ErrorCollection();
        
            errorCollection.NewError(BaseConfigAllSettings(allConfigs, timeoutMs));
        
            //------ remote limit switch ----------//   
            errorCollection.NewError(ConfigSetParameter(ParamEnum.eLimitSwitchSource, (float)allConfigs.forwardLimitSwitchSource, 0, 0, timeoutMs));
            
            errorCollection.NewError(ConfigSetParameter(ParamEnum.eLimitSwitchSource, (float)allConfigs.reverseLimitSwitchSource, 0, 1, timeoutMs));

        
        
            //--------PIDs---------------//
        
            errorCollection.NewError(ConfigurePID(allConfigs.primaryPID, 0, timeoutMs));
        
            errorCollection.NewError(ConfigurePID(allConfigs.auxilaryPID, 1, timeoutMs));
        
            errorCollection.NewError(ConfigSensorTerm(SensorTerm.SensorTerm_Sum0, allConfigs.sum_0, timeoutMs));
        
            errorCollection.NewError(ConfigSensorTerm(SensorTerm.SensorTerm_Sum1, allConfigs.sum_1, timeoutMs));
        
            errorCollection.NewError(ConfigSensorTerm(SensorTerm.SensorTerm_Diff0, allConfigs.diff_0, timeoutMs));
        
            errorCollection.NewError(ConfigSensorTerm(SensorTerm.SensorTerm_Diff1, allConfigs.diff_1, timeoutMs));
        
        
            return errorCollection._worstError;
        }
        /**
         * Gets all persistant settings.
         *
         * @param allConfigs        Object with all of the persistant settings
         * @param timeoutMs
         *              Timeout value in ms. If nonzero, function will wait for
         *              config success and report an error if it times out.
         *              If zero, no blocking or checking is performed.
         */
        public void GetAllConfigs(out VictorSPXConfiguration allConfigs, int timeoutMs = 50) {

            allConfigs = new VictorSPXConfiguration();

            BaseGetAllConfigs(allConfigs, timeoutMs);
        
            GetPIDConfigs(out allConfigs.primaryPID, 0, timeoutMs);
            GetPIDConfigs(out allConfigs.auxilaryPID, 1, timeoutMs);
            allConfigs.sum_0 = (RemoteFeedbackDevice) ConfigGetParameter(ParamEnum.eSensorTerm, 0, timeoutMs);
            allConfigs.sum_1 = (RemoteFeedbackDevice) ConfigGetParameter(ParamEnum.eSensorTerm, 1, timeoutMs);
            allConfigs.diff_0 = (RemoteFeedbackDevice) ConfigGetParameter(ParamEnum.eSensorTerm, 2, timeoutMs);
            allConfigs.diff_1 = (RemoteFeedbackDevice) ConfigGetParameter(ParamEnum.eSensorTerm, 3, timeoutMs);
        
            allConfigs.forwardLimitSwitchSource = (RemoteLimitSwitchSource) ConfigGetParameter(ParamEnum.eLimitSwitchSource, 0, timeoutMs);
            allConfigs.reverseLimitSwitchSource = (RemoteLimitSwitchSource) ConfigGetParameter(ParamEnum.eLimitSwitchSource, 1, timeoutMs);
        
        }
    }
}

