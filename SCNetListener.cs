using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace SC
{
	public class SCNetListener
	{
        public delegate void ProtocolDelegate(FlatBuffers.IFlatbufferObject fbObject);
        Dictionary<Protocol, ProtocolDelegate> _protocolDelegates = new Dictionary<Protocol, ProtocolDelegate>();

        SCRingBuffer _recvBuffer = new SCRingBuffer(Consts.READ_BUFFER_MAX_SIZE);
        byte[] _rawAnalysisBuffer = new byte[Consts.PACKET_BUFFER_MAX_SIZE];

        public void RegisterProtocolDelegate(Protocol protocolType_, ProtocolDelegate delegate_)
		{
            if(_protocolDelegates.ContainsKey(protocolType_))
			{
				ProtocolDelegate temp = _protocolDelegates[protocolType_];
				temp += delegate_;
				_protocolDelegates[protocolType_] = temp;
			}
            else
			{
                _protocolDelegates.Add(protocolType_, delegate_);
            }
		}

		public void OnReceiveData(byte[] recvBuffer_, int length_)
        {
            _recvBuffer.Push(recvBuffer_, length_);
        }

        public void Excute()
        {
            int bufferSize = 0;
            Protocol protocolType = Protocol.NONE;

            while (_recvBuffer.Size() > Consts.PACKET_HEADER_SIZE)
            {
                Debug.LogWarning("--- _recvBuffer.Size : " + _recvBuffer.Size());

                lock (_recvBuffer)
                {
                    _recvBuffer.Read(_rawAnalysisBuffer, Consts.PACKET_HEADER_SIZE);
                }

                FlatBuffers.ByteBuffer analysisBuffer = new FlatBuffers.ByteBuffer(_rawAnalysisBuffer);

                //��Ŷ ��� �б�
                bufferSize = analysisBuffer.GetUshort(0);
                protocolType = (Protocol)analysisBuffer.GetUshort(sizeof(short));

                Debug.LogWarning("--- recv packet : " + protocolType + "|" + bufferSize);

                //��Ŷ ���۰� ��� ���ŵ��� �ʾ� �Ľ� ����
                if (bufferSize > _recvBuffer.Size())
                {
                    Debug.LogWarning("--- wait read full body : " + _recvBuffer.Size() + "/" + bufferSize);

                    break;
                }

                lock (_recvBuffer)
                {
                    _recvBuffer.Pop(_rawAnalysisBuffer, bufferSize);
                }

                if (_protocolDelegates.ContainsKey(protocolType))
                {
                    _protocolDelegates[protocolType](GetFlatBufferObject(protocolType, _rawAnalysisBuffer, Consts.PACKET_HEADER_SIZE));
                }
                else
                {
                    Debug.LogWarning("--- not register protocol : " + protocolType);
                }
            }
        }

        private static FlatBuffers.IFlatbufferObject GetFlatBufferObject(Protocol protocolType_, byte[] resBuffer_, int offset_)
        {
            switch (protocolType_)
            {
                //case Protocol.RES_LOGIN: return Res_Login.GetRootAsRes_Login(new FlatBuffers.ByteBuffer(resBuffer_, offset_));
            }

            return default;
        }
    }
}
