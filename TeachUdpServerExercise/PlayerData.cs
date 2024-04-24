using System.Collections;
using System.Collections.Generic;
using System.Text;


/// <summary>
/// 玩家数据类
/// </summary>
public class PlayerData : BaseData
{
    public int lev;
    public string name;
    public int atk;
    public bool sex;

    public override int GetBytesNum()
    {
        return 4+4+4+Encoding.UTF8.GetBytes(name).Length + 1;
    }

    public override int ReadBytes(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;
        lev = ReadInt(bytes,ref index);
        name = ReadString(bytes,ref index);
        atk = ReadInt(bytes,ref index);
        sex = ReadBool(bytes,ref index);
        return index - beginIndex;
    }

    public override byte[] WriteBytes()
    {
        int index = 0;
        byte[] bytes = new byte[GetBytesNum()];
        WriteInt(bytes,lev,ref index);
        WriteString(bytes,name,ref index);
        WriteInt(bytes,atk,ref index);
        WriteBool(bytes,sex,ref index);
        return bytes;
    }
}
