namespace Oculus.Platform.Models
{
  using UnityEngine;
  using System.Collections;
  using System.Collections.Generic;
  using Oculus.Platform.Models;
  using Newtonsoft.Json;

  [JsonObject(MemberSerialization.OptIn)]
  public class Product
  {
    //Public interface
    public string Sku { get {return _Sku;} }
    public string Description { get {return CurrentOffer.Description;} }
    public string ID { get {return CurrentOffer.ID;} }
    public string Name { get {return CurrentOffer.Name;} }
    public string FormattedPrice { get {return CurrentOffer.FormattedPrice;} }


    //Internal
    private Product()
    {
      CurrentOffer = new Offer();
    }

    [JsonObject(MemberSerialization.OptIn)]
    private class Offer
    {
      public Offer()
      {
        CurrentPrice = new Price();
      }

      public string Description { get {return _Description;} }
      public string ID { get {return _ID;} }
      public string Name { get {return _Name;} }
      public string FormattedPrice { get {return CurrentPrice.Formatted;} }

      [JsonObject(MemberSerialization.OptIn)]
      private class Price {
        public string Formatted { get {return _Formatted;} }

        [JsonProperty("formatted")]
        private string _Formatted;
      }

      [JsonProperty("description")]
      private string _Description;

      [JsonProperty("id")]
      private string _ID;

      [JsonProperty("name")]
      private string _Name;

      [JsonProperty("price")]
      private Price CurrentPrice;
    }

    [JsonProperty("sku")]
    private string _Sku;

    [JsonProperty("current_offer")]
    private Offer CurrentOffer;
  }

  public class ProductList : DeserializableList<Product> {}
}
