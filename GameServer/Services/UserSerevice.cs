using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Network;
using SkillBridge.Message;
using GameServer.Entities;
using GameServer.Managers;

namespace GameServer.Services
{
    class UserService : Singleton<UserService>
    {

        public UserService()
        {
            //Subscribe订阅消息
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserRegisterRequest>(this.OnRegister);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserLoginRequest>(this.OnLogin);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserCreateCharacterRequest>(this.OnCharacterCreate);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserGameEnterRequest>(this.OnGameEnter);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserGameLeaveRequest>(this.OnGameLeave);
        }

        public void Init()
        {
            Log.InfoFormat("UserService is open");

        }

        void OnRegister(NetConnection<NetSession> sender, UserRegisterRequest request)
        {
            Log.InfoFormat("UserRegisterRequest: User:{0}  Pass:{1}", request.User, request.Passward);


            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();

            message.Response.userRegister = new UserRegisterResponse();
            //message.Request.userRegister = 
            //message.Response.firsetTestRegister = new FirstTestResponse();
            //message.Request.firstRequest = new FirstTestRequest();

            TUser user = DBService.Instance.Entities.Users.Where(u => u.Username == request.User).FirstOrDefault();
            if (user != null)
            {
                message.Response.userRegister.Result = Result.Failed;
                message.Response.userRegister.Errormsg = "用户已存在.";
            }
            else
            {
                TPlayer player = DBService.Instance.Entities.Players.Add(new TPlayer());
                DBService.Instance.Entities.Users.Add(new TUser() { Username = request.User, Password = request.Passward, Player = player });
                DBService.Instance.Entities.SaveChanges();
                message.Response.userRegister.Result = Result.Success;
                message.Response.userRegister.Errormsg = "None";
            }

            byte[] data = PackageHandler.PackMessage(message);
            sender.SendData(data, 0, data.Length);
        }

        void OnLogin(NetConnection<NetSession> sender,UserLoginRequest request)
        {
            Log.InfoFormat("UserLogin: User:{0}  Pass:{1}", request.User, request.Passward);

            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            message.Response.userLogin = new UserLoginResponse();

            TUser user = DBService.Instance.Entities.Users.Where(u => u.Username == request.User).FirstOrDefault();
            if(user == null)
            {
                message.Response.userLogin.Result = Result.Failed;
                message.Response.userLogin.Errormsg = "未查询到此用户.";
                //return;
            }
            else{
                if(user.Password == request.Passward)
                {
                    sender.Session.User = user;

                    message.Response.userLogin.Result = Result.Success;
                    message.Response.userLogin.Errormsg = "None";
                    message.Response.userLogin.Userinfo = new NUserInfo();
                    message.Response.userLogin.Userinfo.Id = (int)user.ID;
                    message.Response.userLogin.Userinfo.Player = new NPlayerInfo();
                    message.Response.userLogin.Userinfo.Player.Id = user.Player.ID;
                    foreach(var c in user.Player.Characters)
                    {
                        NCharacterInfo cInfo = new NCharacterInfo();
                        cInfo.Tid = c.ID;

                        //cInfo.Name = c.Name;
                        cInfo.Id = c.ID;//entityId
                        cInfo.Name = c.Name;
                        cInfo.Type = CharacterType.Player;
                        cInfo.Class = (CharacterClass)c.Class;
                        message.Response.userLogin.Userinfo.Player.Characters.Add(cInfo);
                    }
                }
                else
                {
                    message.Response.userLogin.Result = Result.Failed;
                    message.Response.userLogin.Errormsg = "密码错误";
                    message.Response.userLogin.Userinfo = null;
                }
            }
            
            byte[] data = PackageHandler.PackMessage(message);
            sender.SendData(data, 0, data.Length);

        }

        void OnCharacterCreate(NetConnection<NetSession> sender, UserCreateCharacterRequest request)
        {
            Log.InfoFormat("角色创建:: 角色名称: {0}   角色职业: {1}", request.Name, request.Class);



            TCharacter character = new TCharacter()
            {
                Name = request.Name,
                Class = (int)request.Class,
                TID = (int)request.Class,
                MapID = 1,
                MapPosX = 5000,
                MapPosY = 4000,
                MapPosZ = 820,
                //Player = player
                Gold = 100000,
            };
            var bag = new TCharacterBag();
            bag.Owner = character;
            bag.Items = new byte[0];
            bag.Unlocked = 20;
            //TCharacterItem it = new TCharacterItem();
            character.Bag = DBService.Instance.Entities.CharacterBags.Add(bag);

            character = DBService.Instance.Entities.Characters.Add(character);
            //将数据保存在Session中以减少服务器压力
            sender.Session.User.Player.Characters.Add(character);
            //SaveChange()任何数据库操作都要在完成后调用此方法
            DBService.Instance.Entities.SaveChanges();

            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            message.Response.createChar = new UserCreateCharacterResponse();
            message.Response.createChar.Result = Result.Success;
            message.Response.createChar.Errormsg = "None";

            foreach (var c in sender.Session.User.Player.Characters)
            {
                NCharacterInfo info = new NCharacterInfo();
                info.Tid = c.ID;
                info.Id = 0;//entityId
                info.Name = c.Name;
                info.Type = CharacterType.Player;
                info.Class = (CharacterClass)c.Class;
                message.Response.createChar.Characters.Add(info);
            }
            //message.Response.createChar.Characters = null;

            byte[] data = PackageHandler.PackMessage(message);
            sender.SendData(data,0, data.Length);
        }

        void OnGameEnter(NetConnection<NetSession> sender, UserGameEnterRequest request)
        {
            TCharacter dbchar = sender.Session.User.Player.Characters.ElementAt(request.characterIdx);
            Log.InfoFormat("UserGameEnterRequest: characterID: {0} : {1} Map:{2}",dbchar.ID,dbchar.Name,dbchar.MapID);
            //CharacterManager角色管理器
            Character character = CharacterManager.Instance.AddCharacter(dbchar);

            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            message.Response.gameEnter= new UserGameEnterResponse();
            message.Response.gameEnter.Result = Result.Success;
            message.Response.gameEnter.Errormsg = "None";
            message.Response.gameEnter.Character = character.Info;


            ///道具系统测试
            int itemId = 2;
            bool hasItem = character.ItemManager.HasItem(itemId);
            Log.InfoFormat("HasItem: [{0}] {1}", itemId, hasItem);
            if (hasItem)
            {
                //character.ItemManager.RemoveItem(itemId, 1);

            }
            else
            {
                character.ItemManager.AddItem(1, 200);
                character.ItemManager.AddItem(2, 100);
                character.ItemManager.AddItem(3, 30);
                character.ItemManager.AddItem(4, 120);
            }
            Models.Item item = character.ItemManager.GetItem(itemId);
            Log.InfoFormat("<==============>Item: [{0}] [{1}]", itemId, item);
            DBService.Instance.Save();
            ///测试end


            byte[] data = PackageHandler.PackMessage(message);
            sender.SendData(data, 0, data.Length);
            sender.Session.Character = character;
            MapManager.Instance[dbchar.MapID].CharacterEnter(sender, character);

        }

        void OnGameLeave(NetConnection<NetSession> sender, UserGameLeaveRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("角色退出 :: UserGameLeaveRequest: characterID:{0} : {1} Map:{2}", character.Id, character.Info.Name, character.Info.mapId);

            CharacterLeave(character);
            sender.Session.Character = null;
            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            message.Response.gameLeave = new UserGameLeaveResponse();
            message.Response.gameLeave.Result = Result.Success;
            message.Response.gameLeave.Errormsg = "None";

            byte[] data = PackageHandler.PackMessage(message);
            sender.SendData(data, 0, data.Length);
        }

        public void CharacterLeave(Character character)
        {
            CharacterManager.Instance.RemoveCharacter(character.Id);
            MapManager.Instance[character.Info.mapId].CharacterLeave(character);
        }

    }
}
