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
			Transport _transport = new Transport(BUFSIZE, APP);   
            receiveFile(args[0], new Transport(BUFSIZE, APP)); 
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

			buffer = new byte[BUFSIZE];

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

				Console.WriteLine("Downloading...");
                
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

				Console.WriteLine("File downloaded.");
            }
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