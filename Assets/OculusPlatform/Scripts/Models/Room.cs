namespace Oculus.Platform.Models
{
  using UnityEngine;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Runtime.Serialization;
  using Newtonsoft.Json;

  [JsonObject(MemberSerialization.OptIn)]
  public class Room
  {
    //Public interface
    public UInt64 ID { get {return _ID;} }
    public uint MaxUsers { get {return _MaxUsers;} }
    public string Description { get { return _Description;} }
    public RoomType Type { get { return _Type; } }

    public JoinPolicy JoinPolicy { get { return _JoinPolicy; } }
    public RoomJoinability Joinability { get { return _Joinability; } }


    public User Owner { get {return _Owner; } }
    public UserList Users { get { return _Users; } }
    public UInt64 ApplicationID { get { return _Application.ID; } }
    public Dictionary<string, string> DataStore;

    //Internal
    internal Room()
    {
      _Application = new Application();
    }

    [JsonObject(MemberSerialization.OptIn)]
    private class Application
    {
      public UInt64 ID { get {return _ID;} }

      [JsonProperty("id")]
      private UInt64 _ID;
    }

    [JsonProperty("max_users")]
    private uint _MaxUsers;

    [JsonProperty("id")]
    private UInt64 _ID;

    [JsonProperty("description")]
    private string _Description;

    [JsonProperty("type")]
    private string _TypeRaw;
    private RoomType _Type;

    [JsonProperty("join_policy")]
    private string _JoinPolicyRaw;
    private JoinPolicy _JoinPolicy;

    [JsonProperty("joinability")]
    private string _JoinabilityRaw;
    private RoomJoinability _Joinability;

    [JsonProperty("owner")]
    private User _Owner;

    [JsonProperty("users")]
    private UserList _Users;

    [JsonProperty("application")]
    private Application _Application;

    [JsonProperty("data_store")]
    private List<Pair> _DataStoreRawArray;

    [JsonObject(MemberSerialization.OptIn)]
    private class Pair
    {
      [JsonProperty("key")]
      public string _Key;

      [JsonProperty("value")]
      public string _Value;
    }

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
      if(_DataStoreRawArray != null)
      {
        DataStore = new Dictionary<string, string>();
        foreach(Pair pair in _DataStoreRawArray)
        {
          DataStore[pair._Key] = pair._Value;
        }
        _DataStoreRawArray = null;
      }

      //Handle RoomType
      if ("MATCHMAKING".Equals(_TypeRaw)) {
        _Type = RoomType.Matchmaking;
      } else if ("MODERATED".Equals(_TypeRaw)) {
        _Type = RoomType.Moderated;
      } else if ("PRIVATE".Equals(_TypeRaw)){
        _Type = RoomType.Private;
      } else if ("SOLO".Equals(_TypeRaw)){
        _Type = RoomType.Solo;
      } else {
        _Type = RoomType.Unknown;
      }
      _TypeRaw = null;

      //Handle JoinPolicy
      if ("EVERYONE".Equals(_JoinPolicyRaw)) {
        _JoinPolicy = JoinPolicy.Everyone;
      } else if ("FRIENDS_OF_MEMBERS".Equals(_JoinPolicyRaw)){
        _JoinPolicy = JoinPolicy.FriendsOfMembers;
      } else if ("FRIENDS_OF_OWNER".Equals(_JoinPolicyRaw)){
        _JoinPolicy = JoinPolicy.FriendsOfOwner;
      } else if ("INVITED_USERS".Equals(_JoinPolicyRaw)) {
        _JoinPolicy = JoinPolicy.InvitedUsers;
      } else {
        _JoinPolicy = JoinPolicy.None;
      }
      _JoinPolicyRaw = null;

      //Handle Joinability
      if ("ARE_IN".Equals(_JoinabilityRaw)) {
        _Joinability = RoomJoinability.AreIn;
      } else if ("ARE_KICKED".Equals(_JoinabilityRaw)){
        _Joinability = RoomJoinability.AreKicked;
      } else if ("CAN_JOIN".Equals(_JoinabilityRaw)){
        _Joinability = RoomJoinability.CanJoin;
      } else if ("IS_FULL".Equals(_JoinabilityRaw)) {
        _Joinability = RoomJoinability.IsFull;
      } else if ("NO_VIEWER".Equals(_JoinabilityRaw)) {
        _Joinability = RoomJoinability.NoViewer;
      } else if ("POLICY_PREVENTS".Equals(_JoinabilityRaw)) {
        _Joinability = RoomJoinability.PolicyPrevents;
      } else {
        _Joinability = RoomJoinability.Unknown;
      }
      _JoinabilityRaw = null;
    }
  }

  public class RoomList : DeserializableList<Room> {}

}
