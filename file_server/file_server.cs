using System;
using System.IO;
using System.Text;
using Transportlaget;
using Library;

namespace Application
{
	class file_server
	{
		/// <summary>
		/// The BUFSIZE
		/// </summary>
		private const int BUFSIZE = 1000;
		private const string APP = "FILE_SERVER";

		/// <summary>
		/// Initializes a new instance of the <see cref="file_server"/> class.
		/// </summary>
		private file_server ()
		{
			Transport _transport = new Transport(BUFSIZE, APP);
			byte[] buf = new byte[BUFSIZE];

			buf[0] = Convert.ToByte('K');
			buf[0] = Convert.ToByte('A');
			buf[0] = Convert.ToByte('T');
			buf[0] = Convert.ToByte('B');
			buf[0] = Convert.ToByte('B');
			buf[0] = Convert.ToByte('A');
			buf[0] = Convert.ToByte('K');

			_transport.send(buf, 7);
		}

		/// <summary>
		/// Sends the file.
		/// </summary>
		/// <param name='fileName'>
		/// File name.
		/// </param>
		/// <param name='fileSize'>
		/// File size.
		/// </param>
		/// <param name='tl'>
		/// Tl.
		/// </param>
		private void sendFile(String fileName, long fileSize, Transport transport)
		{
			FileStream fs = new FileStream(fileName, FileMode.Open);

			byte[] fileBuf = new byte[BUFSIZE];

			int offset = 0;
			int size = BUFSIZE;

            while(offset < fileSize)
			{
				fs.Read(fileBuf, 0, size);
                
				transport.send(fileBuf, size);

				offset += BUFSIZE;

				if ((offset < fileSize) && (offset + BUFSIZE > fileSize))
                {
                    size = (int)fileSize - offset;
                }
			}
		}

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments.
		/// </param>
		public static void Main (string[] args)
		{
			file_server _file_server = new file_server();
		}
	}
}