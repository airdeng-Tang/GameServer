﻿using Common;
using GameServer.Entities;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    class CharacterManager : Singleton<CharacterManager>
    {
        public Dictionary<int, Character> Characters = new Dictionary<int, Character>();

        public CharacterManager()
        {

        }

        public void Dispose()
        {

        }

        public void Init()
        {

        }

        public void Clear()
        {
            this.Characters.Clear();
        }

        public Character AddCharacter(TCharacter cha)
        {
            Character character = new Character(CharacterType.Player, cha);
            EntityManager.Instance.AddEntity(cha.MapID, character);
            this.Characters[cha.ID] = character;
            return character;
        }

        public void RemoveCharacter(int characterId)
        {
            var cha = this.Characters[characterId];
            EntityManager.Instance.RemoveEntity(cha.Data.MapID, cha);
            this.Characters.Remove(characterId);
        }
    }
}
