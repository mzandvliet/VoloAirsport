namespace Oculus.Platform.Models
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using Newtonsoft.Json;

  [JsonObject(MemberSerialization.OptIn)]
  public class AchievementProgress {
    //Public interface
    public string Name { get {return _Definition.Name; } }
    public string Bitfield { get {return _BitfieldProgress ?? "";} }
    public ulong Count { get {return _CountProgress;} }
	  public bool IsUnlocked { get {return _IsUnlocked;} }
    public DateTime UnlockTime {
      get {
        var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return dt.AddSeconds(_UnlockTime).ToLocalTime ();
      }
    }

	  //Internal
    [JsonProperty("bitfield_progress")]
    private string _BitfieldProgress;

    [JsonProperty("count_progress")]
    private ulong _CountProgress;

    [JsonProperty("is_unlocked")]
    private bool _IsUnlocked;

    [JsonProperty("unlock_time")]
    private uint _UnlockTime;

    [JsonProperty("definition")]
    private AchievementDefinition _Definition;
  }

  public class AchievementProgressList : DeserializableList<AchievementProgress> {}
}
