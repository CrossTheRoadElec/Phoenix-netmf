using Microsoft.SPOT.Hardware;

namespace CTRE
{
    namespace Gadgeteer
    {
        namespace Module
        {
            public class DriverModule : ModuleBase
            {
                public class OutputState
                {
                    public const bool driveLow = true;
                    public const bool pullUp = false;
                }

                public new readonly char kModulePortType = 'Y';

                private PortDefinition port;
                private OutputPort[] output = new OutputPort[7];
                private bool[] outputStates = new bool[7];

                ErrorCodeVar _lastError = new ErrorCodeVar();

                public DriverModule(PortDefinition port)
                {
                    if (CTRE.Phoenix.Util.Contains(port.types, kModulePortType))
                    {
                        _lastError.SetLastError(ErrorCode.OK);
                        this.port = port;
                        InitializePort((IPortGpio7)this.port);
                    }
                    else
                    {
                        _lastError.SetLastError(ErrorCode.PORT_MODULE_TYPE_MISMATCH);
                        CTRE.Phoenix.Reporting.SetError(_lastError.GetLastError());
                    }
                }

                public void Set(int outputNum, bool outputState)
                {
                    if (_lastError == ErrorCode.OK)
                    {
                        output[outputNum - 1].Write(outputState);
                        outputStates[outputNum - 1] = outputState;
                    }
                    else if (_lastError == ErrorCode.PORT_MODULE_TYPE_MISMATCH)
                    {
                        CTRE.Phoenix.Reporting.SetError(ErrorCode.MODULE_NOT_INIT_SET_ERROR);
                    }
                }

                public bool Get(int outputNum)
                {
                    if (_lastError == ErrorCode.OK)
                    {
                        return outputStates[outputNum - 1];
                    }
                    else if (_lastError == ErrorCode.PORT_MODULE_TYPE_MISMATCH)
                    {
                        CTRE.Phoenix.Reporting.SetError(ErrorCode.MODULE_NOT_INIT_GET_ERROR);
                    }
                    
                    return false;
                }

                private void InitializePort(IPortGpio7 port)
                {
                    output[0] = new OutputPort(port.Pin3, OutputState.pullUp);
                    output[1] = new OutputPort(port.Pin4, OutputState.pullUp);
                    output[2] = new OutputPort(port.Pin5, OutputState.pullUp);
                    output[3] = new OutputPort(port.Pin6, OutputState.pullUp);
                    output[4] = new OutputPort(port.Pin7, OutputState.pullUp);
                    output[5] = new OutputPort(port.Pin8, OutputState.pullUp);

                    for (int i = 0; i < 6; i++)
                        outputStates[i] = false;

                }
            }
        }
    }
}
