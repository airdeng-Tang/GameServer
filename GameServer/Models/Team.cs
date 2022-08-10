﻿using Common;
using GameServer.Entities;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Models
{
    class Team
    {
        public int Id;
        public Character Leader;//队长

        public List<Character> Members = new List<Character>();//成员列表

        public int timestamp;//时间戳

        public Team(Character leader)
        {
            this.AddMember(leader);
        }
        
        /// <summary>
        /// 添加成员
        /// </summary>
        /// <param name="member"></param>
        public void AddMember(Character member)
        {
            if(this.Members.Count == 0)
            {
                this.Leader = member;
            }
            this.Members.Add(member);
            //this.Members.Add(member);
            timestamp = Time.timestamp;
            member.Team = this;
        }

        public void Leave(Character member)
        {
            Log.InfoFormat("Leave Team : {0} : {1}", member.Id, member.Info.Name);
            this.Members.Remove(member);

            if(member == this.Leader)
            {
                if(this.Members.Count > 0)
                {
                    this.Leader = this.Members[0];
                }
                else
                {
                    this.Leader = null;
                }
            }
            //member.Team = null;
            timestamp = Time.timestamp;
        }


        public void PostProcess(NetMessageResponse message)
        {
            if(message.teamInfo == null)
            {
                message.teamInfo = new TeamInfoResponse();
                message.teamInfo.Result = Result.Success;
                message.teamInfo.Team = new NTeamInfo();
                message.teamInfo.Team.Id = this.Id;
                message.teamInfo.Team.Leader = this.Leader.Id;
                foreach(var member in this.Members)
                {
                    message.teamInfo.Team.Members.Add(member.GetBasicInfo());
                }
            }
        }
    }
}
