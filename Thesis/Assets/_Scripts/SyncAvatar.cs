
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Photon.Pun;
/// <summary>
/// This script is responsible for transmitting the ovravatar packets to all players
/// </summary>
public class SyncAvatar : MonoBehaviour, IPunObservable {

    private PhotonView photonView;
    public OvrAvatar ovrAvatar;
    public OvrAvatarRemoteDriver remoteDriver;

    private List<byte[]> packetData;
    bool sync = false;
    public void Start() {
        photonView = GetComponent<PhotonView>();
        //start this 3 seconds into the game as there can be a bug where it starts before the other players initialize theirs 
        Invoke("StartSync", 3);

    }
    void StartSync() {
        sync = true;
        if (photonView.IsMine) {
            //start recording if you are the local player
            packetData = new List<byte[]>();
            ovrAvatar.RecordPackets = true;
            ovrAvatar.PacketRecorded += OnLocalAvatarPacketRecorded;
        } else {
        }
    }
    public void OnDisable() {
        if (photonView != null) {

            if (photonView.IsMine) {
                ovrAvatar.RecordPackets = false;
                ovrAvatar.PacketRecorded -= OnLocalAvatarPacketRecorded;
            }
        }
    }

    private int localSequence;

    public void OnLocalAvatarPacketRecorded(object sender, OvrAvatar.PacketEventArgs args) {
        if (!PhotonNetwork.InRoom) {
            return;
        }
        using (MemoryStream outputStream = new MemoryStream()) {
            BinaryWriter writer = new BinaryWriter(outputStream);
            var size = Oculus.Avatar.CAPI.ovrAvatarPacket_GetSize(args.Packet.ovrNativePacket);
            byte[] data = new byte[size];
            Oculus.Avatar.CAPI.ovrAvatarPacket_Write(args.Packet.ovrNativePacket, size, data);

            writer.Write(localSequence++);
            writer.Write(size);
            writer.Write(data);

            packetData.Add(outputStream.ToArray());
        }
    }
    private void DeserializeAndQueuePacketData(byte[] data) {
        using (MemoryStream inputStream = new MemoryStream(data)) {
            BinaryReader reader = new BinaryReader(inputStream);
            int remoteSequence = reader.ReadInt32();

            int size = reader.ReadInt32();
            byte[] sdkData = reader.ReadBytes(size);

            System.IntPtr packet = Oculus.Avatar.CAPI.ovrAvatarPacket_Read((System.UInt32)data.Length, sdkData);
            remoteDriver.QueuePacket(remoteSequence, new OvrAvatarPacket { ovrNativePacket = packet });
        }
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (!sync) return;

        if (stream.IsWriting) {
            if (packetData.Count == 0) {
                stream.SendNext(0);
                return;
            }

            stream.SendNext(packetData.Count);

            foreach (byte[] b in packetData) {
                stream.SendNext(b);
            }

            packetData.Clear();
        }

        if (stream.IsReading) {
            if (stream.PeekNext() != null) {
                int num = (int)stream.ReceiveNext();
                for (int counter = 0; counter < num; ++counter) {
                    byte[] data = (byte[])stream.ReceiveNext();
                    DeserializeAndQueuePacketData(data);
                }
            }
        }
    }
}