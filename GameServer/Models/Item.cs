using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Models
{
    class Item
    {
        TCharacterItem dbItem;

        public int ItemID;
        public int Count;//数量

        public Item(TCharacterItem item)
        {
            this.dbItem = item;

            this.ItemID = (short)item.ItemID;
            this.Count = (short)item.ItemCount;
        }

        public void Add(int count)
        {
            this.Count += count;
            dbItem.ItemCount = this.Count;
        }

        public void Remove(int count)
        {
            this.Count -= count;
            dbItem.ItemCount = this.Count;
        }

        public bool Use(int count = 1)
        {
            //return this.Count > count;
            return false;
        }

        public override string ToString()
        {
            return String.Format("ID : {0} , Count :{1}", this.ItemID, this.Count);
        }
    }
}
