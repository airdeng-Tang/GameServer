using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Configuration;

using System.Threading;

using Network;
using GameServer.Services;
using GameServer.Managers;

namespace GameServer
{
    class GameServer
    {
        Thread thread;
        bool running = false;
        NetService network;
        //UserService user;

        public bool Init()
        {
            //此配置为C#特有  右键项目名->属性->设置 处配置  代码体现于App.config中<userSettings>处
            int Port = Properties.Settings.Default.ServerPort;

            network = new NetService();
            network.Init(Port);
            //在类名后面加上:Singleton<>时则调用方法时不需要new仅需要在类名后接Instance(全局对象)接方法即可
            //参考:https://zhidao.baidu.com/question/1510467911375124700.html
            HelloWorldService.Instance.Init();//初始化服务 HelloWorldService
            DBService.Instance.Init();
            UserService.Instance.Init();
            DataManager.Instance.Load();
            MapManager.Instance.Init();
            //user.Init();
            thread = new Thread(new ThreadStart(this.Update));

            return true;
        }

        public void Start()
        {
            network.Start();
            HelloWorldService.Instance.Start();//启动服务 HelloWorldService
            running = true;
            thread.Start();
        }


        public void Stop()
        {
            
            running = false;
            thread.Join();
            
            network.Stop();
        }

        public void Update()
        {
            while (running)
            {
                Time.Tick();
                Thread.Sleep(100);
                //Console.WriteLine("{0} {1} {2} {3} {4}", Time.deltaTime, Time.frameCount, Time.ticks, Time.time, Time.realtimeSinceStartup);
            }
        }
    }
}
