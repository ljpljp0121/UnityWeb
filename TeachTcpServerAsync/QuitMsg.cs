using System.Collections;
using System.Collections.Generic;

public class QuitMsg : BaseInfo
{
    //����Ҫ������ ����ֱ�ӷ���һ��id���ֽڳ��ȵĳ���
    public override int GetBytesNum()
    {
        return 8;
    }

    public override int GetID()
    {
        return 1003;
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