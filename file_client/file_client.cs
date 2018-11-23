using System;
using System.IO;
using System.Text;
using Transportlaget;
using Library;

namespace Application
{
	class file_client
	{
		/// <summary>
		/// The BUFSIZE.
		/// </summary>
		private const int BUFSIZE = 1000;
		private const string APP = "FILE_CLIENT";

		/// <summary>
		/// Initializes a new instance of the <see cref="file_client"/> class.
		/// 
		/// file_client metoden opretter en peer-to-peer forbindelse
		/// Sender en forspÃ¸rgsel for en bestemt fil om denne findes pÃ¥ serveren
		/// Modtager filen hvis denne findes eller en besked om at den ikke findes (jvf. protokol beskrivelse)
		/// Lukker alle streams og den modtagede fil
		/// Udskriver en fejl-meddelelse hvis ikke antal argumenter er rigtige
		/// </summary>
		/// <param name='args'>
		/// Filnavn med evtuelle sti.
		/// </param>
	    private file_client(String[] args)
	    {
			Console.ReadKey();

            Transport _transport = new Transport(BUFSIZE, APP);
            //byte[] buffer = new byte[BUFSIZE];

            string filePath = "/root/original_boxfish.jpg/";

            while (true)
            {
                receiveFile(filePath, new Transport(BUFSIZE, APP));
            }


            /*
            long fileSize = 40000088;
            byte[] fileSizeByteAr = new byte[1000];

            //fileSizeByteAr = BitConverter.GetBytes(fileSize);

            fileSizeByteAr[0] = 88;
            fileSizeByteAr[1] = 90;
            fileSizeByteAr[2] = 98;
            fileSizeByteAr[3] = 2;

            long newFileSize = BitConverter.ToInt64(fileSizeByteAr, 0);*/
	    }

		/// <summary>
		/// Receives the file.
		/// </summary>
		/// <param name='fileName'>
		/// File name.
		/// </param>
		/// <param name='transport'>
		/// Transportlaget
		/// </param>
		private void receiveFile (String filePath, Transport transport)
		{
			byte[] buffer = new byte[BUFSIZE];

            buffer = Encoding.ASCII.GetBytes(filePath);
            transport.send(buffer, buffer.Length);

            transport.receive(ref buffer);
            long fileSize = BitConverter.ToInt64(buffer, 0);

            if (fileSize == 0)
            {
                Console.WriteLine("Could not find file, please try again.");
            }
            else
            {
                Console.WriteLine($"FileSize: {fileSize.ToString()} bytes.");
                Console.WriteLine("Choose where to save the file, name and filetype.");

                FileStream fs = File.Create(Console.ReadLine());

                int count = 1000, lastRead = 1;

                while (lastRead >= 0)
                {
                    transport.receive(ref buffer);
                    fs.Write(buffer, 0, count);

                    fileSize -= 1000;

                    if (fileSize < 1000)
                    {
                        count = (int)fileSize;
                        --lastRead;
                    }
                }
                fs.Close();
            }

            //FileStream fs = File.Create(fileName);

            /*byte[] buf = new byte[BUFSIZE];

            transport.receive(ref buf);

            Console.WriteLine($"{System.Text.Encoding.Default.GetString(buf)}");
            Console.ReadKey();*/
		}

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name='args'>
		/// First argument: Filname
		/// </param>
		public static void Main (string[] args)
		{
			file_client _file_client = new file_client(args);
		}
	}
}