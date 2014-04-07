﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsFormsApplication7.Frontend;
using WindowsFormsApplication7.Business;
using SlimDX;
using WindowsFormsApplication7.Business.Repositories;

namespace WindowsFormsApplication7.CrossCutting.Entities.Items
{
    class ItemFood : Item
    {
        public ItemFood(int itemId, string name)
            : base(itemId, name)
        {
        }
    }
}
