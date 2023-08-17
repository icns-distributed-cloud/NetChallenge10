using System;

//TCP Socket Packet BufferManager Class 
class PacketBufferManager
{
    private int mBufferSize = 0;
    private int mReadPos = 0;
    private int mWritePos = 0;

    private int mHeaderSize = 0;
    private int mMaxPacketSize = 0;
    private byte[] mPacketData;
    private byte[] mPacketDataTemp;

    public bool Init(int size, int headerSize, int maxPacketSize)
    {
        if (size < (maxPacketSize * 2) || size < 1 || headerSize < 1 || maxPacketSize < 1)
        {
            return false;
        }

        mBufferSize = size;
        mPacketData = new byte[size];
        mPacketDataTemp = new byte[size];
        mHeaderSize = headerSize;
        mMaxPacketSize = maxPacketSize;

        return true;
    }

    public bool Write(byte[] data, int pos, int size)
    {
        if (data == null || (data.Length < (pos + size)))
        {
            return false;
        }

        var remainBufferSize = mBufferSize - mWritePos;

        if (remainBufferSize < size)
        {
            return false;
        }

        Buffer.BlockCopy(data, pos, mPacketData, mWritePos, size);
        mWritePos += size;

        if (NextFree() == false)
        {
            BufferRelocate();
        }
        return true;
    }

    public ArraySegment<byte> Read()
    {
        var enableReadSize = mWritePos - mReadPos;

        if (enableReadSize < mHeaderSize)
        {
            return new ArraySegment<byte>();
        }

        var packetDataSize = BitConverter.ToUInt16(mPacketData, mReadPos);
        if (enableReadSize < packetDataSize)
        {
            return new ArraySegment<byte>();
        }

        var completePacketData = new ArraySegment<byte>(mPacketData, mReadPos, packetDataSize);
        mReadPos += packetDataSize;
        return completePacketData;
    }

    private bool NextFree()
    {
        var enableWriteSize = mBufferSize - mWritePos;

        if (enableWriteSize < mMaxPacketSize)
        {
            return false;
        }

        return true;
    }

    private void BufferRelocate()
    {
        var enableReadSize = mWritePos - mReadPos;

        Buffer.BlockCopy(mPacketData, mReadPos, mPacketDataTemp, 0, enableReadSize);
        Buffer.BlockCopy(mPacketDataTemp, 0, mPacketData, 0, enableReadSize);

        mReadPos = 0;
        mWritePos = enableReadSize;
    }
}