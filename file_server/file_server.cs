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

			byte[] buffer = new byte[1000];

            while(true)
			{
				int len = 0;

				len = _transport.receive(ref buffer);
				string filePath = Encoding.Default.GetString(buffer, 0, len);
                
				long fileSize = LIB.check_File_Exists(filePath);
				Console.WriteLine($"Received file path: {filePath}");

				buffer = BitConverter.GetBytes(fileSize);
                _transport.send(buffer, buffer.Length);
                
                if (fileSize == 0)
                {
                    Console.WriteLine("Could not find file.");
                }
                else
                {
					Console.WriteLine($"Filesize: {fileSize} bytes");



					sendFile(filePath, fileSize, _transport);
                }

				//len = _transport.receive(ref receiveBuffer);
				//long fileSize = BitConverter.ToInt64(receiveBuffer, 0);


			}

			/*byte[] buf = new byte[BUFSIZE];

			buf[0] = Convert.ToByte('K');
			buf[1] = Convert.ToByte('A');
			buf[2] = Convert.ToByte('T');
			buf[3] = Convert.ToByte('B');
			buf[4] = Convert.ToByte('B');
			buf[5] = Convert.ToByte('A');
			buf[6] = Convert.ToByte('K');

			Console.ReadKey();

			_transport.send(buf, 7);*/
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