using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControl.CAN
{
    public class VictorSPXPIDSetConfiguration : BasePIDSetConfiguration {
        public RemoteFeedbackDevice selectedFeedbackSensor;
    
        public VictorSPXPIDSetConfiguration() {
            selectedFeedbackSensor = RemoteFeedbackDevice.None;
        }
        public string ToString(string prependString) {
    
            string retstr = prependString + ".selectedFeedbackSensor = " + FeedbackDeviceRoutines.ToString(selectedFeedbackSensor) + ";\n";
            retstr += base.ToString(prependString);
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
            forwardLimitSwitchSource = RemoteLimitSwitchSource.Deactivated;
            reverseLimitSwitchSource = RemoteLimitSwitchSource.Deactivated;
            sum_0  = RemoteFeedbackDevice.None;
            sum_1  = RemoteFeedbackDevice.None;
            diff_0 = RemoteFeedbackDevice.None;
            diff_1 = RemoteFeedbackDevice.None;
    
        {
        }
        public string ToString(string prependString) {
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
    public ErrorCode ConfigurePID(ref VictorSPXPIDSetConfiguration pid, int pidIdx = 0, int timeoutMs = 50) {
        ErrorCollection errorCollection;
    
        //------ sensor selection ----------//      
    
        errorCollection.NewError(BaseConfigurePID(pid, pidIdx, timeoutMs));
        errorCollection.NewError(ConfigSelectedFeedbackSensor(pid.selectedFeedbackSensor, pidIdx, timeoutMs));
    
    
        return errorCollection._worstError;
    }
    public void GetPIDConfigs(out VictorSPXPIDSetConfiguration pid, int pidIdx = 0, int timeoutMs = 50)
    {
        BaseGetPIDConfigs(pid, pidIdx, timeoutMs);
        pid.selectedFeedbackSensor = (RemoteFeedbackDevice) ConfigGetParameter(ParamEnum.eFeedbackSensorType, pidIdx, timeoutMs);
    
    }
    
    public ErrorCode ConfigAllSettings(ref VictorSPXConfiguration allConfigs, int timeoutMs = 50) {
        ErrorCollection errorCollection;
    
        errorCollection.NewError(BaseConfigAllSettings(allConfigs, timeoutMs));
    
        //------ remote limit switch ----------//   
        errorCollection.NewError(ConfigForwardLimitSwitchSource(allConfigs.forwardLimitSwitchSource, allConfigs.forwardLimitSwitchNormal, allConfigs.forwardLimitSwitchDeviceID, timeoutMs));
    
        errorCollection.NewError(ConfigReverseLimitSwitchSource(allConfigs.reverseLimitSwitchSource, allConfigs.reverseLimitSwitchNormal, allConfigs.reverseLimitSwitchDeviceID, timeoutMs));
    
    
        //--------PIDs---------------//
    
        errorCollection.NewError(ConfigurePID(allConfigs.primaryPID, 0, timeoutMs));
    
        errorCollection.NewError(ConfigurePID(allConfigs.auxilaryPID, 1, timeoutMs));
    
        errorCollection.NewError(ConfigSensorTerm(SensorTerm_Sum0, allConfigs.sum_0, timeoutMs));
    
        errorCollection.NewError(ConfigSensorTerm(SensorTerm_Sum1, allConfigs.sum_1, timeoutMs));
    
        errorCollection.NewError(ConfigSensorTerm(SensorTerm_Diff0, allConfigs.diff_0, timeoutMs));
    
        errorCollection.NewError(ConfigSensorTerm(SensorTerm_Diff1, allConfigs.diff_1, timeoutMs));
    
    
        return errorCollection._worstError;
    }
    public void GetAllConfigs(out VictorSPXConfiguration allConfigs, int timeoutMs = 50) {
    
        BaseGetAllConfigs(allConfigs, timeoutMs);
    
        GetPIDConfigs(allConfigs.primaryPID, 0, timeoutMs);
        GetPIDConfigs(allConfigs.auxilaryPID, 1, timeoutMs);
        allConfigs.sum_0 = (RemoteFeedbackDevice) ConfigGetParameter(ParamEnum.eSensorTerm, 0, timeoutMs);
        allConfigs.sum_1 = (RemoteFeedbackDevice) ConfigGetParameter(ParamEnum.eSensorTerm, 1, timeoutMs);
        allConfigs.diff_0 = (RemoteFeedbackDevice) ConfigGetParameter(ParamEnum.eSensorTerm, 2, timeoutMs);
        allConfigs.diff_1 = (RemoteFeedbackDevice) ConfigGetParameter(ParamEnum.eSensorTerm, 3, timeoutMs);
    
        allConfigs.forwardLimitSwitchSource = (RemoteLimitSwitchSource) ConfigGetParameter(ParamEnum.eLimitSwitchSource, 0, timeoutMs);
        allConfigs.reverseLimitSwitchSource = (RemoteLimitSwitchSource) ConfigGetParameter(ParamEnum.eLimitSwitchSource, 1, timeoutMs);
    
    }
    
    public ErrorCode ConfigFactoryDefault(int timeoutMs = 50) {
        VictorSPXConfiguration defaults;
        return ConfigAllSettings(defaults, timeoutMs);
    }





    }
}

