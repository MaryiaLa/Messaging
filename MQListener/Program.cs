using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MQListener
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

			ReceiveMessage(directoryPath, queueName);

			Console.ReadLine();
		}

		private static void ReceiveMessage(string filePath, string nameOfQueue)
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
				var message = queue.Receive();
				message.Formatter = new BinaryMessageFormatter();
				var reader = new StreamReader(message.BodyStream, Encoding.Unicode);
				var msgBody = reader.ReadToEnd();

				/*IFormatter formatter = new BinaryFormatter();
				stream.Seek(0, SeekOrigin.Begin);
				object o = formatter.Deserialize(stream);*/
			}
		}
	}
}
