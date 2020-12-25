// This file was @generated with LibOVRPlatform/codegen/main. Do not modify it!
// To make additional changes, modify LibOVRPlatform/codegen/requests.yaml

namespace Oculus.Platform
{
  using UnityEngine;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using Newtonsoft.Json;
  using Oculus.Platform.Models;

  public abstract class Message<T> : Message
  {
    public delegate void Callback(Message<T> message);
    public Message(IntPtr c_message) : base(c_message) {
      if (!IsError)
      {
        data = GetDataFromMessage(c_message);
      }
    }

    public T Data { get { return data; } }
    protected abstract T GetDataFromMessage(IntPtr c_message);
    private T data;
  }

  public class Message
  {
    public delegate void Callback(Message message);
    public Message(IntPtr c_message)
    {
      type = (MessageType)CAPI.ovr_Message_GetType(c_message);
      var isError = CAPI.ovr_Message_IsError(c_message);
      requestID = CAPI.ovr_Message_GetRequestID(c_message);

      if (isError)
      {
        IntPtr errorHandle = CAPI.ovr_Message_GetError(c_message);
        error = new Error(
          CAPI.ovr_Error_GetCode(errorHandle),
          CAPI.ovr_Error_GetMessage(errorHandle),
          CAPI.ovr_Error_GetHttpCode(errorHandle));
      }
      else if (Debug.isDebugBuild)
      {
        var message = CAPI.ovr_Message_GetString(c_message);
        if (message != null)
        {
          Debug.Log(message);
        }
        else
        {
          Debug.Log(string.Format("null message string {0}", c_message));
        }
      }
    }

    ~Message()
    {
    }

    // Keep this enum in sync with ovrMessageType in OVR_Platform.h
    public enum MessageType : uint
    { //TODO - rename this to type; it's already in Message class
      Unknown,

      Achievements_AddCount             = 0x03E76231,
      Achievements_AddFields            = 0x14AA2129,
      Achievements_GetAllDefinitions    = 0x03D3458D,
      Achievements_GetAllProgress       = 0x4F9FDE1D,
      Achievements_GetDefinitionsByName = 0x629101BC,
      Achievements_GetProgressByName    = 0x152663B1,
      Achievements_Unlock               = 0x593CCBDD,
      Entitlement_GetIsViewerEntitled   = 0x186B58B1,
      IAP_ConsumePurchase               = 0x1FBB72D9,
      IAP_GetProductsBySKU              = 0x7E9ACAF5,
      IAP_GetViewerPurchases            = 0x3A0F8419,
      IAP_LaunchCheckoutFlow            = 0x3F9B0D0D,
      Leaderboard_GetEntries            = 0x5DB3474C,
      Leaderboard_GetEntriesAfterRank   = 0x18378BEF,
      Leaderboard_GetNextEntries        = 0x4E207CD9,
      Leaderboard_GetPreviousEntries    = 0x4901DAC0,
      Leaderboard_WriteEntry            = 0x117FC8FE,
      Matchmaking_Browse                = 0x1E6532C8,
      Matchmaking_Cancel                = 0x206849AF,
      Matchmaking_Cancel2               = 0x10FE8DD4,
      Matchmaking_CreateAndEnqueueRoom  = 0x604C5DC8,
      Matchmaking_CreateRoom            = 0x033B132A,
      Matchmaking_Enqueue               = 0x40C16C71,
      Matchmaking_EnqueueRoom           = 0x708A4064,
      Matchmaking_JoinRoom              = 0x4D32D7FD,
      Matchmaking_ReportResultInsecure  = 0x1A36D18D,
      Matchmaking_StartMatch            = 0x44D40945,
      Room_CreateAndJoinPrivate         = 0x75D6E377,
      Room_Get                          = 0x659A8FB8,
      Room_GetCurrent                   = 0x09A6A504,
      Room_GetCurrentForUser            = 0x0E0017E5,
      Room_GetInvitableUsers            = 0x1E325792,
      Room_GetModeratedRooms            = 0x0983FD77,
      Room_InviteUser                   = 0x4129EC13,
      Room_Join                         = 0x16CA8F09,
      Room_KickUser                     = 0x49835736,
      Room_Leave                        = 0x72382475,
      Room_SetDescription               = 0x3044852F,
      Room_UpdateDataStore              = 0x026E4028,
      Room_UpdateOwner                  = 0x32B63D1D,
      Room_UpdatePrivateRoomJoinPolicy  = 0x1141029B,
      User_Get                          = 0x6BCF9E47,
      User_GetAccessToken               = 0x06A85ABE,
      User_GetLoggedInUser              = 0x436F345D,
      User_GetLoggedInUserFriends       = 0x587C2A8D,
      User_GetUserProof                 = 0x22810483,

      /// Indicates that a match has been found, for example after calling
      /// ovr_Matchmaking_Enqueue(). Use ovr_Message_GetRoom() to extract the
      /// matchmaking room.
      Notification_Matchmaking_MatchFound = 0x0BC3FCD7,

      /// Indicates that a connection has been established or there's been an error.
      /// Use ovr_NetworkingPeer_GetState() to get the result; as above,
      /// ovr_NetworkingPeer_GetID() returns the ID of the peer this message is for.
      Notification_Networking_ConnectionStateChange = 0x5E02D49A,

      /// Indicates that another user is attempting to establish a P2P connection
      /// with us. Use ovr_NetworkingPeer_GetID() to extract the ID of the peer.
      Notification_Networking_PeerConnectRequest = 0x4D31E2CF,

      /// Generated in response to ovr_Net_Ping().  Either contains ping time in
      /// microseconds or indicates that there was a timeout.
      Notification_Networking_PingResult = 0x51153012,

      /// Indicates that the user has accepted an invitation, for example in Oculus
      /// Home. Use ovr_Message_GetRoom() to extract the room that the user has been
      /// invited to. Note that you must call ovr_Room_Join() to actually join the
      /// room.
      Notification_Room_InviteAccepted = 0x6D1071B1,

      /// Indicates that the current room has been updated. Use ovr_Message_GetRoom()
      /// to extract the updated room.
      Notification_Room_RoomUpdate = 0x60EC3C2F,

    };

    public MessageType Type { get { return type; } }
    public bool IsError { get { return error != null; } }
    public ulong RequestID { get { return requestID; } }

    public virtual Error GetError() { return error; }
    public virtual UserProof GetUserProof() { return null; }
    public virtual Product GetProduct() { return null; }
    public virtual ProductList GetProductList() { return null; }
    public virtual string GetString() { return null; }
    public virtual Purchase GetPurchase() { return null; }
    public virtual PurchaseList GetPurchaseList() { return null; }
    public virtual User GetUser() { return null; }
    public virtual UserList GetUserList() { return null; }
    public virtual Room GetRoom() { return null; }
    public virtual RoomList GetRoomList() { return null; }
    public virtual NetworkingPeer GetNetworkingPeer() { return null; }
    public virtual AchievementDefinitionList GetAchievementDefinitions() {
      return null;
    }
    public virtual AchievementProgressList GetAchievementProgressList() {
      return null;
    }
    public virtual bool GetLeaderboardDidUpdate() { return false; }
    public virtual LeaderboardEntryList GetLeaderboardEntryList()
    {
      return null;
    }
    public virtual MatchmakingEnqueueResult GetMatchmakingEnqueueResult() {return null;}
    public virtual MatchmakingEnqueueResultAndRoom GetMatchmakingEnqueueResultAndRoom() {return null;}
    public virtual PingResult GetPingResult() { return null; }


    private MessageType type;
    private ulong requestID;
    private Error error;

    public static T Deserialize<T>(IntPtr c_message) {
      return Deserialize<T>(CAPI.ovr_Message_GetString(c_message));
    }

    public static T Deserialize<T>(string json) {
      return JsonConvert.DeserializeObject<T>(json);
    }

    internal static Message ParseMessageHandle(IntPtr messageHandle)
    {
      if (messageHandle.ToInt64() == 0)
      {
        return null;
      }

      Message message = null;
      Message.MessageType message_type = (Message.MessageType)CAPI.ovr_Message_GetType(messageHandle);
      switch (message_type)
      {
      // OVR_MESSAGE_TYPE_START
      case Message.MessageType.User_GetLoggedInUser: //intentional fallthrough
      case Message.MessageType.User_Get:
        message = new MessageWithUser(messageHandle);
        break;

      case Message.MessageType.Room_GetInvitableUsers: //intentional fallthrough
      case Message.MessageType.User_GetLoggedInUserFriends:
        message = new MessageWithUserList(messageHandle);
        break;

      case Message.MessageType.Room_GetCurrent:
      case Message.MessageType.Room_GetCurrentForUser:
        message = new MessageWithCurrentRoom(messageHandle);
        break;

      case Message.MessageType.IAP_GetViewerPurchases:
        message = new MessageWithPurchaseList(messageHandle);
        break;

      case Message.MessageType.IAP_GetProductsBySKU:
        message = new MessageWithProductList(messageHandle);
        break;

      case Message.MessageType.IAP_LaunchCheckoutFlow:
        message = new MessageWithPurchase(messageHandle);
        break;

      case Message.MessageType.IAP_ConsumePurchase:
        message = new Message(messageHandle);
        break;

      case Message.MessageType.Room_Get:
        message = new MessageWithRoom(messageHandle);
        break;

      case Message.MessageType.Room_InviteUser: //intentional fallthrough
      case Message.MessageType.Room_Join:
      case Message.MessageType.Room_Leave:
      case Message.MessageType.Room_KickUser:
      case Message.MessageType.Room_SetDescription:
      case Message.MessageType.Room_UpdateDataStore:
      case Message.MessageType.Room_CreateAndJoinPrivate:
      case Message.MessageType.Room_UpdateOwner:
      case Message.MessageType.Notification_Room_RoomUpdate:
      case Message.MessageType.Matchmaking_JoinRoom:
      case Message.MessageType.Room_UpdatePrivateRoomJoinPolicy:
        message = new MessageWithViewerRoom(messageHandle);
        break;

      case Message.MessageType.Matchmaking_Browse:
      case Message.MessageType.Room_GetModeratedRooms:
        message = new MessageWithRoomList(messageHandle);
        break;

      case Message.MessageType.Matchmaking_CreateAndEnqueueRoom:
        message = new MessageWithMatchmakingEnqueueResultAndRoom(messageHandle);
        break;

      case Message.MessageType.Matchmaking_CreateRoom:
        message = new MessageWithViewerRoom(messageHandle);
        break;

      case Message.MessageType.Matchmaking_Enqueue:
      case Message.MessageType.Matchmaking_EnqueueRoom:
        message = new MessageWithMatchmakingEnqueueResult(messageHandle);
        break;

      case Message.MessageType.Matchmaking_Cancel:
      case Message.MessageType.Matchmaking_Cancel2:
        message = new Message(messageHandle);
        break;

      case Message.MessageType.Matchmaking_StartMatch:
      case Message.MessageType.Matchmaking_ReportResultInsecure:
        message = new Message(messageHandle);
        break;

      case Message.MessageType.Notification_Matchmaking_MatchFound:
        message = new MessageWithMatchMadeRoom(messageHandle);
        break;

      case Message.MessageType.Notification_Networking_PeerConnectRequest:
        message = new MessageWithNetworkingPeer(messageHandle);
        break;

      case Message.MessageType.Notification_Networking_ConnectionStateChange:
        message = new MessageWithNetworkingPeer(messageHandle);
        break;

      case Message.MessageType.Notification_Networking_PingResult:
        message = new MessageWithPingResult(messageHandle);
        break;

      case Message.MessageType.Achievements_GetDefinitionsByName:
      case Message.MessageType.Achievements_GetAllDefinitions:
        message = new MessageWithAchievementDefinitionList(messageHandle);
        break;

      case Message.MessageType.User_GetUserProof:
        message = new MessageWithUserProof(messageHandle);
        break;

      case Message.MessageType.Achievements_GetAllProgress:
      case Message.MessageType.Achievements_GetProgressByName:
        message = new MessageWithAchievementProgressList(messageHandle);
        break;

      case Message.MessageType.Achievements_Unlock: // intentional fallthrough
      case Message.MessageType.Achievements_AddCount:
      case Message.MessageType.Achievements_AddFields:
        message = new Message(messageHandle);
        break;

      case Message.MessageType.Leaderboard_WriteEntry:
        message = new MessageWithLeaderboardDidUpdate(messageHandle);
        break;

      case Message.MessageType.Leaderboard_GetEntries:
      case Message.MessageType.Leaderboard_GetNextEntries:
      case Message.MessageType.Leaderboard_GetPreviousEntries:
      case Message.MessageType.Leaderboard_GetEntriesAfterRank:
        message = new MessageWithLeaderboardEntries(messageHandle);
        break;

      case Message.MessageType.Entitlement_GetIsViewerEntitled:
        message = new Message(messageHandle);
        break;

      case Message.MessageType.Notification_Room_InviteAccepted:
      case Message.MessageType.User_GetAccessToken:
        // TODO This is an ID and should be exposed as such, rather than a raw string
        message = new MessageWithString(messageHandle);
        break;

      default:
        if (HandleExtraMessageTypes != null)
        {
          message = HandleExtraMessageTypes(messageHandle, message_type);
        }
        if (message == null)
        {
          Debug.LogError(string.Format("Unrecognized message type {0}\n", message_type));
        }
        break;

        // OVR_MESSAGE_TYPE_END
      }

      return message;
    }

    public static Message PopMessage()
    {
      if (!Core.IsInitialized())
      {
        return null;
      }

      var messageHandle = CAPI.ovr_PopMessage();

      Message message = ParseMessageHandle(messageHandle);

      CAPI.ovr_FreeMessage(messageHandle);
      return message;
    }

    internal delegate Message ExtraMessageTypesHandler(IntPtr messageHandle, Message.MessageType message_type);
    internal static ExtraMessageTypesHandler HandleExtraMessageTypes { set; private get; }
  }

  public class MessageWithString : Message<string>
  {
    public MessageWithString(IntPtr c_message) : base(c_message) { }
    protected override string GetDataFromMessage(IntPtr c_message)
    {
      return CAPI.ovr_Message_GetString(c_message);
    }
    public override string GetString() {return Data;}
  }

  public class MessageWithUserProof : Message<UserProof>
  {
    public MessageWithUserProof(IntPtr c_message) : base(c_message) { }
    protected override UserProof GetDataFromMessage(IntPtr c_message)
    {
      return Deserialize<UserProof>(c_message);
    }
    public override UserProof GetUserProof() { return Data; }
  }

  public class MessageWithProduct : Message<Product>
  {
    public MessageWithProduct(IntPtr c_message) : base(c_message) { }
    protected override Product GetDataFromMessage(IntPtr c_message)
    {
      return Deserialize<Product>(c_message);
    }
    public override Product GetProduct() { return Data; }
  }

  public class MessageWithPurchase : Message<Purchase>
  {
    public MessageWithPurchase(IntPtr c_message) : base(c_message) { }
    protected override Purchase GetDataFromMessage(IntPtr c_message)
    {
      return Deserialize<Purchase>(c_message);
    }
    public override Purchase GetPurchase() { return Data; }
  }

  public class MessageWithProductList : Message<ProductList>
  {
    public MessageWithProductList(IntPtr c_message) : base(c_message) { }
    protected override ProductList GetDataFromMessage(IntPtr c_message)
    {
      return Deserialize<ProductList>(c_message);
    }
    public override ProductList GetProductList() { return Data; }
  }

  public class MessageWithPurchaseList : Message<PurchaseList>
  {
    public MessageWithPurchaseList(IntPtr c_message) : base(c_message) { }
    protected override PurchaseList GetDataFromMessage(IntPtr c_message)
    {
      return Deserialize<PurchaseList>(c_message);
    }
    public override PurchaseList GetPurchaseList() { return Data; }
  }

  public class MessageWithUser : Message<User>
  {
    public MessageWithUser(IntPtr c_message) : base(c_message) { }
    protected override User GetDataFromMessage(IntPtr c_message)
    {
      return Deserialize<User>(c_message);
    }
    public override User GetUser() { return Data; }
  }

  public class MessageWithUserList : Message<UserList>
  {
    public MessageWithUserList(IntPtr c_message) : base(c_message) { }
    protected override UserList GetDataFromMessage(IntPtr c_message)
    {
      return Deserialize<UserList>(c_message);
    }
    public override UserList GetUserList() { return Data; }
  }

  public class MessageWithNetworkingPeer : Message<NetworkingPeer>
  {
    public MessageWithNetworkingPeer(IntPtr c_message) : base(c_message) { }
    protected override NetworkingPeer GetDataFromMessage(IntPtr c_message)
    {
      var peer = CAPI.ovr_Message_GetNetworkingPeer(c_message);
      return new NetworkingPeer(
        CAPI.ovr_NetworkingPeer_GetID(peer),
        (PeerConnectionState)CAPI.ovr_NetworkingPeer_GetState(peer)
      );
    }
    public override NetworkingPeer GetNetworkingPeer() { return Data; }
  }

  public class MessageWithPingResult : Message<PingResult>
  {
    public MessageWithPingResult(IntPtr c_message) : base(c_message) { }
    protected override PingResult GetDataFromMessage(IntPtr c_message)
    {
      var peer = CAPI.ovr_Message_GetPingResult(c_message);
      bool is_timeout = CAPI.ovr_PingResult_IsTimeout(c_message);
      return new PingResult(
        CAPI.ovr_PingResult_GetID(peer),
        is_timeout ? (UInt64?)null : CAPI.ovr_PingResult_GetPingTimeUsec(c_message)
      );
    }
    public override PingResult GetPingResult() { return Data; }
  }

	public class MessageWithAchievementDefinitionList
    : Message<AchievementDefinitionList>
	{
    public MessageWithAchievementDefinitionList(IntPtr c_message) : base(c_message) { }
    protected override AchievementDefinitionList GetDataFromMessage(IntPtr c_message)
		{
      return Deserialize<AchievementDefinitionList>(c_message);
		}

		public override AchievementDefinitionList GetAchievementDefinitions() {
      return Data;
    }
	}

	public class MessageWithAchievementProgressList
    : Message<AchievementProgressList>
	{
		public MessageWithAchievementProgressList(IntPtr c_message) : base(c_message) { }

    protected override AchievementProgressList GetDataFromMessage(IntPtr c_message)
    {
      return Deserialize<AchievementProgressList>(c_message);
    }

		public override AchievementProgressList GetAchievementProgressList() {
      return Data;
    }
	}

  public class MessageWithRoom : Message<Room>
  {
    public MessageWithRoom(IntPtr c_message) : base(c_message) { }
    protected override Room GetDataFromMessage(IntPtr c_message)
    {
      return Deserialize<Room>(c_message);
    }

    public override Room GetRoom() { return Data; }
  }

  public class MessageWithRoomList : Message<RoomList>
  {
    public MessageWithRoomList(IntPtr c_message) : base(c_message) { }
    protected override RoomList GetDataFromMessage(IntPtr c_message)
    {
      return Deserialize<RoomList>(c_message);
    }

    public override RoomList GetRoomList() { return Data; }
  }

  public class MessageWithCurrentRoom : Message<Room>
  {
    public MessageWithCurrentRoom(IntPtr c_message) : base(c_message) { }
    protected override Room GetDataFromMessage(IntPtr c_message)
    {
      return Deserialize<CurrentRoom>(c_message).Room;
    }

    private class CurrentRoom
    {
      [JsonProperty("current_room")]
      public Room Room;
    }

    public override Room GetRoom() { return Data; }
  }

  public class MessageWithViewerRoom : Message<Room>
  {
    public MessageWithViewerRoom(IntPtr c_message) : base(c_message) { }
    protected override Room GetDataFromMessage(IntPtr c_message)
    {
      return Deserialize<ViewerRoom>(c_message).Room;
    }

    private class ViewerRoom
    {
      [JsonProperty("viewer_room")]
      public Room Room;
    }

    public override Room GetRoom() { return Data; }
  }

  public class MessageWithMatchMadeRoom : Message<Room>
  {
    public MessageWithMatchMadeRoom(IntPtr c_message) : base(c_message) { }
    protected override Room GetDataFromMessage(IntPtr c_message)
    {
      return Deserialize<MatchMadeRoom>(c_message).Room;
    }

    private class MatchMadeRoom
    {
      [JsonProperty("matchmade_room")]
      public Room Room;
    }

    public override Room GetRoom() { return Data; }
  }

  public class MessageWithLeaderboardDidUpdate : Message<bool>
  {
    public MessageWithLeaderboardDidUpdate(IntPtr c_message) : base(c_message) { }
    protected override bool GetDataFromMessage(IntPtr c_message)
    {
      return Deserialize<LeaderboardUpdate>(c_message).DidUpdate;
    }

    private class LeaderboardUpdate
    {
      [JsonProperty("did_update")]
      public bool DidUpdate;
    }

    public override bool GetLeaderboardDidUpdate() { return Data; }
  }

  public class MessageWithLeaderboardEntries : Message<LeaderboardEntryList>
  {
    public MessageWithLeaderboardEntries(IntPtr c_message) : base(c_message) { }
    protected override LeaderboardEntryList GetDataFromMessage(IntPtr c_message)
    {
      return Deserialize<LeaderboardEntryList>(c_message);
    }

    public override LeaderboardEntryList GetLeaderboardEntryList() { return Data; }
  }

  public class MessageWithMatchmakingEnqueueResult : Message<MatchmakingEnqueueResult>
  {
    public MessageWithMatchmakingEnqueueResult(IntPtr c_message) : base(c_message) { }

    protected override MatchmakingEnqueueResult GetDataFromMessage(IntPtr c_message)
    {
      return Deserialize<MatchmakingEnqueueResult>(c_message);
    }

    public override MatchmakingEnqueueResult GetMatchmakingEnqueueResult() {return Data;}
  }

  public class MessageWithMatchmakingEnqueueResultAndRoom : Message<MatchmakingEnqueueResultAndRoom>
  {
    public MessageWithMatchmakingEnqueueResultAndRoom(IntPtr c_message) : base(c_message) { }

    protected override MatchmakingEnqueueResultAndRoom GetDataFromMessage(IntPtr c_message)
    {
      return Deserialize<MatchmakingEnqueueResultAndRoom>(c_message);
    }

    public override MatchmakingEnqueueResultAndRoom GetMatchmakingEnqueueResultAndRoom() {return Data;}
  }
}
