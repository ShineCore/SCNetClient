using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace SC
{
	public class SCNetClient
	{
		private TcpClient _tcpClient;
		private SCNetListener _listener;

		private NetworkStream _stream;
		private Thread _readStreamThread;

		private E_NET_STATE _netState;

		private byte[] tempReadBytes = new byte[Consts.READ_BUFFER_MAX_SIZE];

		public void Connect(string host_, int port_)
		{
			_listener = new SCNetListener();

			_netState = E_NET_STATE.CONNECTING;

			_tcpClient = new TcpClient()
			{
				NoDelay = true,
			};

			_tcpClient.BeginConnect(host_, port_, new AsyncCallback(ConnectCallback), _tcpClient);
		}

		public void Disconnect()
        {
			_netState = E_NET_STATE.DISCONNECTED;

			if (_stream != null)
				_stream.Close();
		}

		public void AddReceiveCallback(Protocol protocolType_, SCNetListener.ProtocolDelegate callback_)
        {
			_listener.RegisterProtocolDelegate(protocolType_, callback_);
		}

		private void ConnectCallback(IAsyncResult ar_)
		{
			try
			{
				TcpClient tcpClient = (TcpClient)ar_.AsyncState;
				tcpClient.EndConnect(ar_);

				_netState = E_NET_STATE.CONNECTED;

				Debug.Log("Connected server");

				SetTcpClient(tcpClient);
			}
			catch(Exception e)
			{
				_netState = E_NET_STATE.DISCONNECTED;

				SocketException socketError = e as SocketException;

				//立加 角菩
				if (socketError != null)
					Debug.Log("-- SocketException ErrorCode : " + socketError.ErrorCode);

				Debug.LogError("-- connect failed : " + e.Message);
			}
		}

		private void SetTcpClient(TcpClient tcpClient_)
		{
			_tcpClient = tcpClient_;

			if (_tcpClient.Connected)
			{
				_stream = _tcpClient.GetStream();

				Debug.Log("SetTcpClient CONNECTED");

				_readStreamThread = new Thread(ReadStream) {
					IsBackground = true
				};
				_readStreamThread.Start();
			}
			else
			{
				Debug.Log("SetTcpClient DISCONNECTED");

				_netState = E_NET_STATE.DISCONNECTED;
			}
		}

		private void ReadStream()
		{
			Debug.Log("---- Start ReadStream()");

			while(_netState == E_NET_STATE.CONNECTED)
			{
                try
                {
                    Array.Clear(tempReadBytes, 0, Consts.READ_BUFFER_MAX_SIZE);
                    int readByteSize = _stream.Read(tempReadBytes, 0, Consts.READ_BUFFER_MAX_SIZE);

                    Debug.Log("-- stream read size : " + readByteSize);

					if (readByteSize > 0)
					{
						_listener.OnReceiveData(tempReadBytes, readByteSize);
                    }
                    else
                    {
						break;
					}
                }
                catch (Exception e)
                {
					//立加 角菩
					if (e is SocketException socketError)
						Debug.Log("-- SocketException ErrorCode : " + socketError.ErrorCode);

					Debug.LogError("-- stream read failed : " + e.ToString());

					break;
                }
            }

			_tcpClient.Close();
			_netState = E_NET_STATE.DISCONNECTED;
		}

		public void SendBytes(byte[] buffer_, int length_)
		{
			if (_netState != E_NET_STATE.CONNECTED)
				return;

			try
			{
				_stream.Write(buffer_, 0, length_);
				_stream.Flush();
			}
			catch(Exception e)
			{
                SocketException socketError = e as SocketException;

				//立加 角菩
				if (socketError != null)
					Debug.Log("-- SocketException ErrorCode : " + socketError.ErrorCode);

				Debug.Log("-- SendBytes Falied");
			}
		}

		public void Excute()
		{
			_listener?.Excute();
		}
	}
}
