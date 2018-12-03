using System;
using Linklaget;

namespace Transportlaget
{
	/// <summary>
	/// Transport.
	/// </summary>
	public class Transport
	{
		/// <summary>
		/// The link.
		/// </summary>
		private Link link;
		/// <summary>
		/// The 1' complements checksum.
		/// </summary>
		private Checksum checksum;
		/// <summary>
		/// The buffer.
		/// </summary>
		private byte[] buffer;
		/// <summary>
		/// The seq no.
		/// </summary>
		private byte seqNo;
		/// <summary>
		/// The old_seq no.
		/// </summary>
		private byte old_seqNo;
		/// <summary>
		/// The error count.
		/// </summary>
		private int errorCount;
		/// <summary>
		/// The DEFAULT_SEQNO.
		/// </summary>
		private const int DEFAULT_SEQNO = 2;
		/// <summary>
		/// The data received. True = received data in receiveAck, False = not received data in receiveAck
		/// </summary>
		private bool dataReceived;
		/// <summary>
		/// The number of data the recveived.
		/// </summary>
		private int recvSize = 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="Transport"/> class.
		/// </summary>
		public Transport (int BUFSIZE, string APP)
		{
			link = new Link(BUFSIZE+(int)TransSize.ACKSIZE, APP);
			checksum = new Checksum();
			buffer = new byte[BUFSIZE+(int)TransSize.ACKSIZE];
			seqNo = 0;
			old_seqNo = DEFAULT_SEQNO;
			errorCount = 0;
			dataReceived = false;
		}

		/// <summary>
		/// Receives the ack.
		/// </summary>
		/// <returns>
		/// The ack.
		/// </returns>
		private bool receiveAck()
		{
			recvSize = link.receive(ref buffer);
			dataReceived = true;

			if (recvSize == (int)TransSize.ACKSIZE) {
				dataReceived = false;
				if (!checksum.checkChecksum (buffer, (int)TransSize.ACKSIZE) ||
				  buffer [(int)TransCHKSUM.SEQNO] != seqNo ||
				  buffer [(int)TransCHKSUM.TYPE] != (int)TransType.ACK)
				{
					return false;
				}
				seqNo = (byte)((buffer[(int)TransCHKSUM.SEQNO] + 1) % 2);
			}
 
			return true;
		}

		/// <summary>
		/// Sends the ack.
		/// </summary>
		/// <param name='ackType'>
		/// Ack type.
		/// </param>
		private void sendAck (bool ackType)
		{
			byte[] ackBuf = new byte[(int)TransSize.ACKSIZE];
			ackBuf [(int)TransCHKSUM.SEQNO] = (byte)
				(ackType ? (byte)buffer [(int)TransCHKSUM.SEQNO] : (byte)(buffer [(int)TransCHKSUM.SEQNO] + 1) % 2);
			ackBuf [(int)TransCHKSUM.TYPE] = (byte)(int)TransType.ACK;
			checksum.calcChecksum (ref ackBuf, (int)TransSize.ACKSIZE);
			link.send(ackBuf, (int)TransSize.ACKSIZE);
		}

		/// <summary>
		/// Send the specified buffer and size.
		/// </summary>
		/// <param name='buffer'>
		/// Buffer.
		/// </param>
		/// <param name='size'>
		/// Size.
		/// </param>
		public void send(byte[] buf, int size)
		{
			buffer[(int)TransCHKSUM.SEQNO] = seqNo;
			buffer[(int)TransCHKSUM.TYPE] = (int)TransType.DATA;

			int index = 0;
			for (int i = 4; i < size + 4; i++)
			{
				buffer[i] = buf[index];
				++index;
			}

			checksum.calcChecksum(ref buffer, size+4);         
            
            link.send(buffer, size+4);

			int _numberOfRetransmits = 0;
			while(!receiveAck() && _numberOfRetransmits < 4)
			{
				link.send(buffer, size+4);
				++_numberOfRetransmits;
                
				if (_numberOfRetransmits == 4)
                {
                    Console.WriteLine("Transmission failed.");
					return;
                }            
			}
		}

		/// <summary>
		/// Receive the specified buffer.
		/// </summary>
		/// <param name='buffer'>
		/// Buffer.
		/// </param>
		public int receive (ref byte[] buf)
		{
			bool _isSeqNoDifferent = false;
            bool _isCheckSumOk = false;
            int len = -1;

			int _numberOfTransmits = 0;
            do
            {
				++_numberOfTransmits;
				len = link.receive(ref buffer);
                _isCheckSumOk = checksum.checkChecksum(buffer, len);

                if (buffer[(int)TransCHKSUM.SEQNO] != old_seqNo)
                    _isSeqNoDifferent = true;

                if (!_isCheckSumOk || !_isSeqNoDifferent)
                    sendAck(false);
                else
                    sendAck(true);

            } while (!_isSeqNoDifferent && !_isCheckSumOk);

            old_seqNo = buffer[(int)TransCHKSUM.SEQNO];

            int index = 0;
            for (int i = 4; i < len; ++i)
            {
                buf[index] = buffer[i];
                ++index;
            }

            return len-4;
		}
	}
}