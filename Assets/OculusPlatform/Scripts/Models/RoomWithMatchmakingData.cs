namespace Oculus.Platform.Models
{
  using UnityEngine;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Runtime.Serialization;
  using Newtonsoft.Json;

  [JsonObject(MemberSerialization.OptIn)]
  public class MatchmakingEnqueueResultAndRoom
  {
    public Room Room { get {return _Room;} }
    public uint AverageWait { get {return _AverageWait;} }
    public uint MaxExpectedWait { get {return _MaxExpectedWait;} }
    public string RequestHash { get {return _RequestHash;} }

    [JsonProperty("viewer_room")]
    private Room _Room;

    [JsonProperty("average_wait_s")]
    private uint _AverageWait;

    [JsonProperty("max_expected_wait_s")]
    private uint _MaxExpectedWait;

    [JsonProperty("trace_id")]
    private string _RequestHash;

    private MatchmakingEnqueueResultAndRoom()
    {
      _Room = new Room();
    }
  }

}
