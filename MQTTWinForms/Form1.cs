using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MQTTWinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
          
        }
        private IMqttClient mqttClient;
        private void MqttConnectAsync(User user)
        {
            try
            {
                var mqttFactory = new MqttFactory();
                //使用Build构建
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(user.serve, user.port)
                    .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V311)
                    .WithClientId("clientid_pascalming")
                    .WithCleanSession(false)
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
                    .WithCredentials(user.userName, user.passWord)
                    .Build();

                mqttClient = mqttFactory.CreateMqttClient();
                //与3.1对比，事件订阅名称和接口已经变化
                mqttClient.DisconnectedAsync += MqttClient_DisconnectedAsync;
                mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;
                mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;
                Task task = mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
                task.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mqtt客户端尝试连接出错：{ex.Message}");
                label5.Text = $"Mqtt客户端尝试连接出错：{ex.Message}";
            }
        }
        private Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            var payloadString = arg.ApplicationMessage.ConvertPayloadToString();

            payloadString = ConvertJsonString(payloadString);

            var item = $"{Environment.NewLine}Topic: {arg.ApplicationMessage.Topic}{Environment.NewLine}Payload: {payloadString} {Environment.NewLine}QoS: {arg.ApplicationMessage.QualityOfServiceLevel}";
            label7.Text = $"订阅到Topic消息{item}";
            return Task.CompletedTask;
        }

        private Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            label5.Text = "Mqtt客户端连接成功";
            Console.WriteLine($"Mqtt客户端连接成功.");
            return Task.CompletedTask;
        }

        private Task MqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            label5.Text = "Mqtt客户端连接断开";
            Console.WriteLine($"Mqtt客户端连接断开");
            return Task.CompletedTask;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var s=textBox1.Text.Trim();
           var p= textBox2.Text.Trim();
            if (string.IsNullOrEmpty(s) && string.IsNullOrEmpty(p)) return;
            var user = new User
            {
                serve=s,port=Convert.ToInt32(p)
            };
            MqttConnectAsync(user);
        }
        private string ConvertJsonString(string str)
        {
            try
            {
                //格式化json字符串
                JsonSerializer serializer = new JsonSerializer();
                TextReader tr = new StringReader(str);
                JsonTextReader jtr = new JsonTextReader(tr);
                object obj = serializer.Deserialize(jtr);
                if (obj != null)
                {
                    StringWriter textWriter = new StringWriter();
                    JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                    {
                        Formatting = Formatting.Indented,
                        Indentation = 4,
                        IndentChar = ' '
                    };
                    serializer.Serialize(jsonWriter, obj);
                    return textWriter.ToString();
                }

                return str;
            }
            catch (Exception ex)
            {
                return str;
            }
        }

        public class User
        {
            public string serve { get; set; }
            public int port { get; set; }
            public string? userName { get; set; }
            public string? passWord { get; set; }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var topicFilter = new MqttTopicFilter { Topic = this.textBox3.Text.Trim() };
            this.mqttClient.SubscribeAsync(topicFilter);
            label7.Text = $"订阅到Topic{this.textBox3.Text.Trim()}";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string topics =  this.textBox3.Text.Trim();
            this.mqttClient.UnsubscribeAsync(topics);
            label7.Text = $"已取消订阅Topic：{ this.textBox3.Text.Trim()}";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var publish_topic = textBox4.Text.Trim();
            var publish_msg = textBox5.Text;
            var message = new MqttApplicationMessageBuilder()
            .WithTopic(publish_topic)
            .WithPayload(publish_msg)
            .Build();

            if (this.mqttClient != null)
            {
                // this.mqttClient.PublishAsync(message);

                Task<MqttClientPublishResult> task = mqttClient.PublishAsync(message);
                task.Wait();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.mqttClient.DisconnectAsync();
        }
    }
}
