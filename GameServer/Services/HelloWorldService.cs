using Common;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Services
{
    public class HelloWorldService : Singleton<HelloWorldService>
    {
        public void Init()
        {
            //entities = new ExtremeWorldEntities();
        }
        public void Start()
        {
            //用(OnFirstTestRequest())方法处理 订阅消息(协议FirstTestRequest)  
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<FirstTestRequest>(this.OnFirstTestRequest);
        }


        //处理订阅的消息
        private void OnFirstTestRequest(NetConnection<NetSession> sender, FirstTestRequest request)
        {
            //throw new NotImplementedException();
            Log.InfoFormat("UserLoginRequest: Helloworld:{0} ", request.Helloworld);

            //sender.SendData();//返回客户端信息方法
        }

        public void Stop()
        {

        }
    }
}
