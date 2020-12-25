namespace Oculus.Platform.Models
{
  using UnityEngine;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Runtime.Serialization;
  using Newtonsoft.Json;

  [JsonObject(MemberSerialization.OptIn)]
  public class AchievementDefinition
  {
    //Public interface
    public string Name { get {return _Name;} }
    public ulong Target { get {return _Target;} }
	  public uint BitfieldLength { get {return _BitfieldLength;} }
    public AchievementType Type { get { return _Type; } }

	  //Internal
    [JsonProperty("api_name")]
    private string _Name;

    [JsonProperty("achievement_type")]
    private string _TypeRaw;
    private AchievementType _Type;


    [JsonProperty("target")]
    private ulong _Target;

    [JsonProperty("bitfield_length")]
    private uint _BitfieldLength;

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
      if ("SIMPLE".Equals(_TypeRaw)) {
        _Type = AchievementType.Simple;
      } else if ("BITFIELD".Equals(_TypeRaw)) {
        _Type = AchievementType.Bitfield;
      } else if ("COUNT".Equals(_TypeRaw)){
        _Type = AchievementType.Count;
      } else {
        _Type = AchievementType.Unknown;
      }
      _TypeRaw = null;
    }
  }

  public class AchievementDefinitionList : DeserializableList<AchievementDefinition> {}
}
