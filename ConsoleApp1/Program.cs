
using MQTTnet;
using MQTTnet.Server;
using System;

namespace MQTTServer
{
    class Program
    {
      //  IMqttServer server = new MqttFactory().CreateMqttServer();
        static async void Main(string[] args)
        {
            // var options = new MqttServerOptions();
            var options = new MqttServerOptions();
            var mqttServer = new MqttFactory().CreateMqttServer(options);
            await mqttServer.StartAsync();
            Console.WriteLine("Hello MQTTServer!");
            Console.ReadLine();
        }
    }
}
