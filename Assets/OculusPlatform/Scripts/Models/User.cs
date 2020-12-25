namespace Oculus.Platform.Models
{
  using UnityEngine;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using Newtonsoft.Json;

  [JsonObject(MemberSerialization.OptIn)]
  public class User
  {
    //Public interface
    public string OculusID { get {return _OculusID;} }
    public UInt64 ID { get {return _ID;} }
	  public string InviteToken { get {return _InviteToken;} }
	  public string Presence { get {return _Presence;} }
    public string ImageURL { get {return _ProfileURL;} }

    //Internal
    [JsonProperty("alias")]
    private string _OculusID;

    [JsonProperty("id")]
    private UInt64 _ID;

    [JsonProperty("token")]
    private string _InviteToken;

    [JsonProperty("presence")]
    private string _Presence;

    [JsonProperty("profile_url")]
    private string _ProfileURL;
  }

  public class UserList : DeserializableList<User> {}
}
