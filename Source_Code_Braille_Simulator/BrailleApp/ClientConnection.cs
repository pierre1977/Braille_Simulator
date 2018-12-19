using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.System.Threading;

namespace Braille.Lan
{
    // Command Enum
    public enum Commands
    {
        getDeviceInfos = 20,
        getDeviceList = 26,
        getMVBDVersion = 34,
        getDeviceGraphic = 52, 

        sendPinData = 21,
        sendKeyDown = 22,
        sendKeyUp = 23,
        sendFinger = 24,

        sendDevice = 27,
        sendClientId = 29
    }

    
    public class ClientConnection
    {
        // Connect Var
        private string ip;
        private int port;

        private bool isConnected = false;

        private StreamSocket socket;
        private DataWriter writer;

        // Var für Steuerung von Anfrage und Datenempfang
        private byte[] commandHeader;       // empfang
        private byte[] commandData;         // empfang

        private bool waitForData = false;   // prüfen ob anfrage für daten vorhanden ist
        private byte commandSend;           // send command


        // Event Handler
        // Error
        public delegate void Error(string message);
        public event Error OnError;

        // Datenempfang für gesendete Befehle bzw. Anfragen
        public delegate void DataRecivedCmd(byte[] data);
        public event DataRecivedCmd OnDataRecivedCmd;

        // Datenempfang ohne Anfrage
        public delegate void DataRecived(byte[] data);
        public event DataRecived OnDataRecived;

        // Datenempfang Fingerdaten
        public delegate void DataRecivedFinger(byte[] data);
        public event DataRecivedFinger OnDataRecivedFinger;

        // Datenempfang Pin
        public delegate void DataRecivedPin(byte[] data);
        public event DataRecivedPin OnDataRecivedPin;
        
        // Datenempfang Key Down
        public delegate void DataRecivedKeyDown(byte[] data);
        public event DataRecivedKeyDown OnDataRecivedKeyDown;

        // Datenempfang Key Up
        public delegate void DataRecivedKeyUp(byte[] data);
        public event DataRecivedKeyUp OnDataRecivedKeyUp;

        

        // Konstructor
        public ClientConnection()
        {            
        }

        public ClientConnection(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }


        // Connect
        public async void Connect()
        {
            try
            {
                // Socket Connect; hier muss "using" verwendet werden, da der GC await Threads bzw. Objekte zerstört
                using (socket = new StreamSocket())
                {
                    socket.Control.KeepAlive = true;
                    socket.Control.QualityOfService = SocketQualityOfService.LowLatency;

                    // Socket im Thread
                    var hostName = new HostName(Ip);
                    await socket.ConnectAsync(hostName, Port.ToString());

                    // Writer erstellen
                    writer = new DataWriter(socket.OutputStream);

                    this.isConnected = true;
                    
                    // Reader Funktion; am ende da sie sich selbts aufruft.
                    await dataReader(socket);                   
                }
            }
            catch (Exception ex)
            {
                // Error Delegate
                if (OnError != null)
                    OnError(ex.Message);
            }
        }
        
        // Close Connection, Ressourcen freigeben
        public void Close()
        {
            try
            {
                writer.DetachStream();
                writer.Dispose();               
            }
            catch (Exception ex)
            {
            }
            this.isConnected = false;
            socket.Dispose();
        }


        // GET & SET
        public string Ip { get { return this.ip; } set {this.ip = value; } }
        public int Port { get { return this.port; } set { this.port = value; } }
        public bool IsConnected { get { return this.isConnected; } }
        // Get Daten löscht auch daten, also nur einmal verfügbar
        public byte[] getCommandDaten { get { byte[] tmp = this.commandData; this.commandData = new byte[0]; return tmp; } }
        public byte[] getCommandHeader { get { byte[] tmp = this.commandHeader; this.commandHeader = new byte[0]; return tmp; } }


        // Reader für alle daten vom Server, Request und ohne Request
        // Funktion ruft sich immer wieder selbst auf
        // Es werden zwei "Reader" verwendet, immer "USING" verwenden sonst zerstört GC die Objekte
        private async Task dataReader(StreamSocket socket)
        {
            // Zählt die Daten, wenn 0 dann keine Connection
            uint headerCount = 32999;
            uint dataCount = 32999;

            // Array auf Null setzten
            byte[] header = new byte[0];
            byte[] daten = new byte[0];

            try
            {
                // new Data Reader vom Socket
                using (DataReader reader = new DataReader(socket.InputStream))
                {
                    // Options, Teilstream und LittleEndian
                    //reader.InputStreamOptions = InputStreamOptions.Partial;   // Partial geht nicht, warten bis alle angeforderten Bytes vorhanden sind!
                    reader.ByteOrder = ByteOrder.LittleEndian;

                    // Warte auf Header, 3 bytes
                    headerCount = await reader.LoadAsync(3);

                    // Read Header wenn 3 bytes vorhanden
                    byte cmd = reader.ReadByte();
                    byte lenL = reader.ReadByte();
                    byte lenH = reader.ReadByte();
                    int len = lenH << 8 | lenL;             // Länge der Antwort vom header

                    if (cmd == 21)
                    {
                        // Ausnahme der Länge für Send PIN Command 21
                        // TODO Testen: http://download.metec-ag.de/MVBD/mvbd-command-21.png
                        len = lenH << 8 | lenL;
                    }

                    // Header local im Array speichern
                    header = new byte[] { cmd, lenL, lenH };

                    // Warte auf Daten
                    daten = new byte[len];

                    // Reader nach dem Lesen immer zerstören !!!
                    reader.DetachStream();
                }

                // Zweiter Reader für die Daten, mit "USING"
                using (DataReader readerTow = new DataReader(socket.InputStream))
                {
                    // Warte auf alle Daten
                    dataCount = await readerTow.LoadAsync((uint)daten.Length);
                    
                    // Daten speichern
                    readerTow.ReadBytes(daten);

                    // Reader nach dem Lesen immer zerstören !!!
                    readerTow.DetachStream();
                }
            }
            catch (Exception ex)
            {
                var mess = ex.Message;
                if (headerCount == 0 || dataCount == 0)
                {
                    mess = "Disconnected! " + mess;
                }
                if (OnError != null)
                    OnError(mess);

                // TODO: gehe zur StartSeite!

                return; // break Funktion
            }

            // Auswertung bzw. weitergeben der Daten 
            if (header.Count() > 0 && daten.Count() > 0)
            {
                // TODO: Spezifizieren
                //if (OnDataRecived != null)
                //{
                //    OnDataRecived(daten);
                //}
                readerControll(header, daten);
            }

            // Funktion neu aufrufen - warte auf neue Nachrichten
            await dataReader(socket);            
        }
        

        // Steuerung für die Auswertung der empfangenen Daten
        private void readerControll(byte[] header, byte[] daten)
        {
            // Prüfen ob gezielte anfrage
            if (this.waitForData == true)
            {
                // Prüfe Anwort Header und Command Send
                if (header[0] == this.commandSend)
                {
                    // Commdo Send Vars wieder auf null setzten
                    this.waitForData = false;
                    this.commandSend = 0;

                    // Daten Global speichern
                    this.commandData = daten;
                    this.commandHeader = header;

                    // Daten zurückgeben ohne Header und weitere auswertung
                    if (OnDataRecivedCmd != null)
                    {
                        OnDataRecivedCmd(daten);
                    }
                }
                else
                {
                    // Daten stimmen nicht überein, verwerfen und daten zurück setzten
                    int ddd = 4;

                }
            }
            else
            {
                // Wenn keine gezielte Anfrage

                //sendPinData = 21,
                //sendKeyDown = 22,
                //sendKeyUp = 23,
                //sendFinger = 24
                

                if (header[0] == 21)
                {
                    Debug.Write("<-- 21 GET PIN DATEN");
                    if (OnDataRecivedPin != null)
                    {
                        OnDataRecivedPin(daten);
                    }
                }
                if (header[0] == 22)
                {
                    Debug.Write("<-- 22 Send KeyDown");
                    if (OnDataRecivedKeyDown != null)
                    {
                        OnDataRecivedKeyDown(daten);
                    }
                }
                if (header[0] == 23)
                {
                    Debug.Write("<-- 23 Send KeyUp");
                    if (OnDataRecivedKeyUp != null)
                    {
                        OnDataRecivedKeyUp(daten);
                    }
                }
                if (header[0] == 24)
                {
                    if (OnDataRecivedFinger != null)
                    {
                        OnDataRecivedFinger(daten);
                    }
                }
            }
        }


        // Send
        public async void SendFinger(uint finger, bool contact, Point dots, Point pointer)
        {
            // Commando Daten
            byte[] send = new byte[0];

            // keine Antwort erwartet
            waitForData = false;
                        
            // Daten setzten
            byte[] data = new byte[8];
            data[0] = (byte)(finger-1);                       // number of the finger (0-9)
            data[1] = (byte)(Convert.ToInt32(contact));     // 1 = the finger has contact, 0 = the finger leaves
            data[2] = (byte)(dots.X);                       // X - position of the finger in pin units(Hyperflat: 0 - 75)
            data[3] = (byte)(dots.Y);                       // Y-position of the finger in pin units (Hyperflat: 0-47)
            data[4] = (byte)( (int)pointer.X >> 0 );        // X low byte
            data[5] = (byte)( (int)pointer.X >> 8 );        // X high Byte
            data[6] = (byte)( (int)pointer.Y >> 0 );        // Y low byte
            data[7] = (byte)( (int)pointer.Y >> 8 );        // Y high Byte
            
            // Header erstellen
            send = new byte[3 + data.Length];
            send[0] = 24;
            send[1] = (byte)(data.Length >> 0);
            send[2] = (byte)(data.Length >> 8);

            // Send zusammensetzten
            Array.Copy(data, 0, send, 3, data.Length);

            // alte Daten löschen, somit nur daten für diesen send
            this.commandHeader = new byte[0];
            this.commandData = new byte[0];

            // send data
            try
            {
                writer.WriteBytes(send);

                // Warten bis alles gesendet ist
                await writer.StoreAsync();
                await writer.FlushAsync();
            }
            catch (Exception ex)
            {
                // Wait zurück setzten
                waitForData = false;
                if (OnError != null)
                    OnError(ex.Message);
            }
        }

        public void Send(Commands com)
        {
            this.Send(com, 0);
        }
                
        public async void Send(Commands com, int value)
        {
            // enum umwandeln in zahl, global speichern für die auswertung der read fkt.
            this.commandSend = (byte)com;

            // Commando Daten
            byte[] send = new byte[0];
            
            
            // ================================================
            // Command 20 - Resolution and device information
            // ================================================
            if (this.commandSend == 20)
            {
                // anwort auf comando wird erwartet, damit der reader nicht an geht
                waitForData = true;

                // Bytes für das Commando
                send = new byte[] { 20, 0, 0 };
            }

            // ================================================
            // Command 26 - Verfügbare Device anfragen
            // ================================================
            if (this.commandSend == 26)
            {
                // anwort auf comando wird erwartet, damit der reader nicht an geht
                waitForData = true;
                
                // Bytes für das Commando
                send = new byte[] { 26, 0, 0 };
            }

            // ================================================
            // Command 34 - MVBD Version anfragen
            // ================================================
            if (this.commandSend == 34)
            {
                // anwort auf comando wird erwartet, damit der reader nicht an geht
                waitForData = true;

                // Bytes für das Commando
                send = new byte[] { 34, 0, 0 };
            }

            // ================================================
            // Command 52 - DeviceGraphic anfragen, incl Buttons, Positionen, Index
            // ================================================
            if (this.commandSend == 52)
            {
                // anwort auf comando wird erwartet, damit der reader nicht an geht
                waitForData = true;

                // Bytes für das Commando
                send = new byte[] { 52, 0, 0 };
            }

            // ================================================
            // Command 21 - Set Pin Data setzten
            // ================================================



            // ================================================
            // Command 22 - Set Key Down Data setzten
            // ================================================
            if (this.commandSend == 22)
            {
                // keine Antwort erwartet
                waitForData = false;
                
                // Bytes für das Commando
                send = new byte[] { 22, 1, 0, (byte)value };
            }


            // ================================================
            // Command 23 - Set Key UP Data setzten
            // ================================================
            if (this.commandSend == 23)
            {
                // keine Antwort erwartet
                waitForData = false;

                // Bytes für das Commando
                send = new byte[] { 23, 1, 0, (byte)value };
            }


            // ================================================
            // Command 24 - Set Finger Data setzten
            // ================================================




            // ================================================
            // Command 27 - Device setzten
            // ================================================
            if (this.commandSend == 27)
            {
                // keine Antwort erwartet
                waitForData = false;

                // value muss die id enthalten
                try
                {
                    // Daten setzten
                    byte[] data = new byte[4];
                    data[0] = (byte)(value >> 0);   // low byte
                    data[1] = (byte)(value >> 8);
                    data[2] = (byte)(value >> 16);
                    data[3] = (byte)(value >> 24);

                    // Header erstellen
                    send = new byte[3 + data.Length];
                    send[0] = 27;
                    send[1] = (byte)(data.Length >> 0);
                    send[2] = (byte)(data.Length >> 8);

                    // Send zusammensetzten
                    Array.Copy(data, 0, send, 3, 4);
                }
                catch (Exception ex)
                {
                    if (OnError != null)
                    {
                        string mess = "Command 27 Error: " + ex.Message;
                        OnError(mess);
                    }
                }
            }

            // ================================================
            // Command 29 - Device Type senden, MVBD, GRANT, usw. von ID 0-16
            // ================================================
            if (this.commandSend == 29)
            {
                // keine Antwort erwartet
                waitForData = false;

                // Bytes für das Commando
                send = new byte[] { 29, 1, 0, (byte)value };
            }



            // alte Daten löschen, somit nur daten für diesen send
            this.commandHeader = new byte[0];
            this.commandData = new byte[0];
            
            // send data
            try
            {            
                writer.WriteBytes(send);
                
                // Warten bis alles gesendet ist
                await writer.StoreAsync();
                await writer.FlushAsync();
            }
            catch (Exception ex)
            {
                // Wait zurück setzten
                waitForData = false;
                if (OnError != null)
                    OnError(ex.Message);
            }
        }



    }
}
