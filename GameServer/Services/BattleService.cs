﻿using Common;
using GameServer.Entities;
using GameServer.Managers;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Services
{
    public class BattleService : Singleton<BattleService>
    {

        public BattleService() 
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<SkillCastRequest>(this.OnSkillCast);
        }
        public void Init()
        {

        }

        void OnSkillCast(NetConnection<NetSession> sender, SkillCastRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("BattleService :: OnSkillCast: skill:{0} caster:{1} target:{2} pos:{3}", request.castInfo.skillId, request.castInfo.casterId, request.castInfo.targetId, request.castInfo.Position);

            //sender.Session.Response.skillCast = new SkillCastResponse();
            //sender.Session.Response.skillCast.Result = Result.Success;
            //sender.Session.Response.skillCast.castInfo = request.castInfo;

            //MapManager.Instance[character.Info.mapId].BroadcastBattleResponse(sender.Session.Response);

            BattleManager.Instance.ProcessBattleMessage(sender, request);
        }
    }
}
