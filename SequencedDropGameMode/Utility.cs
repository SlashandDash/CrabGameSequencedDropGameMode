using BepInEx.IL2CPP.Utils;
using SteamworksNative;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SequencedDropGameMode
{
    internal static class Utility
    {
        internal static int MaxChatMessageLength
            => GameUiChatBox.Instance != null ? GameUiChatBox.Instance.field_Private_Int32_0 : 80;

        internal static string FormatMessage(string str)
            => Regex.Replace(
                str,
                "(.)(?<=\\1{5})", // Remove repeating characters (5 or more will truncate to 4, allowing it to appear in chat)
                string.Empty,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled
             );

        internal enum MessageType
        {
            Normal,
            Server,
            Styled
        }
        internal static void SendMessage(ulong recipientClientId, string message, MessageType messageType = MessageType.Server, string displayName = null)
            => SendMessage(message, messageType, displayName, [recipientClientId]);
        internal static void SendMessage(string message, MessageType messageType = MessageType.Server, string displayName = null, IEnumerable<ulong> recipientClientIds = null)
        {
            ulong senderClientId = 0UL;
            message ??= string.Empty;
            message = FormatMessage(message);
            if (messageType == MessageType.Server)
            {
                displayName = string.Empty;
                senderClientId = 1UL;
            }
            else
                displayName ??= string.Empty;

            List<byte> bytes = [];
            bytes.AddRange(BitConverter.GetBytes((int)ServerSendType.sendMessage));
            bytes.AddRange(BitConverter.GetBytes(senderClientId));

            bytes.AddRange(BitConverter.GetBytes(displayName.Length));
            bytes.AddRange(Encoding.ASCII.GetBytes(displayName));

            bytes.AddRange(BitConverter.GetBytes(message.Length));
            bytes.AddRange(Encoding.ASCII.GetBytes(message));

            bytes.InsertRange(0, BitConverter.GetBytes(bytes.Count));

            Packet packet = new();
            packet.field_Private_List_1_Byte_0 = new();
            foreach (byte b in bytes)
                packet.field_Private_List_1_Byte_0.Add(b);

            foreach (ulong clientId in recipientClientIds ?? [.. LobbyManager.steamIdToUID.Keys])
            {
                if (messageType == MessageType.Styled)
                {
                    byte[] clientIdBytes = BitConverter.GetBytes(clientId);
                    for (int i = 0; i < clientIdBytes.Length; i++)
                        packet.field_Private_List_1_Byte_0[i + 8] = clientIdBytes[i];
                }
                SteamPacketManager.SendPacket(new CSteamID(clientId), packet, 8, SteamPacketDestination.ToClient);
            }
        }

        internal static void GiveItem(ulong clientId, int itemId, int ammo = -1)
        {
            if (!GameManager.Instance.activePlayers.ContainsKey(clientId) || GameManager.Instance.activePlayers[clientId].dead)
                if (!ItemManager.idToItem.ContainsKey(itemId)) return;

            if (ItemManager.idToItem[itemId].type == ItemType.Other || ItemManager.idToItem[itemId].type == ItemType.Ammo)
            {
                ServerSend.DropItem(clientId, itemId, SharedObjectManager.Instance.GetNextId(), ammo == -1 ? ItemManager.idToItem[itemId].maxAmmo : ammo);
                return;
            }

            if (ItemManager.idToItem[itemId].type == ItemType.Melee || ItemManager.idToItem[itemId].type == ItemType.Throwable || ammo == -1 || ammo == ItemManager.idToItem[itemId].maxAmmo)
            {
                GameServer.ForceGiveWeapon(clientId, itemId, SharedObjectManager.Instance.GetNextId());
                return;
            }

            LobbyManager.Instance.StartCoroutine(GiveItemCoroutine(clientId, itemId, ammo == -1 ? ItemManager.idToItem[itemId].maxAmmo : ammo));
        }
        internal static IEnumerator GiveItemCoroutine(ulong clientId, int itemId, int ammo)
        {
            int uniqueObjectId = SharedObjectManager.Instance.GetNextId();
            ServerSend.DropItem(clientId, itemId, uniqueObjectId, ammo);
            if (ItemManager.idToItem[itemId].type == ItemType.Other || ItemManager.idToItem[itemId].type == ItemType.Ammo) yield break;

            while (!SharedObjectManager.Instance.field_Private_Dictionary_2_Int32_MonoBehaviourPublicInidBoskUnique_0.ContainsKey(uniqueObjectId)) yield return new WaitForEndOfFrame();
            if (!LobbyManager.steamIdToUID.ContainsKey(clientId) || !SharedObjectManager.Instance.field_Private_Dictionary_2_Int32_MonoBehaviourPublicInidBoskUnique_0.ContainsKey(uniqueObjectId)) yield break;

            yield return new WaitForSeconds(Mathf.Min((LobbyManager.Instance.field_Private_ArrayOf_ObjectPublicBoInBoCSItBoInSiBySiUnique_0[LobbyManager.steamIdToUID[clientId]].field_Public_Int32_0 + 50) / 1000f, 1));
            if (!LobbyManager.steamIdToUID.ContainsKey(clientId) || !SharedObjectManager.Instance.field_Private_Dictionary_2_Int32_MonoBehaviourPublicInidBoskUnique_0.ContainsKey(uniqueObjectId)) yield break;

            Packet packet = new(uniqueObjectId);
            packet.field_Private_ArrayOf_Byte_0 = BitConverter.GetBytes(uniqueObjectId);

            ServerHandle.TryInteract(clientId, packet);
        }
    }
}