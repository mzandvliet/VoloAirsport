using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#pragma warning disable 414
namespace Oculus.Platform
{
  public class CAPI
  {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
  #if UNITY_64 || UNITY_EDITOR_64
    public const string DLL_NAME = "LibOVRPlatform64_1";
  #else
    public const string DLL_NAME = "LibOVRPlatform32_1";
  #endif
#else
    public const string DLL_NAME = "ovrplatform";
#endif

    public static string ovr_Message_GetString(IntPtr message)
    {
      return Marshal.PtrToStringAnsi(ovr_Message_GetString_Unsafe(message));
    }

    public static string ovr_Error_GetMessage(IntPtr message)
    {
      return Marshal.PtrToStringAnsi(ovr_Error_GetMessage_Unsafe(message));
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ovrKeyValuePair {
      public ovrKeyValuePair(string key, string value) {
        key_ = key;
        valueType_ = KeyValuePairType.String;
        stringValue_ = value;

        intValue_ = 0;
        doubleValue_ = 0.0;
      }

      public ovrKeyValuePair(string key, int value) {
        key_ = key;
        valueType_ = KeyValuePairType.Int;
        intValue_ = value;

        stringValue_ = null;
        doubleValue_ = 0.0;
      }

      public ovrKeyValuePair(string key, double value) {
        key_ = key;
        valueType_ = KeyValuePairType.Double;
        doubleValue_ = value;

        stringValue_ = null;
        intValue_ = 0;
      }

      public string key_;
      KeyValuePairType valueType_;

      public string stringValue_;
      public int intValue_;
      public double doubleValue_;
    };

    public static IntPtr ArrayOfStructsToIntPtr(Array ar)
    {
      int totalSize = 0;
      for(int i=0; i<ar.Length; i++) {
        totalSize += Marshal.SizeOf(ar.GetValue(i));
      }

      IntPtr childrenPtr = Marshal.AllocHGlobal(totalSize);
      IntPtr curr = childrenPtr;
      for(int i=0; i<ar.Length; i++) {
        Marshal.StructureToPtr(ar.GetValue(i), curr, false);
        curr = (IntPtr)((long)curr + Marshal.SizeOf(ar.GetValue(i)));
      }
      return childrenPtr;
    }

    public static CAPI.ovrKeyValuePair[] DictionaryToOVRKeyValuePairs(Dictionary<string, object> dict)
    {
      if(dict == null || dict.Count == 0)
      {
        return null;
      }

      var nativeCustomData = new CAPI.ovrKeyValuePair[dict.Count];

      int i = 0;
      foreach(var item in dict)
      {
        if(item.Value.GetType() == typeof(int))
        {
          nativeCustomData[i] = new CAPI.ovrKeyValuePair(item.Key, (int)item.Value);
        }
        else if(item.Value.GetType() == typeof(string))
        {
          nativeCustomData[i] = new CAPI.ovrKeyValuePair(item.Key, (string)item.Value);
        }
        else if(item.Value.GetType() == typeof(double))
        {
          nativeCustomData[i] = new CAPI.ovrKeyValuePair(item.Key, (double)item.Value);
        }
        else
        {
          throw new Exception("Only int, double or string are allowed types in CustomQuery.data");
        }
        i++;
      }
      return nativeCustomData;
    }

    public static byte[] IntPtrToByteArray(IntPtr data, ulong size)
    {
      byte[] outArray = new byte[size];
      Marshal.Copy(data, outArray, 0, (int)size);
      return outArray;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ovrMatchmakingCriterion {
      public ovrMatchmakingCriterion(string key, MatchmakingCriterionImportance importance)
      {
        key_ = key;
        importance_ = importance;

        parameterArray = IntPtr.Zero;
        parameterArrayCount = 0;
      }

      public string key_;
      public MatchmakingCriterionImportance importance_;

      public IntPtr parameterArray;
      public uint parameterArrayCount;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ovrMatchmakingCustomQueryData {
      public IntPtr dataArray;
      public uint dataArrayCount;

      public IntPtr criterionArray;
      public uint criterionArrayCount;
    };



    //Init
    [DllImport (DLL_NAME)]
    public static extern bool ovr_UnityInitWrapper(string appId);

    [DllImport (DLL_NAME)]
    public static extern bool ovr_UnityInitWrapperStandalone(string accessToken, IntPtr loggingCB);

    [DllImport (DLL_NAME)]
    public static extern bool ovr_UnityInitWrapperWindows(string appId, IntPtr loggingCB);

    [DllImport (DLL_NAME)]
    public static extern bool ovr_SetDeveloperAccessToken(string accessToken);

    //Message queue access
    [DllImport (DLL_NAME)]
    public static extern IntPtr ovr_PopMessage();

    [DllImport (DLL_NAME)]
    public static extern void ovr_FreeMessage(IntPtr message);

    //Message field access
    [DllImport (DLL_NAME)]
    public static extern uint ovr_Message_GetType(IntPtr message);

    [DllImport (DLL_NAME)]
    public static extern bool ovr_Message_IsError(IntPtr message);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_User_GetAccessToken();

    [DllImport (DLL_NAME)]
    public static extern IntPtr ovr_Message_GetError(IntPtr message);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Message_GetRequestID(IntPtr message);

    [DllImport (DLL_NAME, EntryPoint = "ovr_Message_GetString", CharSet = CharSet.Unicode)]
    private static extern IntPtr ovr_Message_GetString_Unsafe(IntPtr message);

    [DllImport (DLL_NAME)]
    public static extern IntPtr ovr_Message_GetNetworkingPeer(IntPtr message);

    [DllImport (DLL_NAME)]
    public static extern IntPtr ovr_Message_GetPingResult(IntPtr message);

    [DllImport (DLL_NAME)]
    public static extern uint ovr_NetworkingPeer_GetState(IntPtr networkingPeer);

    [DllImport (DLL_NAME)]
    public static extern uint ovr_NetworkingPeer_GetSendPolicy(IntPtr networkingPeer);

    [DllImport (DLL_NAME)]
    public static extern UInt64 ovr_NetworkingPeer_GetID(IntPtr networkingPeer);

    [DllImport (DLL_NAME)]
    public static extern UInt64 ovr_PingResult_GetID(IntPtr pingResult);

    [DllImport (DLL_NAME)]
    public static extern UInt64 ovr_PingResult_GetPingTimeUsec(IntPtr pingResult);

    [DllImport (DLL_NAME)]
    public static extern bool ovr_PingResult_IsTimeout(IntPtr pingResult);

    //Packet field access
    [DllImport (DLL_NAME)]
    public static extern UInt64 ovr_Packet_GetSenderID(IntPtr packet);

    [DllImport (DLL_NAME)]
    public static extern uint ovr_Packet_GetSize(IntPtr packet);

    [DllImport (DLL_NAME)]
    public static extern IntPtr ovr_Packet_GetBytes(IntPtr packet);

    [DllImport (DLL_NAME)]
    public static extern uint ovr_Packet_GetSendPolicy(IntPtr packet);

    //Error field access
    [DllImport (DLL_NAME)]
    public static extern int ovr_Error_GetCode(IntPtr error);

    [DllImport (DLL_NAME)]
    public static extern int ovr_Error_GetHttpCode(IntPtr error);

    [DllImport (DLL_NAME, EntryPoint = "ovr_Error_GetMessage", CharSet = CharSet.Unicode)]
    private static extern IntPtr ovr_Error_GetMessage_Unsafe(IntPtr error);

    //Requests
    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Entitlement_GetIsViewerEntitled();

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_HTTP_GetWithMessageType(string url, int messageType);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_User_Get(UInt64 userID);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_User_GetLoggedInUser();

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_User_GetFriends(UInt64 userID);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_User_GetLoggedInUserFriends();

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_User_GetUserProof();

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Room_Get(UInt64 id);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Room_Leave(UInt64 roomID);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Room_KickUser(UInt64 roomID, UInt64 userID, int kickDurationSeconds);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Room_Join(UInt64 roomID, bool subscribeToUpdates);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Room_CreateAndJoinPrivate(uint joinPolicy, uint max_users, bool subscribeToUpdates);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Matchmaking_CreateAndEnqueueRoom(
      string pool,
      uint maxUsers,
      bool subscribeToUpdates,
      IntPtr customQuery
    );

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Matchmaking_CreateRoom(
      string pool,
      uint maxUsers,
      bool subscribeToUpdates
    );

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Matchmaking_Browse(string pool, IntPtr customQuery);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Matchmaking_Enqueue(string pool, IntPtr customQuery);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Matchmaking_EnqueueRoom(UInt64 roomID, IntPtr customQuery);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Matchmaking_ReportResultInsecure(UInt64 roomID, ovrKeyValuePair[] data, uint numItems);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Matchmaking_StartMatch(UInt64 roomID);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Matchmaking_Cancel(string pool, string traceID);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Matchmaking_Cancel2();

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Matchmaking_JoinRoom(UInt64 roomID, bool subscribeToUpdates);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Room_SetDescription(UInt64 roomID, string description);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Room_UpdatePrivateRoomJoinPolicy(UInt64 roomID, uint newJoinPolicy);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Room_UpdateDataStore(UInt64 roomID, ovrKeyValuePair[] data, uint numItems);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Room_GetCurrent();

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Room_GetCurrentForUser(UInt64 userID);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Room_GetInvitableUsers();

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Room_InviteUser(UInt64 roomID, string inviteToken);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Room_UpdateOwner(UInt64 roomID, UInt64 userID);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Room_GetModeratedRooms();

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_IAP_GetViewerPurchases();

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_IAP_GetProductsBySKU(string[] skus, int count);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_IAP_ConsumePurchase(string sku);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_IAP_LaunchCheckoutFlow(string offerID);

    [DllImport (DLL_NAME)]
    public static extern bool ovr_Net_SendPacket(UInt64 userID, uint length, byte[] bytes, SendPolicy policy);

    [DllImport (DLL_NAME)]
    public static extern void ovr_Net_Connect (UInt64 userID);

    [DllImport (DLL_NAME)]
    public static extern void ovr_Net_Accept(UInt64 peerID);

    [DllImport (DLL_NAME)]
    public static extern void ovr_Net_Close(UInt64 peerID);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Net_Ping (UInt64 peerID);

    [DllImport (DLL_NAME)]
    public static extern IntPtr ovr_Net_ReadPacket();

    [DllImport (DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ovr_Packet_Free(IntPtr packet);

    [DllImport (DLL_NAME)]
    public static extern void ovr_CrashApplication();

    [DllImport(DLL_NAME)]
    public static extern ulong ovr_Achievements_GetAllDefinitions();

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Achievements_GetDefinitionsByName(string[] names, int count);

    [DllImport(DLL_NAME)]
    public static extern ulong ovr_Achievements_GetAllProgress();

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Achievements_GetProgressByName(string[] names, int count);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Achievements_Unlock(string name);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Achievements_AddCount(string name, ulong count);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Achievements_AddFields(string name, string fields);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Leaderboard_WriteEntry(string leaderboardName, long score, byte[] extraData, uint extraDataLength, bool forceUpdate);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Leaderboard_GetEntries(string leaderboardName, int limit, LeaderboardFilterType filter, LeaderboardStartAt startAt);

    [DllImport (DLL_NAME)]
    public static extern ulong ovr_Leaderboard_GetEntriesAfterRank(string leaderboardName, int limit, ulong afterRank);

    [DllImport (DLL_NAME)]
    public static extern IntPtr ovr_Voip_CreateEncoder();

    [DllImport (DLL_NAME)]
    public static extern IntPtr ovr_Voip_CreateDecoder();

    [DllImport (DLL_NAME)]
    public static extern void ovr_VoipDecoder_ClearDecodedData(IntPtr obj);

    [DllImport (DLL_NAME)]
    public static extern void ovr_VoipDecoder_Decode(IntPtr obj, byte[] compressedData, ulong compressedSize);

    [DllImport (DLL_NAME)]
    public static extern IntPtr ovr_VoipDecoder_GetDecodedData(IntPtr obj);

    [DllImport (DLL_NAME)]
    public static extern UIntPtr ovr_VoipDecoder_GetDecodedDataSize(IntPtr obj);

    [DllImport (DLL_NAME)]
    public static extern void ovr_VoipEncoder_AddPCM(IntPtr obj, float[] inputData, uint inputSize);

    [DllImport (DLL_NAME)]
    public static extern void ovr_VoipEncoder_ClearOutputBuffer(IntPtr obj);

    [DllImport (DLL_NAME)]
    public static extern IntPtr ovr_VoipEncoder_GetCompressedData(IntPtr obj);

    [DllImport (DLL_NAME)]
    public static extern UIntPtr ovr_VoipEncoder_GetCompressedDataSize(IntPtr obj);
  }
}
