namespace Oculus.Platform.Models
{
  using UnityEngine;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using Newtonsoft.Json;

  [JsonObject(MemberSerialization.OptIn)]
  public class Purchase
  {
    //Public interface
    public string Sku { get {return _Item.Sku;} }
    public UInt64 ID { get {return _ID;} }
    public DateTime GrantTime {
      get {
        var dateTime = new DateTime (1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        return dateTime.AddSeconds (_GrantTime);
      }
    }


    //Internal
    private Purchase()
    {
      _Item = new Item();
    }

    [JsonObject(MemberSerialization.OptIn)]
    private class Item
    {
      public string Sku { get {return _Sku;} }

      [JsonProperty("sku")]
      private string _Sku;
    }

    [JsonProperty("item")]
    private Item _Item;

    [JsonProperty("id")]
    private UInt64 _ID;

    [JsonProperty("grant_time")]
    private UInt64 _GrantTime;

  }

  public class PurchaseList : DeserializableList<Purchase> {}
}
