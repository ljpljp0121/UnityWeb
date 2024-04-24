using System.Collections;
using System.Collections.Generic;

//心跳消息
public class Heart : BaseInfo
{
    public override int GetID()
    {
        return 999;
    }
    //不需要方法体 所以直接返回一个id和字节长度的长度
    public override int GetBytesNum()
    {
        return 8;
    }


    public override int ReadBytes(byte[] bytes, int beginIndex = 0)
    {
        return 0;
    }

    public override byte[] WriteBytes()
    {
        int index = 0;
        byte[] bytes = new byte[GetBytesNum()];
        WriteInt(bytes, GetID(), ref index);
        WriteInt(bytes, 0, ref index);
        return bytes;
    }
}
