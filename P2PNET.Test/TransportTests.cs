﻿using System;
using NUnit.Framework;
using P2PNET.TransportLayer;
using System.Net;
using System.Threading.Tasks;
using P2PNET.TransportLayer.EventArgs;
using System.Text;

namespace P2PNET.Test
{
    [TestFixture]
    public class ConnectionTests
    {
        private TransportManager transMgr;
        TransportManager transManger2;

        public ConnectionTests()
        {
            transMgr = new TransportManager(8080, true);
            transManger2 = new TransportManager(8100, true);

            transMgr.StartAsync().Wait();
        }

        [Test]
        public async Task EstablishConnection()
        {
            bool peerChange = false;
            transMgr.PeerChange += (object obj, PeerChangeEventArgs e) =>
            {
                peerChange = true;
            };

            string ipAddress = IPAddress.Loopback.ToString();

            await transMgr.DirrectConnectAsyncTCP(ipAddress);

            int peerCount = transMgr.KnownPeers.Count;

            Assert.IsTrue(peerCount > 0);
            Assert.IsTrue(peerChange == true);
        }

        [Test]
        public async Task MultipleSockets()
        {
            bool msgReceived = false;

            transManger2.PeerChange += (object obj, PeerChangeEventArgs e) => {
                msgReceived = true;
            };

            transManger2.MsgReceived += (object obj, MsgReceivedEventArgs e) => {
                msgReceived = true;
            };

            transManger2.StartAsync().Wait();


            string ipAddress = IPAddress.Loopback.ToString();

            await transMgr.DirrectConnectAsyncTCP(ipAddress);

            Assert.IsTrue(msgReceived == false);
        }

        
        [Test]
        public async Task SendTCPMsg()
        {
            byte[] reciMsg = null;
            transMgr.MsgReceived += (object obj, MsgReceivedEventArgs e) =>
            {
                reciMsg = e.Message;
            };

            string ipAddress = IPAddress.Loopback.ToString();


            byte[] SendMsg = new byte[] { 255, 0, 153, 00 };
            await transMgr.SendAsyncTCP(ipAddress, SendMsg);
            System.Threading.Thread.Sleep(100);
            Console.WriteLine(BinToString(reciMsg));

            Assert.IsTrue(reciMsg != null);
        }
        

        [Test]
        public async Task KnownPeerTest()
        {
            byte[] reciMsg = null;
            transMgr.MsgReceived += (object obj, MsgReceivedEventArgs e) =>
            {
                reciMsg = e.Message;
            };

            string ipAddress = IPAddress.Loopback.ToString();

            //make sure the local peer is known
            await transMgr.DirrectConnectAsyncTCP(ipAddress);

            byte[] SendMsg = new byte[] { 255, 0, 153, 00 };
            await transMgr.SendToAllPeersAsyncUDP(SendMsg);
            System.Threading.Thread.Sleep(100);
            Console.WriteLine(BinToString(reciMsg));

            Assert.IsTrue(reciMsg != null);
        }

        [Test]
        public async Task GetIpAddress()
        {
            string localIp = await transMgr.GetIpAddress();
            Console.WriteLine("local ip address = " + localIp);
            Assert.IsTrue(localIp != null);
        }


        [Test]
        public async Task SendNullTCPMsg()
        {
            bool msgReceived = false;
            transMgr.MsgReceived += (object obj, MsgReceivedEventArgs e) =>
            {
                msgReceived = true;
            };

            string ipAddress = IPAddress.Loopback.ToString();

            byte[] SendMsg = null;
            try
            {
                await transMgr.SendAsyncTCP(ipAddress, SendMsg);
            }
            catch
            {
                
            }

            Assert.IsTrue(msgReceived == false);
        }

        public async Task SendBlankTCPMsg()
        {
            bool msgReceived = false;
            transMgr.MsgReceived += (object obj, MsgReceivedEventArgs e) =>
            {
                msgReceived = true;
            };

            string ipAddress = IPAddress.Loopback.ToString();

            byte[] SendMsg = new byte[]{ };
            try
            {
                await transMgr.SendAsyncTCP(ipAddress, SendMsg);
            }
            catch
            {

            }

            Assert.IsTrue(msgReceived == false);
        }

        public string BinToString(byte[] msg)
        {
            if(msg == null)
            {
                return "no message";
            }
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < msg.Length; i++ )
            {
                sb.Append(msg[i]);
            }
            return sb.ToString();
        }
    }
}