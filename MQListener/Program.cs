using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MQListener
{
	class Program
	{
		private static string directoryPath;
		private static string queueName;
		static ManualResetEvent resetEvent = new ManualResetEvent(false);
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

				ReceiveMessage(queueName);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			Console.ReadLine();
		}

		private static void ReceiveMessage(string nameOfQueue)
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

			queue.Formatter = new BinaryMessageFormatter();

			using (queue)
			{
				queue.ReceiveCompleted += OnReceiveCompleted;

				while (true)
				{
					queue.BeginReceive();
					resetEvent.WaitOne();
				}
			}
		}

		private static void OnReceiveCompleted(Object source, ReceiveCompletedEventArgs asyncResult)
		{
			MessageQueue messageQueue = (MessageQueue)source;
			Message message = messageQueue.EndReceive(asyncResult.AsyncResult);

			if (message != null)
			{
				message.Formatter = new BinaryMessageFormatter();
				var fileName = message.Label;
				byte[] byteArray = message.Body as byte[];
				string fullFileName = directoryPath + @"\" + fileName;

				if (byteArray != null)
				{
					using (var fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write))
					{
						fs.Write(byteArray, 0, byteArray.Length);
					}
				}

				Console.WriteLine("File {0} received.", fullFileName);
			}

			resetEvent.Set();

			messageQueue.BeginReceive();
		}
	}
}
