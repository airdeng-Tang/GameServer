﻿using Common;
using Common.Data;
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
    class MapService : Singleton<MapService>
    {
        public MapService()
        {
            //MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<MapCharacterEnterRequest>(this.OnMapCharacterEnter);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<MapEntitySyncRequest>(this.OnMapEntitySync);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<MapTeleportRequest>(this.OnMapTeleport);

        }

        public void Init()
        {
            MapManager.Instance.Init();
        }

        private void OnMapCharacterEnter(NetConnection<NetSession> sender, MapCharacterEnterRequest message)
        {
            throw new NotImplementedException();
        }

        private void OnMapEntitySync(NetConnection<NetSession> sender, MapEntitySyncRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("MapService :: OnMapEntitySync: characterID:{0} : {1} Entity.id: {2} Evt:{3} Entity: {4}", character.Id, character.Info.Name, request.entitySync.Id, request.entitySync.Event, request.entitySync.Entity.String());

            MapManager.Instance[character.Info.mapId].UpdateEntity(request.entitySync);
        }

        internal void SendEntityUpdate(NetConnection<NetSession> conn, NEntitySync entity)
        {
            //NetMessage message = new NetMessage();
            //message.Response = new NetMessageResponse();

            //message.Response.mapEntitySync = new MapEntitySyncResponse();
            //message.Response.mapEntitySync.entitySyncs.Add(entity);

            conn.Session.Response.mapEntitySync = new MapEntitySyncResponse();
            conn.Session.Response.mapEntitySync.entitySyncs.Add(entity);
            conn.SendResponse();
        }

        void OnMapTeleport(NetConnection<NetSession> sender, MapTeleportRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("MapService :: OnMapTeleport(请求地图传送): characterID: {0} : {1} TeleporterId : {2}", character.Id, character.Data, request.teleporterId);

            if (!DataManager.Instance.Teleporters.ContainsKey(request.teleporterId))
            {
                Log.WarningFormat("Source TeleporterID [{0} not existed", request.teleporterId);
                return;
            }
            TeleporterDefine source = DataManager.Instance.Teleporters[request.teleporterId];
            if(source.LinkTo==0 || !DataManager.Instance.Teleporters.ContainsKey(source.LinkTo))
            {
                Log.WarningFormat("Source TeleporterID [{0}] LinkTo ID [{1}] not existed", request.teleporterId, source.LinkTo);
            }
            TeleporterDefine target = DataManager.Instance.Teleporters[source.LinkTo];

            MapManager.Instance[source.MapID].CharacterLeave(character);
            EntityManager.Instance.RemoveEntity(source.MapID, character);
            character.Position = target.Position;
            character.DirectionEntity = target.Direction;
            EntityManager.Instance.AddEntity(target.MapID, character);
            MapManager.Instance[target.MapID].CharacterEnter(sender, character);
        }
    }
}
