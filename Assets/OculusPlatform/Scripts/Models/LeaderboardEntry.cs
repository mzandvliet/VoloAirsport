namespace Oculus.Platform.Models
{
  using UnityEngine;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Runtime.Serialization;
  using Newtonsoft.Json;

  [JsonObject(MemberSerialization.OptIn)]
  public class LeaderboardEntry
  {
    public int Rank { get { return rank; } }
    public User User { get { return user; } }
    public Int64 Score { get { return score; } }
    public uint Timestamp { get { return timestamp; } }
    public byte[] ExtraData { get { return extraData; } }

    private LeaderboardEntry()
    {
    }

    [JsonProperty("rank")]
    private int rank;

    [JsonProperty("user")]
    private User user;

    [JsonProperty("score")]
    private Int64 score;

    [JsonProperty("timestamp")]
    private uint timestamp;

    [JsonProperty("extra_data_base64")]
    private string extraDataRaw;

    private byte[] extraData;

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
      if (extraDataRaw != null)
      {
        try
        {
          extraData = System.Convert.FromBase64String(extraDataRaw);
        }
        catch (Exception e)
        {
          Debug.LogException(e);
        }
        extraDataRaw = null;
      }
    }
  }

  public class LeaderboardEntryList : DeserializableList<LeaderboardEntry>
  {
    public uint TotalCount
    {
      get
      {
        return (summary != null) ? summary.TotalCount : 0;
      }
    }

    [JsonProperty("summary")]
    private Summary summary;

    private class Summary
    {
      [JsonProperty("total_count")]
      public uint TotalCount;
    }
  }

}
