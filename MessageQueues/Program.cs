using System;
using System.Collections.Generic;
using System.Configuration;
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

			try
			{
				directoryPath = ConfigurationSettings.AppSettings["directoryPath"];

				if (!Directory.Exists(directoryPath))
				{
					throw new Exception("Directory does not exists");
				}

				queueName = ConfigurationSettings.AppSettings["queueName"];

				MonitorDirectory(directoryPath);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			Console.ReadLine();
		}

		private static void MonitorDirectory(string path)
		{
			string[] existingFiles = Directory.GetFiles(path);

			if (existingFiles.Length > 0)
			{
				foreach (var file in existingFiles)
				{
					AddFileToQueue(file, queueName, Path.GetFileName(file));
				}
			}

			FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();
			fileSystemWatcher.Path = path;
			fileSystemWatcher.Created += FileSystemWatcher_Created;

			fileSystemWatcher.EnableRaisingEvents = true;

		}

		private static void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
		{
			string filePath = directoryPath + @"\"+ e.Name;
			Console.WriteLine("File {0} added.", filePath);
			AddFileToQueue(filePath, queueName, e.Name);
		}

		private static void AddFileToQueue(string filePath, string nameOfQueue, string fileName)
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

			using (queue)
			using (FileStream fileStream = File.OpenRead(filePath))
			{
				byte[] fileBytes = new byte[fileStream.Length];
				fileStream.Read(fileBytes, 0, fileBytes.Length);

				Message message = new Message()
				{
					Body = fileBytes,
					Priority = MessagePriority.Normal,
					Formatter = new BinaryMessageFormatter(),
					Label = fileName
				};

				queue.Send(message);
				Console.WriteLine("File {0} sent.", filePath);

				fileStream.Close();
			}
			DeleteFile(filePath);
		}

		private static void DeleteFile(string filePath)
		{
			File.Delete(filePath);
			Console.WriteLine("File {0} dropped.", filePath);
		}
	}
}
