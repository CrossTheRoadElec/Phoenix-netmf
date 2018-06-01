using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.LowLevel
{
    class TimestampMsgMap
    {
    }

    class MsgEntry
    {
        ErrorCode _code;
        string _origin = "";
        string _stackTrace = "";
        int _hierarchy;
        long _timestamp = 0;
        string _shortMessage = "";
        string _longMessage = "";
        string _logMessage = ""; //!< intended to be logged


        internal MsgEntry(ErrorCode code, string origin,
		string stacktrace, int hierarchy) {

            _code = code;
	        _origin = origin;
	        if (stacktrace == null) {
                /* leave str empty */
                _stackTrace = "";
	        } else {
		        _stackTrace = stacktrace;
	        }
            _hierarchy = hierarchy;
	        _timestamp = GetTimeMs();

            Compile();
        }
        internal MsgEntry( MsgEntry rhs) {
	        /* simple copy of all members */
	        _code = rhs._code;
	        _origin = rhs._origin;
	        _stackTrace = rhs._stackTrace;
	        _hierarchy = rhs._hierarchy;
	        _timestamp = rhs._timestamp;
	        _shortMessage = rhs._shortMessage;
	        _longMessage = rhs._longMessage;
	        _logMessage = rhs._logMessage;
        }

        //public static bool operator < (MsgEntry toCompare) const {
	       // /* use origin string only */
	       // int compareRes = std::strcmp(_origin.c_str(), toCompare._origin.c_str());
	       // if (compareRes == 0) {
		      //  /* if device string is the same, diff the hierarchy */
		      //  compareRes = _hierarchy - toCompare._hierarchy;
	       // }
	       // return compareRes< 0;
        //}

        long GetTimeMs()
        {
            return DateTime.Now.Ticks;
        }

        internal bool NotWorthLogging()
        {
            //For now, check the short message instead of the log message
	        //if (_logMessage.Length == 0) {
            if(_shortMessage.Length == 0) {
		        /* Parse() determined this was NOT in the white list */
		        return true;
	        }
	        return false; /* this entry is worth loggign */
        }
        /** return the full message */
        void Compile()
        {
            switch (_code)
            {
                //White list error codes that do not throw errors to the DS
                case ErrorCode.OKAY:
                case ErrorCode.PulseWidthSensorNotPresent:
                    /* empty the output */
                    break;
                default:
                    CTRE.Phoenix.ErrorStrings.GetErrorDescription(_code, out _shortMessage, out _longMessage);

                    //// t = t - instance->startOfSeconds;

                    //string s = _stackTrace;
                    ////s.Replace(s.begin(), s.end(), '\n', ':'); //TODO: .netmf lacks a replace function, need to write custom

                    //std::stringstream ss;
                    //ss << _timestamp / 1000 << "." << _timestamp % 1000 << ": ThreadID: "
                    //        << std::this_thread::get_id() << ": Origin: " << _origin << ": "
                    //        << _longMessage.c_str() << ": Error Code: " << (int)(_code)
                    //        << ": Stack Trace: " << s << "\n";


                    //_logMessage = ss.str();
                    break;
            }
        }

        internal void LogToDs()
        {
            //For now, just debug print.  May pipe this elsewhere in the future.
            System.Text.StringBuilder _outmsg = new System.Text.StringBuilder();

            _outmsg.Append("Error ");
            _outmsg.Append(_code);
            _outmsg.Append(" HERO: ");
            _outmsg.Append(_shortMessage);
            _outmsg.Append(" ");
            _outmsg.Append(_origin);
            _outmsg.Append("\n");
            _outmsg.Append(_stackTrace);

            Debug.Print(_outmsg.ToString());
        }
    }
}
