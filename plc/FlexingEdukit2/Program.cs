﻿
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace SmartConnector.Edukit
{
    class Program
    {

        private static void Main(string[] args)
        {
            var instance = new Service();
            instance.Start();
        }


        public class Service
        {

            private static Ecng.Net.SocketIO.Client.Socket ServerSocket;
            public static MqttClient mqttClient;
            static EdgeConfig edgeConfigResult = null;

            //internal : 동일한 어셈블리 내 코드에서 접근, async는 비동기 메서드  
            internal async void Start()
            {

                SetConfig();
                await Connect();
            }
            private static void SetConfig()
            {



                string fullpathFile = AppDomain.CurrentDomain.BaseDirectory;

                //Console.WriteLine(fullpathFile);


                string EdgeConfigFile = fullpathFile + "//EdgeConfigFile.json";
                //string EdgeConfigFile = "./EdgeConfigFile.json";

                //logmanager.log.Debug(EdgeConfigFile);
                string edgeConfig = File.ReadAllText(EdgeConfigFile);
                edgeConfigResult = JsonConvert.DeserializeObject<EdgeConfig>(edgeConfig);
                //logmanager.EnsureLogger();
            }

            public async Task<Boolean> Connect()
            {
                try
                {

                    //192.168.0.122 : 2004
                    var ip = edgeConfigResult.EdukitIP;
                    var port = Int32.Parse(edgeConfigResult.EdukitPort);
                    int DelayTime = Int32.Parse(edgeConfigResult.DelayTime);
                    XGTAddressData pAddress2 = new XGTAddressData();
                    XGTAddressData test1 = new XGTAddressData();
                    XGTClass xGTClass = new XGTClass(ip, port);

                    int mqttport = Int32.Parse(edgeConfigResult.MqttBrokerPort);
                    mqttClient = new MqttClient(edgeConfigResult.MqttBrokerIP, mqttport, false, null, null, MqttSslProtocols.TLSv1_2);
                    mqttClient.ProtocolVersion = MqttProtocolVersion.Version_3_1_1;    // 기본값 3.1.1, 버전이 맞아야 연결된다.
                    byte code = mqttClient.Connect(Guid.NewGuid().ToString());
                    //mqttClient.MqttMsgPublishReceived += client_MqttMsgPublishReceived;   // Received

                    //mqttClient.ConnectionClosed += MqttClient_ConnectionClosed;

                    //byte code = mqttClient.Connect(Guid.NewGuid().ToString());                                      // clientID
                    mqttClient.Subscribe(new string[] { "edukit2/control" },
                       new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

                    // 이벤트 핸들러 등록
                    mqttClient.MqttMsgPublishReceived += MqttMsgReceived;

                    // 이벤트 핸들러 정의
                    void MqttMsgReceived(object sender, MqttMsgPublishEventArgs e)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(e.Message);
                        dynamic test = JsonConvert.DeserializeObject<test>(receivedMessage);
                        string topic = e.Topic;
                        Console.WriteLine(test);
                        if (test.tagId.Equals("1")) //start
                        {
                            if (test.value.Equals("0"))
                            {
                                pAddress2.Address = "0";
                                pAddress2.Data = "0";
                                test1.Address = "22";
                                test1.Data = "0";


                               xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                               xGTClass.Write(XGT_DataType.Bit, test1, XGT_MemoryType.IO_P, 0);
                            }
                            else if (test.value.Equals("1"))
                            {
                                pAddress2.Address = "0";
                                pAddress2.Data = "1";
                               test1.Address = "22";
                                test1.Data = "1";

                               xGTClass.Write(XGT_DataType.Bit, test1, XGT_MemoryType.IO_P, 0);
                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                            }
                        }
                        else if (test.tagId.Equals("8")) //reset
                        {
                            pAddress2.Address = "F";
                            pAddress2.Data = "1";
                            xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);

                            pAddress2.Address = "F";
                            pAddress2.Data = "0";
                            xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                        }
                        else if (test.tagId.Equals("9")) // 1호기 ON/OFF
                        {
                            if (test.value.Equals("0"))
                            {
                                pAddress2.Address = "8F";
                                pAddress2.Data = "0";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                            }
                            else if (test.value.Equals("1"))
                            {
                                pAddress2.Address = "8F";
                                pAddress2.Data = "1";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                            }
                        }
                        else if (test.tagId.Equals("10")) // 2호기 ON/OFF
                        {
                            if (test.value.Equals("0"))
                            {
                                pAddress2.Address = "9F";
                                pAddress2.Data = "0";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                            }
                            else if (test.value.Equals("1"))
                            {
                                pAddress2.Address = "9F";
                                pAddress2.Data = "1";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                            }
                        }
                        else if (test.tagId.Equals("11")) // 3호기 ON/OFF
                        {
                            if (test.value.Equals("0"))
                            {
                                pAddress2.Address = "10E";
                                pAddress2.Data = "0";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                            }
                            else if (test.value.Equals("1"))
                            {
                                pAddress2.Address = "10E";
                                pAddress2.Data = "1";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                            }     
                        }
                        else if (test.tagId.Equals("12")) // sensor1 ON/OFF
                        {
                            if (test.value.Equals("0"))
                            {
                                pAddress2.Address = "6F";
                                pAddress2.Data = "0";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                            }
                            else if (test.value.Equals("1"))
                            {
                                pAddress2.Address = "6F";
                                pAddress2.Data = "1";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                            }
                        }
                        else if (test.tagId.Equals("13")) // sensor2 ON/OFF
                        {
                            if (test.value.Equals("0"))
                            {
                                pAddress2.Address = "7F";
                                pAddress2.Data = "0";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                            }
                            else if (test.value.Equals("1"))
                            {
                                pAddress2.Address = "7F";
                                pAddress2.Data = "1";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                            }
                        }
                        else if (test.tagId.Equals("14")) // 공정반복시간
                        {
                            pAddress2.Address = "1101";
                            pAddress2.Data = test.value;
                            
                            xGTClass.Write(XGT_DataType.Word, pAddress2, XGT_MemoryType.DataRegister_D, 0);
  
                        }
                        else if (test.tagId.Equals("31")) // 2호기 운전방법
                        {
                            if (test.value.Equals("0"))
                            {
                                pAddress2.Address = "18";
                                pAddress2.Data = "0";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                            }
                            else if (test.value.Equals("1"))
                            {
                                pAddress2.Address = "18";
                                pAddress2.Data = "1";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                            }
                        }
                        else if (test.tagId.Equals("36")) // 생산량 리미트
                        {
                            pAddress2.Address = "10000";
                            pAddress2.Data = test.value;

                            xGTClass.Write(XGT_DataType.Word, pAddress2, XGT_MemoryType.DataRegister_D, 0);
                        }
                        else if (test.tagId.Equals("38")) // 주사위 비교숫자
                        {
                            pAddress2.Address = "150";
                            pAddress2.Data = test.value;

                            xGTClass.Write(XGT_DataType.Word, pAddress2, XGT_MemoryType.DataRegister_D, 0);
                        }
                        else if (test.tagId.Equals("100")) // 3호기 1축 수동제어
                        {
                            if (test.value.Equals("10")) // 1축 up stop
                            {
                                pAddress2.Address = "4360";
                                pAddress2.Data = "0";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.KeepRelay_K, 0);
                            }
                            else if (test.value.Equals("11")) // 1축 up
                            {
                                pAddress2.Address = "4360";
                                pAddress2.Data = "1";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.KeepRelay_K, 0);
                            }
                            else if (test.value.Equals("20")) // 1축 down stop
                            {
                                pAddress2.Address = "4361";
                                pAddress2.Data = "0";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.KeepRelay_K, 0);
                            }
                            else if (test.value.Equals("21")) // 1축 down
                            {
                                pAddress2.Address = "4361";
                                pAddress2.Data = "1";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.KeepRelay_K, 0);
                            }
                        }
                        else if (test.tagId.Equals("200")) // 3호기 2축 수동제어
                        {
                            if (test.value.Equals("10")) // 2축 front stop
                            {
                                pAddress2.Address = "4560";
                                pAddress2.Data = "0";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.KeepRelay_K, 0);
                            }
                            else if (test.value.Equals("11")) // 2축 front
                            {
                                pAddress2.Address = "4560";
                                pAddress2.Data = "1";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.KeepRelay_K, 0);
                            }
                            else if (test.value.Equals("20")) // 2축 back stop
                            {
                                pAddress2.Address = "4561";
                                pAddress2.Data = "0";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.KeepRelay_K, 0);
                            }
                            else if (test.value.Equals("21")) // 2축 back
                            {
                                pAddress2.Address = "4561";
                                pAddress2.Data = "1";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.KeepRelay_K, 0);
                            }
                        }
                        else if (test.tagId.Equals("300")) // 2호기 운전방법
                        {
                            if (test.value.Equals("0"))
                            {
                                pAddress2.Address = "15";
                                pAddress2.Data = "0";

                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                            }
                            else if (test.value.Equals("1"))
                            {
                                test1.Address = "F";
                                test1.Data = "1";
                                xGTClass.Write(XGT_DataType.Bit, test1, XGT_MemoryType.SubRelay_M, 0);

                                test1.Address = "F";
                                test1.Data = "0";
                                xGTClass.Write(XGT_DataType.Bit, test1, XGT_MemoryType.SubRelay_M, 0);
                                pAddress2.Address = "15";
                                pAddress2.Data = "1";
                                xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                                                               
                            }
                        }

                    }

                    string websocket = edgeConfigResult.WebSocketServerUrl;
                    var query = new Dictionary<string, string>()
                  {
                     { "type", "EDGE" },
                     { "id", edgeConfigResult.EdukitId }
                  };
                    var options = new Ecng.Net.SocketIO.Client.IO.Options();
                    options.Query = query;
                    ServerSocket = Ecng.Net.SocketIO.Client.IO.Socket(websocket, options);

                    ServerSocket.Unhandled += ServerSocket_Unhandled;
                    Console.WriteLine("##########################");
                    Console.WriteLine("Edukit Connection State : True");
                    Console.WriteLine("Edukit IP : " + ip);
                    Console.WriteLine("Edukit PORT : " + port);
                    Console.WriteLine("##########################");

                    Console.WriteLine("mqtt : " + edgeConfigResult.MqttBrokerIP);
                    Console.WriteLine("mqttport : " + edgeConfigResult.MqttBrokerPort);
                    Console.WriteLine("websocket : " + websocket);
                    Console.WriteLine("delayTime : " + edgeConfigResult.DelayTime);

                    ConnectionStart(DelayTime, xGTClass, ip, port);

                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message + "\n" + ex.StackTrace);

                }

                return true;
            }

            private void ServerSocket_Unhandled(string arg1, object[] arg2)
            {

                //Console.WriteLine("lost socket");


                ServerSocket.Emit("joinRoom", edgeConfigResult.EdukitId);

                //throw new NotImplementedException();
            }

            private void ConnectionStart(int DelayTime, XGTClass xGTClass, string ip, int port)
            {


                xGTClass.Connect(ip, port);


                ServerSocket.Emit("joinRoom", edgeConfigResult.EdukitId);




                ServerSocket.On($"SEND{edgeConfigResult.EdukitId}", (msg) =>
                {
                    Console.WriteLine("event!!");
                    XGTAddressData pAddress2 = new XGTAddressData();
                    XGTAddressData test1 = new XGTAddressData();

                    dynamic test = JsonConvert.DeserializeObject<test>(msg.ToString());
                    //Console.WriteLine("test: "+test);

                    if (test.tagId.Equals("1")) //start
                    {
                        if (test.value.Equals("0"))
                        {
                            pAddress2.Address = "0";
                            pAddress2.Data = "0";
                            test1.Address = "22";
                            test1.Data = "0";


                            xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                            xGTClass.Write(XGT_DataType.Bit, test1, XGT_MemoryType.IO_P, 0);
                        }
                        else if (test.value.Equals("1"))
                        {
                            pAddress2.Address = "0";
                            pAddress2.Data = "1";
                            test1.Address = "22";
                            test1.Data = "1";

                            xGTClass.Write(XGT_DataType.Bit, test1, XGT_MemoryType.IO_P, 0);
                            xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                        }
                    }
                    else if (test.tagId.Equals("8")) //reset
                    {
                        pAddress2.Address = "F";
                        pAddress2.Data = "1";
                        xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);

                        pAddress2.Address = "F";
                        pAddress2.Data = "0";
                        xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                    }
                    else if (test.tagId.Equals("9")) // 1호기 ON/OFF
                    {
                        if (test.value.Equals("0"))
                        {
                            pAddress2.Address = "8F";
                            pAddress2.Data = "0";

                            xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                        }
                        else if (test.value.Equals("1"))
                        {
                            pAddress2.Address = "8F";
                            pAddress2.Data = "1";

                            xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                        }
                    }
                    else if (test.tagId.Equals("10")) // 2호기 ON/OFF
                    {
                        if (test.value.Equals("0"))
                        {
                            pAddress2.Address = "9F";
                            pAddress2.Data = "0";

                            xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                        }
                        else if (test.value.Equals("1"))
                        {
                            pAddress2.Address = "9F";
                            pAddress2.Data = "1";

                            xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                        }
                    }
                    else if (test.tagId.Equals("11")) // 3호기 ON/OFF
                    {
                        if (test.value.Equals("0"))
                        {
                            pAddress2.Address = "10E";
                            pAddress2.Data = "0";

                            xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                        }
                        else if (test.value.Equals("1"))
                        {
                            pAddress2.Address = "10E";
                            pAddress2.Data = "1";

                            xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                        }
                    }
                    else if (test.tagId.Equals("12")) // sensor1 ON/OFF
                    {
                        if (test.value.Equals("0"))
                        {
                            pAddress2.Address = "6F";
                            pAddress2.Data = "0";

                            xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                        }
                        else if (test.value.Equals("1"))
                        {
                            pAddress2.Address = "6F";
                            pAddress2.Data = "1";

                            xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                        }
                    }
                    else if (test.tagId.Equals("13")) // sensor2 ON/OFF
                    {
                        if (test.value.Equals("0"))
                        {
                            pAddress2.Address = "7F";
                            pAddress2.Data = "0";

                            xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                        }
                        else if (test.value.Equals("1"))
                        {
                            pAddress2.Address = "7F";
                            pAddress2.Data = "1";

                            xGTClass.Write(XGT_DataType.Bit, pAddress2, XGT_MemoryType.SubRelay_M, 0);
                        }
                    }
                });



                //Dictionary<XGTAddressData, string> PLCAddressList = new Dictionary<XGTAddressData, string>();

                Dictionary<XGTAddressData, string> BitAddressList = new Dictionary<XGTAddressData, string>();
                XGTAddressData Start = new XGTAddressData();            //시작 M0000 bit
                XGTAddressData No1PartsError = new XGTAddressData();         // no1 칩없음 M0011 bit 1이 정상 0이 칩없음

                XGTAddressData No1_Action = new XGTAddressData();         // no1 move M92 bit                        
                XGTAddressData No2_Action = new XGTAddressData();        // no2 move M00104 bit

                XGTAddressData No3Ready = new XGTAddressData();        // no3 칩도착 P0000E bit
                XGTAddressData Sensor1 = new XGTAddressData();          //colorsensor on M105 bit

                XGTAddressData Reset = new XGTAddressData();            //Reset M0F BIT

                XGTAddressData no1_on_off = new XGTAddressData(); //1호기 on/off M0008F bit
                XGTAddressData no2_on_off = new XGTAddressData(); //2호기 on/off M0009F bit
                XGTAddressData no3_on_off = new XGTAddressData(); //3호기 on/off M0010E bit
                XGTAddressData sensor1_on_off = new XGTAddressData();   //sensor1 on/off M0006F bit
                XGTAddressData sensor2_on_off = new XGTAddressData();   //sensor2 on/off M0007F bit
                XGTAddressData lamp_green = new XGTAddressData();   //sensor2 on/off M0007F bit
                XGTAddressData lamp_yellow = new XGTAddressData();   //sensor2 on/off M0007F bit
                XGTAddressData lamp_red = new XGTAddressData();   //sensor2 on/off M0007F bit

                Dictionary<XGTAddressData, string> WordAddressList = new Dictionary<XGTAddressData, string>();
                XGTAddressData No1Delay = new XGTAddressData();        //1호기 delay 시간  D01101 WORD
                XGTAddressData No1Count = new XGTAddressData();        //1호기 수량 C0001 WORD
                XGTAddressData No2Count = new XGTAddressData();        //2호기 수량 C0002 WORD
                XGTAddressData No3Count = new XGTAddressData();        //3호기 수량 C0003 WORD
                XGTAddressData Sensor2 = new XGTAddressData();          //visionSensor on C05 WORD

                XGTAddressData no3_motor1 = new XGTAddressData();          //3호기 축1 value K424 WORD
                XGTAddressData no3_motor2 = new XGTAddressData();          //3호기 축2 value K444 WORD

                // Xr Team 요청으로 인한 Tag Data 추가

                XGTAddressData No1ChipFull = new XGTAddressData();          // 1호기 칩 유무 P00009 bit
                XGTAddressData No2Chip = new XGTAddressData();              // 2호기 칩 도착 P0000A bit
                XGTAddressData No2CubeFull = new XGTAddressData();          // 2호기 주사위 유무 P0000B bit
                XGTAddressData No2InPoint = new XGTAddressData();           // 2호기 in point P0000C bit
                XGTAddressData No2OutPoint = new XGTAddressData();          // 2호기 out point P0000D bit
                XGTAddressData No2Sol = new XGTAddressData();               // 2호기 솔레노이드 P0002A bit
                XGTAddressData No2SolAction = new XGTAddressData();         // 2호기 솔작동 M00106 WORD
                XGTAddressData No2BackToSquare = new XGTAddressData();      // 2호기 원점으로 M00107 WORD
                XGTAddressData No2Mode = new XGTAddressData();              // 2호기 운전방법 M00018 WORD
                XGTAddressData No3Chip = new XGTAddressData();              // 3호기 칩 도착 P0000E bit
                XGTAddressData VisionCmdMemory = new XGTAddressData();      // 비젼지령기억 M0001C bit
                XGTAddressData No3DiceReading = new XGTAddressData();       // 주사위판독 C0004 WORD
                XGTAddressData Emergency = new XGTAddressData();            // 비상정지 P0000F bit
                XGTAddressData OutputLimit = new XGTAddressData();          // 생산량 리미트 D10000 WORD
                XGTAddressData DiceValue = new XGTAddressData();            // 주사위값 D01100 WORD
                XGTAddressData DiceComparisonValue = new XGTAddressData();  // 주사위 비교 숫자 D00150 WORD
                XGTAddressData ColorSensorSensing = new XGTAddressData();   // 컬러센서센싱 P00004 bit
                XGTAddressData No3Gripper = new XGTAddressData();           // 3호기 그리퍼 P0002B bit
                XGTAddressData Motor1Busy = new XGTAddressData();           // 1축 운전중 K04200 WORD
                XGTAddressData Motor2Busy = new XGTAddressData();           // 2축 운전중 K04400 WORD
                XGTAddressData _POS_1_CurSpd = new XGTAddressData();           // 3호기 1축 현재속도 K0426 WORD
                XGTAddressData _POS_2_CurSpd = new XGTAddressData();           // 3호기 2축 현재속도 K0446 WORD
                XGTAddressData No1_in_point = new XGTAddressData();           // 1호기 in point P00008
                XGTAddressData No1_Motar = new XGTAddressData();            // 1호기 motor P00028

                Start.Address = "0";
                Start.Name = "Start";
                Start.TagId = "1";

                No1PartsError.Address = "11";
                No1PartsError.Name = "No1PartsError";
                No1PartsError.TagId = "2";


                No1_Action.Address = "92";
                No1_Action.Name = "No1_Action";
                No1_Action.TagId = "3";


                No2_Action.Address = "104";
                No2_Action.Name = "No2_Action";
                No2_Action.TagId = "4";

                No3Ready.Address = "0E";
                No3Ready.Name = "No3Ready";
                No3Ready.TagId = "5";

                Sensor1.Address = "105";
                Sensor1.Name = "ColorSensor"; //version 변경 M00105 color값 true
                Sensor1.TagId = "6";

                Sensor2.Address = "5";
                Sensor2.Name = "VisionSensor"; //version 변경
                Sensor2.TagId = "7";

                Reset.Address = "0F";
                Reset.Name = "Reset";
                Reset.TagId = "8";

                //1호기 on/off M0008F bit
                //2호기 on/off M0009F bit
                //3호기 on/off M0010E bit
                //sensor1 on/off M0006F bit
                //sensor2 on/off M0007F bit

                no1_on_off.Address = "8F";
                no1_on_off.Name = "no1_on_off";
                no1_on_off.TagId = "9";

                no2_on_off.Address = "9F";
                no2_on_off.Name = "no2_on_off";
                no2_on_off.TagId = "10";

                no3_on_off.Address = "10E";
                no3_on_off.Name = "no3_on_off";
                no3_on_off.TagId = "11";

                sensor1_on_off.Address = "6F";
                sensor1_on_off.Name = "sensor1_on_off";
                sensor1_on_off.TagId = "12";

                sensor2_on_off.Address = "7F";
                sensor2_on_off.Name = "sensor2_on_off";
                sensor2_on_off.TagId = "13";


                No1Delay.Address = "1101";
                No1Delay.Name = "No1Delay";
                No1Delay.TagId = "14";

                No1Count.Address = "1";
                No1Count.Name = "No1Count";
                No1Count.TagId = "15";

                No2Count.Address = "2";
                No2Count.Name = "No2Count";
                No2Count.TagId = "16";

                No3Count.Address = "7";
                No3Count.Name = "No3Count";
                No3Count.TagId = "17";

                lamp_green.Address = "2C";
                lamp_green.Name = "lamp_green";
                lamp_green.TagId = "18";

                lamp_yellow.Address = "2D";
                lamp_yellow.Name = "lamp_yellow";
                lamp_yellow.TagId = "19";

                lamp_red.Address = "2E";
                lamp_red.Name = "lamp_red";
                lamp_red.TagId = "20";

                no3_motor1.Address = "424";
                no3_motor1.Name = "No3Motor1";
                no3_motor1.TagId = "21";

                no3_motor2.Address = "444";
                no3_motor2.Name = "No3Motor2";
                no3_motor2.TagId = "22";

                // Xr Team 요청으로 인한 Tag Data 추가

                No1ChipFull.Address = "9";
                No1ChipFull.Name = "No1ChipFull";
                No1ChipFull.TagId = "23";

                No2Chip.Address = "A";
                No2Chip.Name = "No2Chip";
                No2Chip.TagId = "24";

                No2CubeFull.Address = "B";
                No2CubeFull.Name = "No2CubeFull";
                No2CubeFull.TagId = "25";

                No2InPoint.Address = "C";
                No2InPoint.Name = "No2InPoint";
                No2InPoint.TagId = "26";

                No2OutPoint.Address = "D";
                No2OutPoint.Name = "No2OutPoint";
                No2OutPoint.TagId = "27";

                No2Sol.Address = "2A";
                No2Sol.Name = "No2Sol";
                No2Sol.TagId = "28";

                No2SolAction.Address = "106";
                No2SolAction.Name = "No2SolAction";
                No2SolAction.TagId = "29";

                No2BackToSquare.Address = "107";
                No2BackToSquare.Name = "No2BackToSquare";
                No2BackToSquare.TagId = "30";

                No2Mode.Address = "18";
                No2Mode.Name = "No2Mode";
                No2Mode.TagId = "31";

                No3Chip.Address = "E";
                No3Chip.Name = "No3Chip";
                No3Chip.TagId = "32";

                VisionCmdMemory.Address = "1C";
                VisionCmdMemory.Name = "VisionCmdMemory";
                VisionCmdMemory.TagId = "33";

                No3DiceReading.Address = "4";
                No3DiceReading.Name = "No3DiceReading";
                No3DiceReading.TagId = "34";

                Emergency.Address = "F";
                Emergency.Name = "Emergency";
                Emergency.TagId = "35";

                OutputLimit.Address = "10000";
                OutputLimit.Name = "OutputLimit";
                OutputLimit.TagId = "36";

                DiceValue.Address = "1100";
                DiceValue.Name = "DiceValue";
                DiceValue.TagId = "37";

                DiceComparisonValue.Address = "150";
                DiceComparisonValue.Name = "DiceComparisonValue";
                DiceComparisonValue.TagId = "38";

                ColorSensorSensing.Address = "4";
                ColorSensorSensing.Name = "ColorSensorSensing";
                ColorSensorSensing.TagId = "39";

                No3Gripper.Address = "2B";
                No3Gripper.Name = "No3Gripper";
                No3Gripper.TagId = "40";

                Motor1Busy.Address = "42";
                Motor1Busy.Name = "Motor1Busy";
                Motor1Busy.TagId = "41";

                Motor2Busy.Address = "44";
                Motor2Busy.Name = "Motor2Busy";
                Motor2Busy.TagId = "42";

                _POS_1_CurSpd.Address = "426";
                _POS_1_CurSpd.Name = "_POS_1_CurSpd";
                _POS_1_CurSpd.TagId = "43";

                _POS_2_CurSpd.Address = "446";
                _POS_2_CurSpd.Name = "_POS_2_CurSpd";
                _POS_2_CurSpd.TagId = "44";

                No1_in_point.Address = "8";
                No1_in_point.Name = "No1_in_point";
                No1_in_point.TagId = "45";

                No1_Motar.Address = "28";
                No1_Motar.Name = "No1_Motar";
                No1_Motar.TagId = "46";

                BitAddressList.Add(Start, "M");
                BitAddressList.Add(No1PartsError, "M");

                BitAddressList.Add(No1_Action, "M");

                BitAddressList.Add(No2_Action, "M");

                BitAddressList.Add(No3Ready, "P");
                BitAddressList.Add(Sensor1, "M"); //version 변경
                WordAddressList.Add(Sensor2, "C"); //version 변경

                BitAddressList.Add(Reset, "M");

                BitAddressList.Add(no1_on_off, "M");
                BitAddressList.Add(no2_on_off, "M");
                BitAddressList.Add(no3_on_off, "M");
                BitAddressList.Add(sensor1_on_off, "M");
                BitAddressList.Add(sensor2_on_off, "M");

                WordAddressList.Add(No1Delay, "D");
                WordAddressList.Add(No1Count, "C");
                WordAddressList.Add(No2Count, "C");
                WordAddressList.Add(No3Count, "C");

                WordAddressList.Add(no3_motor1, "K");
                WordAddressList.Add(no3_motor2, "K");

                BitAddressList.Add(lamp_green, "P");
                BitAddressList.Add(lamp_yellow, "P");
                BitAddressList.Add(lamp_red, "P");

                // Xr Team 요청으로 인한 Tag Data 추가

                BitAddressList.Add(No1ChipFull, "P");
                BitAddressList.Add(No2Chip, "P");
                BitAddressList.Add(No2CubeFull, "P");
                BitAddressList.Add(No2InPoint, "P");
                BitAddressList.Add(No2OutPoint, "P");
                BitAddressList.Add(No2Sol, "P");

                BitAddressList.Add(No2SolAction, "M");
                BitAddressList.Add(No2BackToSquare, "M");
                BitAddressList.Add(No2Mode, "M");

                BitAddressList.Add(No3Chip, "P");

                BitAddressList.Add(VisionCmdMemory, "M");
                WordAddressList.Add(No3DiceReading, "C");

                BitAddressList.Add(Emergency, "P");

                WordAddressList.Add(OutputLimit, "D");
                WordAddressList.Add(DiceValue, "D");
                WordAddressList.Add(DiceComparisonValue, "D");

                BitAddressList.Add(ColorSensorSensing, "P");
                BitAddressList.Add(No3Gripper, "P");

                BitAddressList.Add(Motor1Busy, "K");
                BitAddressList.Add(Motor2Busy, "K");

                WordAddressList.Add(_POS_1_CurSpd, "K");
                WordAddressList.Add(_POS_2_CurSpd, "K");

                BitAddressList.Add(No1_in_point, "P");
                BitAddressList.Add(No1_Motar, "P");



                while (true)
                {
                    try
                    {
                        // no1 공급지연시간  D01101 WORD
                        // no1 생산량       C0001 WORD
                        // no2 생산량       C0002 WORD
                        // no3 생산량       C0003 WORD

                        //Dictionary<string, object> EduKitDataDIc = new Dictionary<string, object>();

                        List<EdukitNewdata> edukitData = new List<EdukitNewdata>();

                        List<EdukitNewdata> edukitMqttData = new List<EdukitNewdata>();

                        foreach (var address in BitAddressList)
                        {
                            XGTData val = null;
                            if (address.Value == "M")
                            {
                                val = xGTClass.Read(XGT_DataType.Bit, address.Key, XGT_MemoryType.SubRelay_M, 0);
                                if (val.DataList == null)
                                    continue;
                                if (val.DataList != null)
                                {
                                    if (val.DataList[0].IntData == 0)
                                    {
                                        EdukitNewdata newdata = new EdukitNewdata();
                                        newdata.name = address.Key.Name;
                                        newdata.tagId = address.Key.TagId;
                                        newdata.value = false;
                                        edukitData.Add(newdata);
                                    }
                                    else
                                    {
                                        EdukitNewdata newdata = new EdukitNewdata();

                                        newdata.name = address.Key.Name;
                                        newdata.tagId = address.Key.TagId;
                                        newdata.value = true;
                                        edukitData.Add(newdata);
                                    }
                                }
                                Thread.Sleep(DelayTime / 100);

                            }


                            else if (address.Value == "P")
                            {
                                val = xGTClass.Read(XGT_DataType.Bit, address.Key, XGT_MemoryType.IO_P, 0);

                                if (val.DataList != null)
                                {
                                    if (val.DataList[0].IntData == 0)
                                    {
                                        EdukitNewdata newdata = new EdukitNewdata();

                                        newdata.name = address.Key.Name;
                                        newdata.tagId = address.Key.TagId;
                                        newdata.value = false;
                                        edukitData.Add(newdata);
                                    }
                                    else
                                    {
                                        EdukitNewdata newdata = new EdukitNewdata();

                                        newdata.name = address.Key.Name;
                                        newdata.tagId = address.Key.TagId;
                                        newdata.value = true;
                                        edukitData.Add(newdata);
                                    }
                                }
                                Thread.Sleep(DelayTime / 100);
                            }
                        }
                        foreach (var address in WordAddressList)
                        {
                            XGTData val = null;

                            if (address.Value == "D")
                            {
                                val = xGTClass.Read(XGT_DataType.Word, address.Key, XGT_MemoryType.DataRegister_D, 0);

                                if (val.DataList != null)
                                {

                                    EdukitNewdata newdata = new EdukitNewdata();
                                    double data = (double)val.DataList[0].IntData;
                                    if (address.Key.Name == "No1Delay") data = (double)val.DataList[0].IntData / 10;

                                    newdata.name = address.Key.Name;
                                    newdata.tagId = address.Key.TagId;
                                    newdata.value = data.ToString();
                                    edukitData.Add(newdata);
                                    //EduKitData.Add(address.Key.Name, data);
                                }
                                Thread.Sleep(DelayTime / 100);
                            }
                            else if (address.Value == "C")
                            {
                                val = xGTClass.Read(XGT_DataType.Word, address.Key, XGT_MemoryType.Counter_C, 0);

                                if (val.DataList != null)
                                {

                                    EdukitNewdata newdata = new EdukitNewdata();
                                    newdata.name = address.Key.Name;
                                    newdata.tagId = address.Key.TagId;
                                    newdata.value = val.DataList[0].IntData.ToString();
                                    edukitData.Add(newdata);
                                    //EduKitData.Add(address.Key.Name, val.DataList[0].IntData);
                                }
                                Thread.Sleep(DelayTime / 100);
                            }
                            else if (address.Value == "K")
                            {
                                int test = Int32.Parse(address.Key.Address);
                                XGTAddressData q = new XGTAddressData();
                                q.Address = (test + 1).ToString();

                                XGTData val1 = xGTClass.Read(XGT_DataType.Word, address.Key, XGT_MemoryType.KeepRelay_K, 0);
                                XGTData val2 = xGTClass.Read(XGT_DataType.Word, q, XGT_MemoryType.KeepRelay_K, 0);
                                
                                if (val1.DataList != null && val2.DataList != null)
                                {
                                    int wordVal1 = val1.DataList[0].IntData;
                                    int wordVal2 = val2.DataList[0].IntData;
                                    long dWordVal = (wordVal2 * 65536) + wordVal1;
                                    EdukitNewdata newdata = new EdukitNewdata
                                    {
                                        name = address.Key.Name,
                                        tagId = address.Key.TagId,
                                        value = dWordVal.ToString()
                                    };

                                    edukitData.Add(newdata);
                                    edukitMqttData.Add(newdata);
                                }

                                Thread.Sleep(DelayTime / 100);
                            }
                        }
                        
                        EdukitNewdata newdata2 = new EdukitNewdata();
                        newdata2.name = "DataTime";
                        newdata2.tagId = "0";
                        newdata2.value = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
                        edukitData.Add(newdata2);
                        //edukitMqttData.Add(newdata2);



                        MqttData mqttData = new MqttData();

                        mqttData.plcdata = edukitData;


                        SocketIoData(edukitData);
                        MqttData(mqttData);

                        if (edgeConfigResult.DebugType == "Debug")
                        {
                            Console.Clear();

                            List<EdukitNewdata> SortedList = edukitData.OrderBy(x => Int32.Parse(x.tagId)).ToList();

                            foreach (var data in SortedList)
                            {
                                Console.WriteLine($"[{data.tagId}]{data.name} : {data.value}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);

                    }


                    Thread.Sleep(DelayTime);

                }

            }

            static Task<string> SocketIoData(List<EdukitNewdata> EduKitData)
            {


                string data = JsonConvert.SerializeObject(EduKitData, Formatting.Indented);


                //StringContent queryString = new StringContent(json, Encoding.UTF8, "application/json");

                string EdukitID = edgeConfigResult.EdukitId;
                try
                {
                    //ServerSocket.Emit("joinRoom", "EDGEID1234");
                    ServerSocket.Emit("create", EdukitID, data);
                    //ServerSocket.EmitAsync("create", EdukitID, data);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "\n" + ex.StackTrace);

                }


                return null;
            }

            static void MqttData(MqttData EduKitData)
            {


                string json = JsonConvert.SerializeObject(EduKitData, Formatting.Indented);

                string data = json;

                string topic = "edukit2";
                try
                {
                    //mqtt send
                    //await Console.WriteLine(data);
                    mqttClient.Publish(topic, Encoding.Default.GetBytes(data),
                       MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                    //logmanager.log.Debug(ex.Message + "\n" + ex.StackTrace);
                }


            }


        }


        public class EdukitNewdata
        {
            public string tagId { get; set; }
            public string name { get; set; }
            public object value { get; set; }

        }


        public class EdgeConfig
        {
            public string EdukitId { get; set; }

            public string EdukitIP { get; set; }

            public string EdukitPort { get; set; }

            public string MqttBrokerIP { get; set; }
            public string MqttBrokerPort { get; set; }

            public string WebSocketServerUrl { get; set; }
            public string DelayTime { get; set; }
            public string DebugType { get; set; }

        }

        public class test
        {
            public string tagId { get; set; }
            public string value { get; set; }
        }

        public class MqttData
        {
            public List<EdukitNewdata> plcdata { get; set; }
        }
    }
}
