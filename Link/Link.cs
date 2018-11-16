using System;
using System.IO.Ports;

namespace Linklaget
{
	/// <summary>
	/// Link.
	/// </summary>
	public class Link
	{
		/// <summary>
		/// The DELIMITE for slip protocol.
		/// </summary>
		const byte DELIMITER = (byte)'A';
		/// <summary>
		/// The buffer for link.
		/// </summary>
		private byte[] buffer;
		/// <summary>
		/// The serial port.
		/// </summary>
		SerialPort serialPort;

		/// <summary>
		/// Initializes a new instance of the <see cref="link"/> class.
		/// </summary>
		public Link (int BUFSIZE, string APP)
		{
			// Create a new SerialPort object with default settings.
			#if DEBUG
				if(APP.Equals("FILE_SERVER"))
				{
					serialPort = new SerialPort("/dev/ttySn0",115200,Parity.None,8,StopBits.One);
				}
				else
				{
					serialPort = new SerialPort("/dev/ttySn1",115200,Parity.None,8,StopBits.One);
				}
			#else
				serialPort = new SerialPort("/dev/ttyS1",115200,Parity.None,8,StopBits.One);
			#endif
			if(!serialPort.IsOpen)
				serialPort.Open();

			buffer = new byte[(BUFSIZE*2)];

			// Uncomment the next line to use timeout
			//serialPort.ReadTimeout = 500;

			serialPort.DiscardInBuffer ();
			serialPort.DiscardOutBuffer ();
		}

		/// <summary>
		/// Send the specified buf and size.
		/// </summary>
		/// <param name='buf'>
		/// Buffer.
		/// </param>
		/// <param name='size'>
		/// Size.
		/// </param>
		public void send (byte[] buf, int size)
		{
			int bufferIndex = 0;
			buffer[bufferIndex] = Convert.ToByte('A');
			++bufferIndex;
            
			for (int i = 0; i < size; ++i)
			{
				if(buf[i] == 'A')
				{
					buffer[bufferIndex] = Convert.ToByte('B');
					++bufferIndex;
					buffer[bufferIndex] = Convert.ToByte('C');
					++bufferIndex;
				}
				else if (buf[i] == 'B')
                {
					buffer[bufferIndex] = Convert.ToByte('B');
					++bufferIndex;
					buffer[bufferIndex] = Convert.ToByte('D');
					++bufferIndex;
                }
				else
				{
					buffer[bufferIndex] = buf[i];
					++bufferIndex;
				}
			}

			buffer[bufferIndex] = Convert.ToByte('A');
			++bufferIndex;

			serialPort.Write(buffer, 0, bufferIndex);
		}

		/// <summary>
		/// Receive the specified buf and size.
		/// </summary>
		/// <param name='buf'>
		/// Buffer.
		/// </param>
		/// <param name='size'>
		/// Size.
		/// </param>
		public int receive (ref byte[] buf)
		{
			int readByte = 0, readSecondByte = 0, i = 0;

			while(readByte != (int)Convert.ToByte('A'))
			{
				readByte = serialPort.ReadByte();
			}

			readByte = 0;

			while(readByte != (int)Convert.ToByte('A'))
			{
				readByte = serialPort.ReadByte();

				if(readByte == (int)Convert.ToByte('B'))
				{
					readSecondByte = serialPort.ReadByte();

					if(readSecondByte == (int)Convert.ToByte('C'))
					{
						buf[i] = Convert.ToByte('A');
					}
					else if (readSecondByte == (int)Convert.ToByte('D'))
                    {
                        buf[i] = Convert.ToByte('B');
                    }
				}
				else if (readByte != (int)Convert.ToByte('A'))
				{
					buf[i] = Convert.ToByte(readByte);
				}

				++i;
			}

			return i-1;
		}
	}
}
