using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;
using System.Net.Mime;
using System.Drawing;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MessageQueues
{
	class Program
	{
		private static string directoryPath;
		private static string queueName;
		static void Main(string[] args)
		{
			Console.Clear();

			Console.WriteLine("Enter directory path:");
			directoryPath = Console.ReadLine();

			Console.WriteLine("Enter queue name:");
			queueName = Console.ReadLine();

			MonitorDirectory(directoryPath);

			Console.ReadLine();
		}

		private static void MonitorDirectory(string path)
		{
			FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();
			fileSystemWatcher.Path = path;
			fileSystemWatcher.Created += FileSystemWatcher_Created;
			//fileSystemWatcher.Changed += FileSystemWatcher_Changed;

			fileSystemWatcher.EnableRaisingEvents = true;

		}

		private static void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
		{
			string filePath = directoryPath + @"\"+ e.Name;
			Console.WriteLine("File {0} added.", filePath);
			AddFileToQueue(filePath, queueName);
		}

		private static void AddFileToQueue(string filePath, string nameOfQueue)
		{
			MessageQueue queue;

			if (MessageQueue.Exists(nameOfQueue))
			{
				queue = new MessageQueue(nameOfQueue);
			}
			else
			{
				queue = MessageQueue.Create(nameOfQueue);
			}

			//Image myImage = Image.FromFile(filePath);

			/*MemoryStream stream = new MemoryStream();
			IFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, file);*/

			using (queue)
			using (Stream fileStream = new FileStream(filePath, FileMode.Open))
			{
				Message message = new Message()
				{
					BodyStream = fileStream,
					Label = Path.GetFileName(filePath),
					Priority = MessagePriority.Normal,
					Formatter = new BinaryMessageFormatter()
				};
			  //Message message = new Message("file");


				queue.Send(message);
			}

		}
	}
}
