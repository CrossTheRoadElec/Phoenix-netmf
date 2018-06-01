using System;
using System.Text;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace CTRE
{
    namespace Gadgeteer
    {
        namespace Module
        {
            public class WiFiESP12F : ModuleBase
            {
                public new readonly char kModulePortType = 'U';

                PortDefinition port;
                System.IO.Ports.SerialPort uart;
                ErrorCodeVar status;

                int baud = 115200;
                String SSID;

                const int kDefaultTimeoutMs = 100;
                const int kSendTimeoutMs = 1000;

                private CTRE.Phoenix.PeriodicTimeout _timeSched = new CTRE.Phoenix.PeriodicTimeout(100);
                private System.Collections.ArrayList lines = new System.Collections.ArrayList();
                private StringBuilder temp = new StringBuilder();
                private bool isDone = false;

                public enum SecurityType { OPEN, WPA_PSK = 2, WPA2_PSK, WPA_WPA2_PSK }
                public enum wifiMode { STATION = 1, SOFTAP, SOFTAP_STATION }


                //UART Return Value Processor
                const int READY = 0;
                const int HEADER = 10;
                const int LINKID = 20;
                const int SIZE = 30;
                const int DATA = 40;

                const int STAGE1 = 1;
                const int STAGE2 = 2;
                const int STAGE3 = 3;
                const int STAGE4 = 4;

                int processState;
                int headerState;
                int linkIDState;

                int processedLinkID;
                int processedSize;

                int dataCount;
                byte[] dataCache = new byte[0];
                byte[] _rx = new byte[1024];

                //  WiFiSerialLexer lex;

                InputPort resetPin;
                InputPort GPIO;
                bool modulePresent = true;

                public WiFiESP12F(PortDefinition port)
                {
                    if (Contains(port.types, kModulePortType))
                    {
                        status = ErrorCode.OK;
                        this.port = port;

                        processState = READY;
                        headerState = STAGE1;
                        linkIDState = STAGE1;
                        processedSize = 0;
                        dataCount = 0;
                        InitUart((IPortUart)(this.port));
                        InitPresenceCheck((IPortUart)this.port);


                        // lex = new WiFiSerialLexer(uart);
                    }
                    else
                    {
                        status = ErrorCode.PORT_MODULE_TYPE_MISMATCH;
                        //Reporting.SetError(status);
                    }
                }

                private void InitUart(IPortUart port)
                {
                    uart = new System.IO.Ports.SerialPort(port.UART, baud, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                    uart.Open();
                }

                public void InitPresenceCheck(IPortUart port)
                {
                    resetPin = new InputPort(port.Pin6, false, Port.ResistorMode.PullDown);
                    GPIO = new InputPort(port.Pin3, false, Port.ResistorMode.PullDown);
                    modulePresent = resetPin.Read();
                }

                private int transaction(byte[] toSend, int timeoutMs, String toSearch = null)
                {
                    return transaction(toSend, 0, toSend.Length, timeoutMs, toSearch);
                }
                private int transaction(byte[] toSend, int offset, int byteLen, int timeoutMs, String toSearch = null)
                {
                    bool suc = false;

                    clear();
                    uart.Write(toSend, offset, byteLen);

                    if (toSearch != null)
                        suc = waitForResponse(toSearch, timeoutMs);
                    else
                        suc = waitForDone(timeoutMs);

                    return suc ? 0 : -1;
                }


                public int test(int timeoutMs = 100)
                {
                    byte[] toSend = MakeByteArrayFromString("AT" + "\r\n");

                    int retval = transaction(toSend, timeoutMs, "OK");

                    return retval;
                }

                public string getVersion(int timeoutMs = 100)
                {
                    byte[] toSend = MakeByteArrayFromString("AT+GMR\r\n");

                    int err = transaction(toSend, timeoutMs);

                    string temp = "";

                    if (err == 0)
                    {
                        foreach (String x in lines)
                        {
                            if (x.Length >= 11 && x.Substring(0, 11) == "SDK version")
                            {
                                temp = x.ToString();
                            }
                        }
                    }

                    return temp;
                }

                /**
                 * Get the Station IP Address
                 *
                 * @param timeoutMs
                 *            Timeout value in ms. If nonzero, function will wait for
                 *            config success and report an error if it times out.
                 *            If zero, no blocking or checking is performed.
                 * @return String of the IP Address
                 *            If in Station Mode, we check the first 12 characters to match 
                 *            "+CIFSR:STAIP". 
                 */
                public String getStationIP(int timeoutMs = 100)
                {
                    byte[] toSend = MakeByteArrayFromString("AT+CIFSR" + "\r\n");

                    int err = transaction(toSend, timeoutMs);

                    string temp = "";   /* Return empty string if there is no response */
                    if (err == 0)
                    {
                        foreach (String x in lines)
                        {
                            if (x.Length >= 12 && x.Substring(0, 12) == "+CIFSR:STAIP")
                            {
                                /* the line holding IP has been found, copy it out */
                                temp = x.ToString();
                                /* crop out everything between quotes */
                                int start = temp.IndexOf('\"');
                                int end = temp.LastIndexOf('\"');
                                int len = end - start - 1; /* chars between quotes not including quotes themselves */
                                /* validate input, do not parse if garbage */
                                if (start <= -1 || end <= -1 || len <= -1)
                                {
                                    /* Leave temp as empty if start, end, or length are invalid */
                                }
                                else
                                {
                                    /* start with char after first quote */
                                    temp = temp.Substring(start + 1, len);
                                    /* leave for loop immedietely since IP has been found */
                                    return temp;
                                }
                            }
                        }
                    }
                    return temp;
                }

                /* Get the Access Point IP Address
                 *
                 * @param timeoutMs
                 * Timeout value in ms.If nonzero, function will wait for
                 *            config success and report an error if it times out.
                 *            If zero, no blocking or checking is performed.
                 * @return String of the IP Address
                 *            If in softAP Mode, we check the first 11 characters to match
                 *            "+CIFSR:APIP"
                 */
                public String getAccessPointIP(int timeoutMs = 100)
                {
                    byte[] toSend = MakeByteArrayFromString("AT+CIFSR" + "\r\n");

                    int err = transaction(toSend, timeoutMs);

                    string temp = "";   /* Return empty string if there is no response */
                    if (err == 0)
                    {
                        foreach (String x in lines)
                        {
                            if (x.Length >= 11 && x.Substring(0, 11) == "+CIFSR:APIP")
                            {
                                /* the line holding IP has been found, copy it out */
                                temp = x.ToString();
                                /* crop out everything between quotes */
                                int start = temp.IndexOf('\"');
                                int end = temp.LastIndexOf('\"');
                                int len = end - start - 1; /* chars between quotes not including quotes themselves */
                                /* validate input, do not parse if garbage */
                                if (start <= -1 || end <= -1 || len <= -1)
                                {
                                    /* Leave temp as empty if start, end, or length are invalid */
                                }
                                else
                                {
                                    /* start with char after first quote */
                                    temp = temp.Substring(start + 1, len);
                                    /* leave for loop immedietely since IP has been found */
                                    return temp;
                                }
                            }
                        }
                    }
                    return temp;
                }

                public int setWifiMode(wifiMode mode, int timeoutMs = 50)
                {
                    byte[] toSend = MakeByteArrayFromString("AT+CWMODE_CUR=" + mode + "\r\n");

                    return transaction(toSend, timeoutMs, "OK");
                }

                public int setAP(String _SSID, String password, int channel, SecurityType encryption)
                {
                    SSID = _SSID;
                    byte[] toSend = MakeByteArrayFromString("AT+CWSAP_CUR=\"" + _SSID + "\",\"" + password + "\"," + channel + "," + encryption + "\r\n");
                    return transaction(toSend, kDefaultTimeoutMs, "OK");
                }

                public int sendUDP(int id, byte[] data)
                {
                    return sendUDP(id, data, data.Length, 0);
                }
                public int sendUDP(int id, byte[] data, int byteLen, int offset)
                {
                    int retval = 0;
                    byte[] cipsend = MakeByteArrayFromString("AT+CIPSEND=" + id + "," + byteLen + "\r\n");

                    retval |= transaction(cipsend, 10, "OK");
                    retval |= transaction(data, offset, byteLen, kSendTimeoutMs, "SEND OK");
                    return retval;
                }

                public void startUDP(int id, string remoteIP, int remotePort, int localPort)
                {
                    byte[] cipmux = MakeByteArrayFromString("AT+CIPMUX=1" + "\r\n");
                    transaction(cipmux, kDefaultTimeoutMs, "OK");


                    byte[] cipstart = MakeByteArrayFromString("AT+CIPSTART=" + id + ",\"UDP\",\"" + remoteIP + "\"," + remotePort + "," + localPort + ",0" + "\r\n");
                    transaction(cipstart, kDefaultTimeoutMs, "OK");
                }

                public int stopUDP(int id)
                {
                    byte[] cipclose = MakeByteArrayFromString("AT+CIPCLOSE=" + id + "\r\n");
                    return transaction(cipclose, kDefaultTimeoutMs, "OK");
                }

                //Used to connect to a TCP server.
                public void startTCP(int id, string remoteIP, int remotePort)
                {
                    byte[] cipmux = MakeByteArrayFromString("AT+CIPMUX=1" + "\r\n");
                    transaction(cipmux, kDefaultTimeoutMs, "OK");
                    byte[] cipstart = MakeByteArrayFromString("AT+CIPSTART=" + id + ",\"TCP\",\"" + remoteIP + "\"," + remotePort + "\r\n");
                    transaction(cipstart, kDefaultTimeoutMs, "OK");
                }

                public int sendTCP(int id, byte[] data)
                {
                    int retval = 0;

                    int offset = 0, len = data.Length;

                    while ((len > 0) && (retval == 0))
                    {
                        int chunk = len;
                        if (chunk > 1024) { chunk = 1024; }

                        retval |= sendUDP(id, data,chunk, offset);

                        offset += chunk;
                        len -= chunk;
                    }

                    return retval;
                }

                public int closeTCP(int id)
                {
                    return stopUDP(id);
                }

                //TCP Server used to manage connections being made to ESP12F.
                //  The ESP1F will assign a channel ID when the connection is made (0 indexed with maximum 5 connections).
                public int openTCPServer(int localPort) //port is specified
                {
                    int retval = 0;
                    byte[] cipmux = MakeByteArrayFromString("AT+CIPMUX=1" + "\r\n");
                    retval |= transaction(cipmux, kDefaultTimeoutMs, "OK");
                    byte[] cipstart = MakeByteArrayFromString("AT+CIPSERVER=1," + localPort + "\r\n");
                    retval |= transaction(cipstart, kDefaultTimeoutMs, "OK");
                    return retval;
                }

                public void openTCPServer() //overload for non-specified port (default is 333)
                {
                    byte[] cipmux = MakeByteArrayFromString("AT+CIPMUX=1" + "\r\n");
                    transaction(cipmux, kDefaultTimeoutMs, "OK");
                    byte[] cipstart = MakeByteArrayFromString("AT+CIPSERVER=1\r\n");
                    transaction(cipstart, kDefaultTimeoutMs, "OK");
                }

                public int connect(string _ssid, string password, int timeoutMs = 10000)
                {
                    byte[] toSend = MakeByteArrayFromString("AT+CWJAP_CUR=" + "\"" + _ssid + "\",\"" + password + "\"\r\n");

                    int retval = transaction(toSend, timeoutMs, null);

                    if (retval == 0)
                    {
                        if (!respContains("WIFI CONNECTED"))
                            retval = -1;
                        else if (!respContains("OK"))
                            retval = -1;
                        else
                            retval = 0;
                    }
                    return retval;
                }

                public int PingCheck(string ip, int timeoutMs = 1000)
                {
                    byte[] toSend = MakeByteArrayFromString("AT+PING=\"" + ip + "\"\r\n");
                    return transaction(toSend, timeoutMs, "OK");
                }


                public void disconnect()
                {
                    byte[] toSend = MakeByteArrayFromString("AT+CWQAP\r\n");
                    transaction(toSend, kDefaultTimeoutMs, "OK");
                }

                public int reset()
                {
                    byte[] reset = MakeByteArrayFromString("AT+RST" + "\r\n");
                    return transaction(reset, kDefaultTimeoutMs, "OK");
                }
                //public int mdns()
                //{
                //    byte[] reset = MakeByteArrayFromString("AT+MDNS=1," + "hello" + "," + "iot" + ",8080" + "\r\n");
                //    return transaction(reset, kDefaultTimeoutMs, "OK");
                //}
                public void FactoryReset()
                {
                    byte[] reset = MakeByteArrayFromString("AT+RESTORE\r\n");
                    transaction(reset, kDefaultTimeoutMs, "OK");
                }
                public bool setCommRate(int baudRate)
                {
                    string cur = "AT+UART_CUR=" + baudRate + ",8,1,0,3\r\n";
                    byte[] uart_cur = MakeByteArrayFromString(cur);

                    Debug.Print(cur);
                    uart.Write(uart_cur, 0, uart_cur.Length);
                    System.Threading.Thread.Sleep(10);

                    baud = baudRate;
                    uart.Close();
                    uart.BaudRate = baud;
                    uart.DiscardInBuffer();
                    uart.Open();

                    return true;
                }

                private byte[] MakeByteArrayFromString(String msg)
                {
                    byte[] retval = new byte[msg.Length];
                    for (int i = 0; i < msg.Length; ++i)
                        retval[i] = (byte)msg[i];
                    return retval;
                }
                public byte[] getDataCache()
                {
                    return dataCache;
                }

                public int transferDataCache(byte[] outsideCache)
                {
                    if (outsideCache.Length >= dataCache.Length)
                    {
                        for (int i = 0; i < dataCache.Length; i++)
                        {
                            outsideCache[i] = dataCache[i];
                        }

                        return dataCache.Length;
                    }
                    else { return -1; }
                }

                public bool processInput()
                {
                    if (uart.BytesToRead > 0)
                    {
                        int readCnt = uart.Read(_rx, 0, uart.BytesToRead);
                        for (int i = 0; i < readCnt; ++i)
                        {
                            if (processByte(_rx[i]))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                public bool processByte(byte data)
                {
                    switch (processState)
                    {
                        case READY:
                            {
                                if (data == '+') { processState = HEADER; }
                                break;
                            }
                        case HEADER:
                            {
                                switch (headerState)
                                {
                                    case STAGE1:
                                        {
                                            if (data == 'I') { headerState = STAGE2; }
                                            else { headerState = STAGE1; processState = READY; }
                                            break;
                                        }
                                    case STAGE2:
                                        {
                                            if (data == 'P') { headerState = STAGE3; }
                                            else { headerState = STAGE1; processState = READY; }
                                            break;
                                        }
                                    case STAGE3:
                                        {
                                            if (data == 'D') { headerState = STAGE4; }
                                            else { headerState = STAGE1; processState = READY; }
                                            break;
                                        }
                                    case STAGE4:
                                        {
                                            if (data == ',') { headerState = STAGE1; processState = LINKID; }
                                            else { headerState = STAGE1; processState = READY; }
                                            break;
                                        }
                                }
                                break;
                            }
                        case LINKID:
                            {
                                switch (linkIDState)
                                {
                                    case STAGE1:
                                        {
                                            processedLinkID = data - '0'; //being directly from the chip it's ascii encoded
                                            linkIDState = STAGE2;
                                            break;
                                        }
                                    case STAGE2:
                                        {
                                            if (data == ',') { linkIDState = STAGE1; processState = SIZE; }
                                            else { linkIDState = STAGE1; processState = READY; }
                                            break;
                                        }
                                }
                                break;
                            }
                        case SIZE:
                            {
                                if (data == ':') { processState = DATA; }
                                else
                                {
                                    int digit = data - '0';
                                    processedSize = (processedSize * 10) + digit;
                                }
                                break;
                            }
                        case DATA:
                            {
                                //Debug.Print("DataCount: " + dataCount + "  Data: " + data);
                                if (dataCount == 0) {
                                    dataCache = null;
                                    dataCache = new byte[processedSize];
                               }

                                if (dataCount < processedSize)
                                {
                                    dataCache[dataCount] = data;
                                    dataCount++;
                                }

                                if (dataCount == processedSize)
                                {
                                    processState = READY;
                                    dataCount = 0;
                                    processedSize = 0;

                                    return true;
                                }

                                break;
                            }
                        default: { break; }
                    }

                    return false;
                }

                private bool Contains(char[] array, char item)
                {
                    bool found = false;

                    foreach (char element in array)
                    {
                        if (element == item)
                            found = true;
                    }

                    return found;
                }


                //bool CheckPresence()
                //{
                //    bool temp1 = resetPin.Read();
                //    bool temp2 = GPIO.Read();
                //    Debug.Print("Reset Pin: " + temp1 + "   | GPIO0: " + temp2 + "\r\n");
                //    modulePresent = temp1 && temp2;
                //    return modulePresent;
                //}

                public void process(byte[] input, int len)
                {
                    for (int i = 0; i < len; i++)
                    {
                        process((char)input[i]);
                    }
                }

                public void process(char input)
                {
                    if (input == '\n')
                    {
                        if (temp.ToString() == "OK" || temp.ToString() == "FAIL" || temp.ToString() == "ERROR" || temp.ToString() == "ready")
                        {
                            isDone = true;
                        }

                        if (temp.ToString() != "\n" && temp.ToString() != "\r" && temp.ToString() != null)
                        {
                            lines.Add(temp.ToString());
                            Debug.Print(temp.ToString());
                        }

                        temp.Clear();
                    }
                    else
                    {
                        if (input != '\r')
                        {
                            temp.Append(input);
                        }
                    }
                }

                bool respContains(String toSearch)
                {
                    System.Collections.ArrayList arrayList = lines;
                    foreach (Object value in arrayList)
                    {
                        if (toSearch.Equals(value))
                            return true;
                    }
                    return false;
                }
                void clear()
                {
                    isDone = false;
                    lines.Clear();
                }

                bool waitForResponse(String toSearch, int timeoutMs, bool failEarlyIfBadIo = true)
                {
                    _timeSched.Restart(timeoutMs);

                    while (_timeSched.Process() == false)
                    {
                        if (respContains(toSearch))
                            return true;
                        if (failEarlyIfBadIo)
                        {
                            //todo
                        }
                        System.Threading.Thread.Sleep(1);
                        if (uart.BytesToRead > 0)
                        {
                            _timeSched.Restart(timeoutMs);

                            int readCnt = uart.Read(_rx, 0, _rx.Length);
                            process(_rx, readCnt);
                        }
                    }

                    return false;
                }

                bool waitForDone(int timeoutMs, bool failEarlyIfBadIo = true)
                {
                    _timeSched.Restart(timeoutMs);

                    while (_timeSched.Process() == false)
                    {
                        if (isDone)
                            return true;
                        if (failEarlyIfBadIo)
                        {
                            //todo
                        }
                        System.Threading.Thread.Sleep(1);
                        if (uart.BytesToRead > 0)
                        {
                            _timeSched.Restart(timeoutMs);

                            int readCnt = uart.Read(_rx, 0, _rx.Length);
                            process(_rx, readCnt);
                        }
                    }
                    return false;
                }
            }
        }
    }
}
