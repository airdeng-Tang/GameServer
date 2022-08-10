using Common;
using GameServer.Entities;
using GameServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    class TeamManager : Singleton<TeamManager>
    {
        public List<Team> Teams = new List<Team>();
        public Dictionary<int, Team> CharacterTeams = new Dictionary<int, Team>();

        public void Init()
        {
            
        }

        public void clear()
        {
            Teams.Clear();
        }

        public Team GetTeamByCharacter(int characterId)
        {
            Team team = null;
            this.CharacterTeams.TryGetValue(characterId, out team); 
            return team;
        }

        public Team GetTeamTimestamp(Character member)
        {
            return this.Teams.Find(
                    (Team t) => t.Id == member.Team.Id
                );
        }

        public void LeaveTeamByCharacterInList(Character member)
        {
            //this.clear();
            if (member.Team != null)
            {

                //List.Find()方法使用参考  https://www.xin3721.com/ArticlePrograme/C_biancheng/9306.html
                //与 https://blog.csdn.net/jerry11112/article/details/86065017
                ///第一种用法
                //Team team = TeamManager.Instance.Teams.Find(
                //    delegate (Team t) { 
                //        return (t.Id == character.Team.Id); 
                //    }
                //);
                ///第二种用法
                this.Teams.Find(
                    (Team t) => t.Id == member.Team.Id
                ).Leave(member);
            }
        }

        /// <summary>
        /// 添加至队伍
        /// </summary>
        /// <param name="leader">队长</param>
        /// <param name="member">添加队员</param>
        public void AddTeamMember(Character leader, Character member)
        {
            if(leader.Team == null)
            {
                leader.Team = CreateTeam(leader);
            }
            //if (this.Teams.Find(
            //        (Team t) => t.Id == leader.Team.Id
            //    ).Members.Count == 2)
            //{
            //    this.clear();
            //}
            leader.Team.AddMember(member);
            
        }

        Team CreateTeam(Character leader)
        {
            Team team = null;
            for(int i =0; i < this.Teams.Count; i++)
            {
                team = this.Teams[i];
                if(team.Members.Count == 0)
                {
                    team.AddMember(leader);
                    return team;
                }
            }
            team = new Team(leader);
            this.Teams.Add(team);
            team.Id = this.Teams.Count;
            return team;
        }
    }
}
