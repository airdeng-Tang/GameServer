using Common;
using Common.Data;
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

namespace GameServer.Models
{
    class Map
    {
        internal class MapCharacter
        {
            public NetConnection<NetSession> connection;
            public Character character;

            public MapCharacter(NetConnection<NetSession> conn, Character cha)
            {
                this.connection = conn;
                this.character = cha;
            }
        }

        public int ID
        {
            get { return this.Define.ID; }
        }
        internal MapDefine Define;

        /// <summary>
        /// 地图中的角色以CharacterID为Key
        /// </summary>
        Dictionary<int, MapCharacter> MapCharacters = new Dictionary<int, MapCharacter>();

        /// <summary>
        /// 刷怪管理器
        /// </summary>
        SpawnManager SpawnManager = new SpawnManager();

        /// <summary>
        /// 战斗
        /// </summary>
        public Battle.Battle Battle;

        /// <summary>
        /// 怪物管理器
        /// </summary>
        public MonsterManager MonsterManager = new MonsterManager();


        internal Map(MapDefine define)
        {
            this.Define = define;
            this.SpawnManager.Init(this);
            this.MonsterManager.Init(this);
            this.Battle = new Battle.Battle(this);
        }

        internal void Update()
        {
            SpawnManager.Update();
            this.Battle.Update();
        }
        /// <summary>
        /// 角色进入地图
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="character"></param>
        internal void CharacterEnter(NetConnection<NetSession> conn, Character character)
        {
            Log.InfoFormat("地图与角色信息=>CharacterEnter: Map: {0}  characterId:{1}", this.Define.ID, character.Id);
            
            character.Info.mapId = this.ID;
            this.MapCharacters[character.Id] = new MapCharacter(conn, character);

            //NetMessage message = new NetMessage();
            //message.Response = new NetMessageResponse();

            conn.Session.Response.mapCharacterEnter = new MapCharacterEnterResponse();
            conn.Session.Response.mapCharacterEnter.mapId = this.Define.ID;
            //message.Response.mapCharacterEnter.Characters.Add(character.Info);
            
            //遍历地图中玩家
            foreach(var kv in this.MapCharacters)
            {
                conn.Session.Response.mapCharacterEnter.Characters.Add(kv.Value.character.Info);//将地图中的玩家同步给登陆玩家
                if (kv.Value.character != character)//将自身信息发送给其他玩家
                    this.AddCharacterEnterMap(kv.Value.connection, character.Info);
            }
            Log.InfoFormat("添加前地图中玩家总数为:  {0}", this.MapCharacters.Count());
            //创建将玩家信息添加至地图

            foreach(var kv in this.MonsterManager.Monsters)
            {
                conn.Session.Response.mapCharacterEnter.Characters.Add(kv.Value.Info);
            }
            //this.MapCharacters[character.Id] = new MapCharacter(conn, character);
            
            Log.InfoFormat("添加后地图中玩家总数为:  {0}", this.MapCharacters.Count());
            //byte[] data = PackageHandler.PackMessage(message);
            //conn.SendData(data,0,data.Length);
            conn.SendResponse();
        }

        internal void CharacterLeave(Character cha)
        {
            Log.InfoFormat("CharacterLeave: Map{0} ; characterId:{1}", this.Define, cha.Id);
            Log.InfoFormat("退出  {0}  前玩家总数为:  {1}", cha.Id, this.MapCharacters.Count());
            foreach (var kv in this.MapCharacters){
                this.SendCharacterLeaveMap(kv.Value.connection, cha);
            }
            this.MapCharacters.Remove(cha.Id);
            Log.InfoFormat("退出  {0}  后玩家总数为:  {1}",cha.Id, this.MapCharacters.Count());
        }

        void SendCharacterEnterMap(NetConnection<NetSession> conn, NCharacterInfo character)
        {
            Log.InfoFormat("进入地图玩家id为 :: characterId:{0}", character.Id);
            //NetMessage message = new NetMessage();
            //message.Response = new NetMessageResponse();

            conn.Session.Response.mapCharacterEnter = new MapCharacterEnterResponse();
            conn.Session.Response.mapCharacterEnter.mapId = this.Define.ID;
            conn.Session.Response.mapCharacterEnter.Characters.Add(character);
            
            //byte[] data = PackageHandler.PackMessage(message);
            conn.SendResponse();
        }

        void AddCharacterEnterMap(NetConnection<NetSession> conn, NCharacterInfo character)
        {
            Log.InfoFormat("进入地图玩家id为 :: characterId:{0}", character.Id);
            if (conn.Session.Response.mapCharacterEnter == null)
            {
                conn.Session.Response.mapCharacterEnter = new MapCharacterEnterResponse();
                conn.Session.Response.mapCharacterEnter.mapId=this.Define.ID;
            }
            conn.Session.Response.mapCharacterEnter.Characters.Add(character);
            conn.SendResponse();
        }

        void SendCharacterLeaveMap(NetConnection<NetSession> conn, Character character)
        {
            Log.InfoFormat("SendCharacterLeaveMap To {0} : {1}  : Map : {2} Character : {3} : {4}",conn.Session.Character.Id,conn.Session.Character.Info.Name,this.Define.ID, character.Id, character.Info.Name);
            //NetMessage message = new NetMessage();
            //message.Response = new NetMessageResponse();
            
            //message.Response.mapCharacterLeave = new MapCharacterLeaveResponse();
            //message.Response.mapCharacterLeave.characterId = character.Id;

            //byte[] data = PackageHandler.PackMessage(message);
            //conn.SendData(data, 0, data.Length);

            conn.Session.Response.mapCharacterLeave = new MapCharacterLeaveResponse();
            conn.Session.Response.mapCharacterLeave.entityId = character.entityId;
            conn.SendResponse();
        }

        //internal void UpdataEntity(NEntitySync entity)
        //{
        //    foreach (var ky in this.MapCharacters)
        //    {
        //        if (ky.Value.character == entity.Id)
        //        {

        //        }
        //    }
        //}


        internal void UpdateEntity(NEntitySync entity)
        {
            foreach(var kv in this.MapCharacters)
            {
                if(kv.Value.character.entityId == entity.Id)
                {
                    kv.Value.character.Position = entity.Entity.Position;
                    kv.Value.character.Direction = entity.Entity.Direction;
                    kv.Value.character.Speed = entity.Entity.Speed;
                    if(entity.Event == EntityEvent.Ride)
                    {
                        kv.Value.character.Ride = entity.Param;
                    }
                }
                else
                {
                    MapService.Instance.SendEntityUpdate(kv.Value.connection, entity);
                }
            }
        }

        /// <summary>
        /// 添加怪物
        /// </summary>
        /// <param name="monster"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal void MonsterEnter(Monster monster)
        {
            Log.InfoFormat("MonsterEnter: Map:{0} monsterId:{1}", this.Define.ID, monster.Id);
            foreach(var kv in this.MapCharacters)
            {
                this.AddCharacterEnterMap(kv.Value.connection, monster.Info);
            }
        }
        
        /// <summary>
        /// 广播战斗
        /// </summary>
        /// <param name="response"></param>
        public void BroadcastBattleResponse(NetMessageResponse response)
        {
            foreach(var kv in this.MapCharacters)
            {
                kv.Value.connection.Session.Response.skillCast = response.skillCast;
                kv.Value.connection.SendResponse();
            }
        }
    }
}
