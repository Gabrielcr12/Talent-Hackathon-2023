using System;
using System.Threading;
using System.ComponentModel;
using System.IO.Ports;
using MavLink; 

namespace MavlinkWrapper
{
    public class Program
    {
        static BackgroundWorker HeartBeat;
        static SerialPort telemetryPort;

        static Mavlink MVL = new Mavlink();
        static Msg_heartbeat Hb = new Msg_heartbeat();

        static int Systemid;
        static int Componentid;
        static int Sequence;
        static Boolean HB = true;

        static DroneStates dronState = DroneStates.NOT_CONNECTED;

        enum DroneStates
        {
            NOT_CONNECTED,
            CONNECTED,
            ARMING,
            ARMED
        }

        public static int Main(string[] args)
        {
            try
            {
                Console.Write("Connecting...");
                telemetryPort = new SerialPort("COM15", 57600);
                telemetryPort.Open();
                
                telemetryPort.DataReceived += TelemetryPort_DataReceived;
                MVL.PacketReceived += MVL_PacketReceived;

                HeartBeat = new BackgroundWorker();
                HeartBeat.DoWork += HeartBeat_DoWork;
                HeartBeat.RunWorkerAsync();

                dronState = DroneStates.CONNECTED;
                Console.WriteLine("OK!");

                dronState = DroneStates.ARMING;
                Console.Write("Arming Drone...");
                Thread.Sleep(1000);
                Arm();
            }
            catch
            {
                telemetryPort.Close();
                return -1;
            }

            while (HB) ;
            telemetryPort.Close();
            Console.WriteLine("Press any key to Exit.");
            Console.ReadKey();
            return 0;
        }

        private static void Arm()
        {
            Msg_command_long msg = new Msg_command_long();
            msg.target_system = (byte)Systemid;
            msg.target_component = (byte)Componentid;
            msg.command = (ushort)MAV_CMD.MAV_CMD_COMPONENT_ARM_DISARM;
            msg.confirmation = 0;
            msg.param1 = 1;
            msg.param2 = 0;
            msg.param3 = 0;
            msg.param4 = 0;
            msg.param5 = 0;
            msg.param6 = 0;
            msg.param7 = 0;

            SendPacket(msg);
        }

        private static void HeartBeat_DoWork(object sender, DoWorkEventArgs e)
        {
            int sec;

            sec = 0;

            while (HB)
            {
                Msg_heartbeat hb = new Msg_heartbeat();
                if (sec != DateTime.Now.Second)
                {
                    hb.type = 6;
                    hb.system_status = 0;
                    hb.custom_mode = 0;
                    hb.base_mode = 0;
                    hb.autopilot = 0;

                    SendPacket(hb);
                    sec = DateTime.Now.Second;
                }
            }
        }

        private static void SendPacket(MavlinkMessage m)
        {
            MavlinkPacket p = new MavlinkPacket();
            p.Message = m;
            p.SequenceNumber = (byte)Sequence;
            p.SystemId = 255;
            p.ComponentId = (byte)MAV_COMPONENT.MAV_COMP_ID_MISSIONPLANNER;
            byte[] b = MVL.Send(p);
            telemetryPort.Write(b, 0, b.Length);
        }

        private static void MVL_PacketReceived(object sender, MavlinkPacket e)
        {
            uint x = MVL.PacketsReceived;
            Systemid = e.SystemId;
            Componentid = e.ComponentId;
            Sequence = e.SequenceNumber;
            MavlinkMessage m = e.Message;
            if (m.GetType() == Hb.GetType())
            {
                Hb = (Msg_heartbeat)e.Message;
            }

            if (dronState == DroneStates.ARMING)
            {
                if ((Hb.base_mode & (byte)MAV_MODE_FLAG.MAV_MODE_FLAG_SAFETY_ARMED) != 0)
                {
                    dronState = DroneStates.ARMED;
                    Console.WriteLine("OK!");
                    HB = false;
                }
                else
                {
                    Console.Write(".");
                }
            }
        }

        private static void TelemetryPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (telemetryPort.IsOpen)
            {
                int x = telemetryPort.BytesToRead;
                byte[] b = new byte[x];
                for (int i = 0; i < x; i++)
                {
                    b[i] = (byte)telemetryPort.ReadByte();
                }
                MVL.ParseBytes(b);
            }
        }
    }
}
