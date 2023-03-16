using Common;
using GameServer.Entities;
using GameServer.Managers;
using GameServer.Services;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Services
{
    class FriendService : Singleton<FriendService>
    {
        //List<FriendAddRequest> friendRequests = new List<FriendAddRequest>();

        public FriendService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<FriendAddRequest>(this.OnFriendAddRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<FriendAddResponse>(this.OnFriendAddResponse);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<FriendRemoveRequest>(this.OnFriendRemove);

        }

        public void Init()
        {

        }

        /// <summary>
        /// 收到添加好友请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        void OnFriendAddRequest(NetConnection<NetSession> sender, FriendAddRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnFriendAddRequest: : FromId:{0} FromName:{1} ToID:{2} ToName: {3}", request.FromId, request.FromName, request.ToId, request.ToName);
            
            if(request.ToId == 0)
            {
                foreach(var cha in CharacterManager.Instance.Characters)
                {
                    if(cha.Value.Data.Name == request.ToName)
                    {
                        request.ToId = cha.Key;
                        break;
                    }
                }
            }
            NetConnection<NetSession> friend = null;
            if(request.ToId > 0)
            {
                if (character.FriendManager.GetFriendInfo(request.ToId) != null)
                {
                    sender.Session.Response.friendAddRes = new FriendAddResponse();
                    sender.Session.Response.friendAddRes.Result = Result.Failed;
                    sender.Session.Response.friendAddRes.Errormsg = "已经是好友了";
                    sender.SendResponse();
                    return;
                }
                friend = SessionManager.Instance.GetSession(request.ToId);
            }

            if(friend == null)
            {
                sender.Session.Response.friendAddRes = new FriendAddResponse();
                sender.Session.Response.friendAddRes.Result = Result.Failed;
                sender.Session.Response.friendAddRes.Errormsg = "好友不存在或者不在线";
                sender.SendResponse();
                return;
            }

            Log.InfoFormat("ForwardRequest: : FromId:{0}  FormName: {1} ToID: {2} ToName:{3}", request.FromId, request.FromName, request.ToId, request.ToName);
            friend.Session.Response.friendAddReq = request;
            friend.SendResponse();
        }

        /// <summary>
        /// 收到添加好友响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="response"></param>
        void OnFriendAddResponse(NetConnection<NetSession> sender, FriendAddResponse response)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnFriendAddResponse: : character: {0} Result:{1} FromId:{2} ToID:{3}", character.Id, response.Result, response.Request.FromId, response.Request.ToId);
            sender.Session.Response.friendAddRes = response;
            var requester = SessionManager.Instance.GetSession(response.Request.FromId);
            if (response.Result == Result.Success)
            {
                if(requester == null)
                {
                    sender.Session.Response.friendAddRes.Result = Result.Failed;
                    sender.Session.Response.friendAddRes.Errormsg = "请求者已下线";
                }
                else
                {
                    character.FriendManager.AddFriend(requester.Session.Character);
                    requester.Session.Character.FriendManager.AddFriend(character);
                    DBService.Instance.Save();
                    requester.Session.Response.friendAddRes = response;
                    requester.Session.Response.friendAddRes.Result = Result.Success;
                    requester.Session.Response.friendAddRes.Errormsg = "添加好友成功";
                    requester.SendResponse();
                }
            }
            else
            {
                requester.Session.Response.friendAddRes = new FriendAddResponse();
                requester.Session.Response.friendAddRes.Result = Result.Failed;
                requester.Session.Response.friendAddRes.Errormsg = character.Name + "  拒绝加你为好友";
            }


            //sender.Session.Response.friendAddRes.Result = Result.Success;
            //sender.Session.Response.friendAddRes.Errormsg = response.Request.FromName + "成为您的好友";
            sender.SendResponse();
        }


        void OnFriendRemove(NetConnection<NetSession> sender, FriendRemoveRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnFriendRemove: : character: {0} FriendReletionID: {1}", character.Id, request.Id);
            sender.Session.Response.friendRemove = new FriendRemoveResponse();
            sender.Session.Response.friendRemove.Id = request.Id;
            //删除自己好友
            if (character.FriendManager.RemoveFriendByID(request.Id))
            {
                sender.Session.Response.friendRemove.Result = Result.Success;

                //删除别人好友中的自己
                var friend = SessionManager.Instance.GetSession(request.friendId);
                if(friend != null)
                {//在线
                    var character1 = SessionManager.Instance.GetSession(character.Id);
                    character1.Session.Character.FriendManager.RemoveFriendByFriendId(request.friendId);
                    friend.Session.Character.FriendManager.RemoveFriendByFriendId(character.Id);
                }
                else
                {//不在线
                    this.RemoveFriend(request.friendId, character.Id);
                }
            }
            else
            {
                sender.Session.Response.friendRemove.Result = Result.Failed;
            }

            DBService.Instance.Save();

            sender.SendResponse();
        }

        void RemoveFriend(int charId,int friendId)
        {
            var removeItem = DBService.Instance.Entities.CharacterFriends.FirstOrDefault(v => v.CharacterID == charId && v.FriendID == friendId);
            var removeItem2 = DBService.Instance.Entities.CharacterFriends.FirstOrDefault(v => v.CharacterID == friendId && v.FriendID == charId);
            if(removeItem != null)
            {
                DBService.Instance.Entities.CharacterFriends.Remove(removeItem);
            }
            if(removeItem2 != null)
            {
                DBService.Instance.Entities.CharacterFriends.Remove(removeItem2);
            }
        }
    }
}
