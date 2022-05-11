using Common;
using Common.Data;
using GameServer.Entities;
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

        Dictionary<int, MapCharacter> MapCharacters = new Dictionary<int, MapCharacter>();

        internal Map(MapDefine define)
        {
            this.Define = define;
        }

        internal void Update()
        {

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

            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();

            message.Response.mapCharacterEnter = new MapCharacterEnterResponse();
            message.Response.mapCharacterEnter.mapId = this.Define.ID;
            message.Response.mapCharacterEnter.Characters.Add(character.Info);
            
            //遍历地图中玩家
            foreach(var kv in this.MapCharacters)
            {
                message.Response.mapCharacterEnter.Characters.Add(kv.Value.character.Info);//将地图中的玩家同步给登陆玩家
                if (kv.Value.character != character)//将自身信息发送给其他玩家
                    this.SendCharacterEnterMap(kv.Value.connection, character.Info);
            }
            Log.InfoFormat("添加前地图中玩家总数为:  {0}", this.MapCharacters.Count());
            //创建将玩家信息添加至地图
            this.MapCharacters[character.Id] = new MapCharacter(conn, character);
            
            Log.InfoFormat("添加后地图中玩家总数为:  {0}", this.MapCharacters.Count());
            byte[] data = PackageHandler.PackMessage(message);
            conn.SendData(data,0,data.Length);
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
            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();

            message.Response.mapCharacterEnter = new MapCharacterEnterResponse();
            message.Response.mapCharacterEnter.mapId = this.Define.ID;
            message.Response.mapCharacterEnter.Characters.Add(character);
            
            byte[] data = PackageHandler.PackMessage(message);
            conn.SendData(data, 0,data.Length);
        }

        void SendCharacterLeaveMap(NetConnection<NetSession> conn, Character character)
        {
            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            
            message.Response.mapCharacterLeave = new MapCharacterLeaveResponse();
            message.Response.mapCharacterLeave.characterId = character.Id;

            byte[] data = PackageHandler.PackMessage(message);
            conn.SendData(data, 0, data.Length);
        }

        //internal void UpdataEntity(NEntitySync entity)
        //{
        //    foreach(var ky in this.MapCharacters)
        //    {
        //        if(ky.Value.character == entity.Id)
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
                }
                else
                {
                    MapService.Instance.SendEntityUpdate(kv.Value.connection, entity);
                }
            }
        }
    }
}
