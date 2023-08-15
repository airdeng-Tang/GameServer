using GameServer.Core;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Entities
{
    class Entity
    {
        public int entityId
        {
            get { return this.entityData.Id; }
        }


        private Vector3Int position;

        public Vector3Int Position
        {
            get { return position; }
            set {
                position = value;
                this.entityData.Position = position;
            }
        }

        /// <summary>
        /// 对象实体的朝向(面对方向)
        /// </summary>
        public Vector3Int direction;
        public Vector3Int DirectionEntity
        {
            get { return direction; }
            set
            {
                direction = value;
                this.entityData.Direction = direction;
            }
        }

        private int speed;
        public int Speed
        {
            get { return speed; }
            set
            {
                speed = value;
                this.entityData.Speed = speed;
            }
        }

        private NEntity entityData;

        /// <summary>
        /// 实体的位置坐标方向信息
        /// </summary>
        public NEntity EntityData
        {
            get
            {
                return entityData;
            }
            set
            {
                entityData = value;
                this.SetEntityData(value);
            }
        }

        public Entity(Vector3Int pos,Vector3Int dir)
        {
            this.entityData = new NEntity();
            this.entityData.Position = pos;
            this.entityData.Direction = dir;
            this.SetEntityData(this.entityData);
        }

        public Entity(NEntity entity)
        {
            this.entityData = entity;
        }

        public void SetEntityData(NEntity entity)
        {
            this.Position = entity.Position;
            this.DirectionEntity = entity.Direction;
            this.speed = entity.Speed;
        }

        public virtual void Update()
        {

        }
    }
}
