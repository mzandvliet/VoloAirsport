namespace Oculus.Platform.Models
{
  using UnityEngine;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using Newtonsoft.Json;

  [JsonObject(MemberSerialization.OptIn)]
  public class UserProof
  {
    //Public interface
    public string Value { get {return _Nonce;} }

    //Internal
    [JsonProperty("nonce")]
    private string _Nonce;
  }
}
