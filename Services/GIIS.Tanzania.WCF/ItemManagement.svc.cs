﻿//*******************************************************************************
//Copyright 2015 TIIS  Webservices - Tanzania Immunization Information System
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
 //******************************************************************************
using GIIS.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace GIIS.Tanzania.WCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ItemManagement" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select ItemManagement.svc or ItemManagement.svc.cs at the Solution Explorer and start debugging.
    public class ItemManagement : IItemManagement
    {
        public List<Item> GetItemList()
        {
            List<Item> itemList = Item.GetItemList();

            return itemList;
        }


		public ItemManufacturer GetItemManufacturerByGtin(string gtin)
		{
			ItemManufacturer itemList = ItemManufacturer.GetItemManufacturerByGtin(gtin);
			return itemList;
		}


		public List<ItemManufacturer> GetItemManufacturerList()
		{
			List<ItemManufacturer> itemList = ItemManufacturer.GetItemManufacturerList();
			return itemList;
		}

    }
}
