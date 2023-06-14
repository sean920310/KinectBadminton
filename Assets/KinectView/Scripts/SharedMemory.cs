using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

public class SharedMemory : MonoBehaviour
{
    [SerializeField] PlayerMovement playerMovement1;
    [SerializeField] PlayerMovement playerMovement2;
    private PositioningManager.PlayerCount m_GameMode;
    public struct AudioInfo
    {
        public byte player;
        public float beamAngleInDeg;
        public float energy;

        public AudioInfo(byte player, float beamAngleInDeg, float energy)
        {
            this.player = player;
            this.beamAngleInDeg = beamAngleInDeg;
            this.energy = energy;
        }
    }

    // Define the shared memory size
    private const int SharedMemorySize = 1024;

    // Define the name of the shared memory
    private const string SharedMemoryName = "AudioSharedMemory";

    // The shared memory object
    private MemoryMappedFile sharedMemory;

    // Start is called before the first frame update
    void Start()
    {
        // Create or open the shared memory
        sharedMemory = MemoryMappedFile.CreateOrOpen(SharedMemoryName, SharedMemorySize);

        // Write data to the shared memory
        WriteDataToSharedMemory("Hello, shared memory!");

        // Read data from the shared memory
        string data = ReadDataFromSharedMemory();
        Debug.Log("Data read from shared memory: " + data);
    }

    // Update is called once per frame
    void Update()
    {
        string data = ReadDataFromSharedMemory();
        if(data.Length != 0)
        {
            print(data);
            switch (m_GameMode)
            {
                case PositioningManager.PlayerCount.Solo:
                    if (data.Contains("player1") || data.Contains("player2"))
                    {
                        playerMovement1.OnKinectSkill();
                    }
                    break;
                case PositioningManager.PlayerCount.Dual:
                    if (data.Contains("player1"))
                    {
                        playerMovement1.OnKinectSkill();
                    }
                    else if (data.Contains("player2"))
                    {
                        playerMovement2.OnKinectSkill();
                    }
                    break;
                case PositioningManager.PlayerCount.AllBots:
                    break;
                default:
                    break;
            }
            
        }
    }

    // Write data to the shared memory
    void WriteDataToSharedMemory(string data)
    {
        using (MemoryMappedViewAccessor accessor = sharedMemory.CreateViewAccessor())
        {
            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(data);
            accessor.WriteArray(0, buffer, 0, buffer.Length);
        }
    }

    // Read data from the shared memory
    string ReadDataFromSharedMemory()
    {
        using (MemoryMappedViewAccessor accessor = sharedMemory.CreateViewAccessor())
        {
            byte[] buffer = new byte[SharedMemorySize];
            accessor.ReadArray(0, buffer, 0, buffer.Length);
            string data = System.Text.Encoding.ASCII.GetString(buffer);
            return data;
        }
    }
    byte[] ReadDataFromSharedMemoryInByteArr()
    {
        using (MemoryMappedViewAccessor accessor = sharedMemory.CreateViewAccessor())
        {
            byte[] buffer = new byte[SharedMemorySize];
            accessor.ReadArray(0, buffer, 0, buffer.Length);
            return buffer;
        }
    }
    // Clean up the shared memory object
    void OnDestroy()
    {
        if (sharedMemory != null)
        {
            sharedMemory.Dispose();
        }
    }

}
// reference: https://gist.github.com/13xforever/2835844
public static class CastingHelper
{
    public static T CastToStruct<T>(this byte[] data) where T : struct
    {
        var pData = GCHandle.Alloc(data, GCHandleType.Pinned);
        var result = (T)Marshal.PtrToStructure(pData.AddrOfPinnedObject(), typeof(T));
        pData.Free();
        return result;
    }

    public static byte[] CastToArray<T>(this T data) where T : struct
    {
        var result = new byte[Marshal.SizeOf(typeof(T))];
        var pResult = GCHandle.Alloc(result, GCHandleType.Pinned);
        Marshal.StructureToPtr(data, pResult.AddrOfPinnedObject(), true);
        pResult.Free();
        return result;
    }
}