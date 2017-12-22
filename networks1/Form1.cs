using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms;
using PcapDotNet.Analysis;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Base;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using PcapDotNet.Packets.Http;
using PcapDotNet.Packets.IpV6;

namespace networks1
{
    public partial class Interface : Form
    {

        private IList<LivePacketDevice> AdaptersList;
        private PacketDevice selectedAdapter;
        private bool first_time = true; //boolean variable needed on re-capturing
        public static byte[] payload;
        public static Dictionary<int, Packet1> packets = new Dictionary<int, Packet1>();


        public Interface()
        {
            InitializeComponent();
            this.BackgroundImage = Properties.Resources.eee;


            try
            {
                AdaptersList = LivePacketDevice.AllLocalMachine;//locate all adapters
            }
            catch (Exception e)
            {
                MessageBox.Show("Run as Administrator");
            }

            PcapDotNetAnalysis.OptIn = true;//enable pcap analysis

            if (AdaptersList.Count == 0)
            {

                MessageBox.Show("No adapters found !!");

                return;

            }

            for (int i = 0; i != AdaptersList.Count; ++i)//add all adapters to my Combobox
            {
                LivePacketDevice Adapter = AdaptersList[i];

                if (Adapter.Description != null)

                    list.Items.Add(Adapter.Description);
                else
                    list.Items.Add("Unknown");
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (!first_time)//we need to re-capturing so we need to restart the program
            {
                Application.Restart();
            }

            else if (list.SelectedIndex >= 0)//if an adapter selected
            {
                timer1.Enabled = true;//start updating listview and textbox to show info
                selectedAdapter = AdaptersList[list.SelectedIndex];//get selected adapter from combobox
                backgroundWorker1.RunWorkerAsync();//start capturing and making filters
                backgroundWorker2.RunWorkerAsync();//start saving .pcap file if needed
                start.Enabled = false;
                stop.Enabled = true;
                list.Enabled = false;
                _udp.Enabled = false;
                _tcp.Enabled = false;
                first_time = false;
                save.Enabled = false;
            }
            else
            {
                MessageBox.Show("Please select an adapter!");
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            start.Enabled = true;
            stop.Enabled = false;
            list.Enabled = true;
            _udp.Enabled = true;
            _tcp.Enabled = true;
            timer1.Enabled = false;
            save.Enabled = true;
        }


        //variabels needed to get info from packet
        string count = "";
        string time = "";
        string source = "";
        string destination = "";
        string protocol = "";
        string length = "";

        string tcpack = "";
        string tcpsec = "";
        string tcpnsec = "";
        string tcpsrc = "";
        string tcpdes = "";
        string tcpheader = ""; 
     
        string udpscr = "";
        string udpdes = "";

        string httpheader = "";
        string httpbody = "";
        string httpver = "";
        string httplen = "";

        string reqres = "";
        //variables needed when saving files
        string folderName = "";
        string pathString = "";
        private string tcppheader;

        private void PacketHandler(Packet packet)
        {
            this.count = "";
            this.time = "";
            this.source = "";
            this.destination = "";
            this.protocol = "";
            this.length = "";

            this.tcpack = "";
            this.tcpsec = "";
            this.tcpnsec = "";
            this.tcpsrc = "";
            this.tcpdes = "";
            this.tcpheader = "";

            this.udpscr = "";
            this.udpdes = "";

           
            IpV4Datagram ip = packet.Ethernet.IpV4;
            TcpDatagram tcp = ip.Tcp;
            UdpDatagram udp = ip.Udp;
            HttpDatagram TcpPacket = null;
          

            if (ip.Protocol.ToString().Equals("Tcp"))
            {
                TcpPacket = tcp.Http;//Initialize http variable only if the packet was tcp

                if ((TcpPacket.Header != null) && (!_tcp.Checked))
                {
                    protocol = "tcp";
                    tcppheader = TcpPacket.Header.ToString();
                    count = packet.Count.ToString();
                    time = packet.Timestamp.ToString();
                    this.source = ip.Source.ToString();
                    this.destination = ip.Destination.ToString();
                    length = ip.Length.ToString();
                    httpver = TcpPacket.Version.ToString();
                    httplen = TcpPacket.Length.ToString();
                    httpbody = TcpPacket.Body.ToString();

                    if (TcpPacket.IsRequest)
                    {
                        reqres = "Request";
                    }
                    else
                    {
                        reqres = "Response";
                    }
                }

                else
                {

                    count = packet.Count.ToString();
                    time = packet.Timestamp.ToString();
                    this.source = ip.Source.ToString();
                    this.destination = ip.Destination.ToString();
                    length = ip.Length.ToString();
                    protocol = ip.Protocol.ToString();

                    tcpsrc = tcp.SourcePort.ToString();
                    tcpdes = tcp.DestinationPort.ToString();
                    tcpack = tcp.AcknowledgmentNumber.ToString();
                    tcpsec = tcp.SequenceNumber.ToString();
                    tcpnsec = tcp.NextSequenceNumber.ToString();
                }


            }
            else
            {
                if ((ip.Protocol.ToString().Equals("Udp")))
                {
                    count = packet.Count.ToString();
                    time = packet.Timestamp.ToString();
                    this.source = ip.Source.ToString();
                    this.destination = ip.Destination.ToString();
                    length = ip.Length.ToString();
                    protocol = ip.Protocol.ToString();
                    udpscr = udp.SourcePort.ToString();
                    udpdes = udp.DestinationPort.ToString();
                }
                else
                {

                    count = packet.Count.ToString();
                    time = packet.Timestamp.ToString();
                    this.source = ip.Source.ToString();
                    this.destination = ip.Destination.ToString();
                    length = ip.Length.ToString();
                    protocol = ip.Protocol.ToString();

                }
            }


            if (ip.Protocol.ToString().Equals("Tcp") && (save.Checked))
            {
                int _source = tcp.SourcePort;
                int _destination = tcp.DestinationPort;

                if (tcp.PayloadLength != 0) //not syn or ack
                {
                    payload = new byte[tcp.PayloadLength];
                    tcp.Payload.ToMemoryStream().Read(payload, 0, tcp.PayloadLength);// read payload from 0 to length
                    if (_destination == 80)// request from server
                    {
                        Packet1 packet1 = new Packet1();
                        int i = Array.IndexOf(payload, (byte)32, 6);
                        byte[] t = new byte[i - 5];
                        Array.Copy(payload, 5, t, 0, i - 5);
                        packet1.Name = System.Text.ASCIIEncoding.ASCII.GetString(t);

                        if (!packets.ContainsKey(_source))
                            packets.Add(_source, packet1);
                    }
                    else
                        if (_source == 80)
                        if (packets.ContainsKey(_destination))
                        {
                            Packet1 packet1 = packets[_destination];
                            if (packet1.Data == null)
                            {
                                if ((TcpPacket.Header != null) && (TcpPacket.Header.ContentLength != null))
                                {
                                    packet1.Data = new byte[(uint)TcpPacket.Header.ContentLength.ContentLength];
                                    Array.Copy(TcpPacket.Body.ToMemoryStream().ToArray(), packet1.Data, TcpPacket.Body.Length);
                                    packet1.Order = (uint)(tcp.SequenceNumber + payload.Length - TcpPacket.Body.Length);
                                    packet1.Data_Length = TcpPacket.Body.Length;
                                    for (int i = 0; i < packet1.TempPackets.Count; i++)
                                    {
                                        Temp tempPacket = packet1.TempPackets[i];
                                        Array.Copy(tempPacket.data, 0, packet1.Data, tempPacket.tempSeqNo - packet1.Order, tempPacket.data.Length);
                                        packet1.Data_Length += tempPacket.data.Length;
                                    }
                                }
                                else
                                {
                                    Temp tempPacket = new Temp();
                                    tempPacket.tempSeqNo = (uint)tcp.SequenceNumber;
                                    tempPacket.data = new byte[payload.Length];
                                    Array.Copy(payload, tempPacket.data, payload.Length);
                                    packet1.TempPackets.Add(tempPacket);
                                }
                            }
                            else if (packet1.Data_Length != packet1.Data.Length)
                            {
                                Array.Copy(payload, 0, packet1.Data, tcp.SequenceNumber - packet1.Order, payload.Length);

                                packet1.Data_Length += payload.Length;
                            }

                            if (packet1.Data != null)
                                if (packet1.Data_Length == packet1.Data.Length)
                                {

                                    using (BinaryWriter writer = new BinaryWriter(File.Open(@"D:\captured\" + Directory.CreateDirectory(Path.GetFileName(packet1.Name)), FileMode.Create)))
                                    {
                                        writer.Write(packet1.Data);

                                    }

                                    packets.Remove(_destination);

                                }
                        }
                }
            }


        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!count.Equals(""))
            {
                ListViewItem item = new ListViewItem(time);
                item.SubItems.Add(source);
                item.SubItems.Add(destination);
                item.SubItems.Add(protocol);

                if (protocol.Equals("Tcp"))
                {
                    textBox1.Text = "Protocol :  Tcp  \r\n SourcePort :  " + tcpsrc + "\r\n DestinationPort :  " + tcpdes + "\r\n SequenceNumber :  " + tcpsec + "\r\n NextSequenceNuber :  " + tcpnsec + "\r\n AcknowladgmentNumber :  " + tcpack;
                    if (save.Checked)
                    {
                        using (StreamWriter writer = new StreamWriter(@"D:\Capture\TcpPacketsInfo.txt", true))
                        {
                            writer.Write("Protocol :  Tcp  \r\n SourcePort :  " + tcpsrc + "\r\n DestinationPort :  " + tcpdes + "\r\n SequenceNumber :  " + tcpsec + "\r\n NextSequenceNuber :  " + tcpnsec + "\r\n AcknowladgmentNumber :  " + tcpack + "\r\n --------------------------------------------- \r\n");
                        }
                    }
                }
                else
                {
                    if (protocol.Equals("Http"))
                    {
                        textBox1.Text = "Protocol :  Http  \r\n Version :  " + httpver + "\r\n Length :  " + httplen + "\r\n Type :  " + reqres + "\r\n Header :  \r\n" + httpheader + "\r\n Body :  \r\n" + httpbody;
                        if (save.Checked)
                        {
                            using (StreamWriter writer = new StreamWriter(@"C:\Users\Islam95\Desktop\networks1\save.txt", true))
                            {
                                writer.Write("Protocol :  Http  \r\n Version :  " + httpver + "\r\n Length :  " + httplen + "\r\n Type :  " + reqres + "\r\n Header :  \r\n" + httpheader + "\r\n --------------------------------------------- \r\n");
                            }
                        }
                    }
                    else
                    {
                        if (protocol.Equals("Udp"))
                        {

                            textBox1.Text = "Protocol :  Udp  \r\n SourcePort :  " + udpscr + "\r\n DestinationPort :  " + udpdes;
                            if (save.Checked)
                            {
                                using (StreamWriter writer = new StreamWriter(@"C:\Users\Islam95\Desktop\networks1\save.txt", true))
                                {
                                    writer.Write("Protocol :  Udp  \r\n SourcePort :  " + udpscr + "\r\n DestinationPort :  " + udpdes + "\r\n --------------------------------------------- \r\n");
                                }
                            }
                        }
                    }
                }

                item.SubItems.Add(length);
                listView1.Items.Insert(0, item);
            }

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            using (PacketCommunicator communicator = selectedAdapter.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000))
            {
                // Check the link layer.
                if (communicator.DataLink.Kind != DataLinkKind.Ethernet)
                {
                    MessageBox.Show("This program works only on Ethernet networks!");

                    return;
                }


                //Deallocation is  necessary
                if (_tcp.Checked && (!_udp.Checked))//just tcp
                {
                    using (BerkeleyPacketFilter filter = communicator.CreateFilter("tcp"))
                    {
                        // Set the filter
                        communicator.SetFilter(filter);
                    }
                }

                else if (_udp.Checked && !(_tcp.Checked))//just udp
                {
                    using (BerkeleyPacketFilter filter = communicator.CreateFilter("udp"))
                    {
                        // Set the filter
                        communicator.SetFilter(filter);
                    }
                }
               
                else if (_tcp.Checked && (_udp.Checked))//tcp and udp
                {
                    using (BerkeleyPacketFilter filter = communicator.CreateFilter("ip and udp"))
                    {
                        // Set the filter
                        communicator.SetFilter(filter);
                    }
                }
                // Begin the capture
                communicator.ReceivePackets(0, PacketHandler);
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            if (save.Checked)
            {
                using (PacketCommunicator communicator = selectedAdapter.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000))
                {
                    using (PacketDumpFile dumpFile = communicator.OpenDump(@"C:\Users\Islam95\Desktop\networks1\save.pcap"))
                    {
                        // start the capture
                        communicator.ReceivePackets(0, dumpFile.Dump);
                    }
                }
            }
        }

        private void udp_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void list_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
