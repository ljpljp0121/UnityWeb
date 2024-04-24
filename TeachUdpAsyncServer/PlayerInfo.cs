using System.Text;

public class PlayerInfo : BaseInfo
{
    public int playerID;
    
    public PlayerData playerData;

    public override int GetBytesNum()
    {
        return 4 //消息id长度
                +4
                +4 // playerID的长度
                +playerData.GetBytesNum();
    }

    public override byte[] WriteBytes()
    {
        int index = 0;
        int bytesNum = GetBytesNum();
        byte[] bytes = new byte[GetBytesNum()];
        //消息ID
        WriteInt(bytes,GetID(),ref index);
        WriteInt(bytes, bytesNum - 8, ref index);
        //消息成员变量
        WriteInt(bytes, playerID, ref index);
        WriteData(bytes,playerData,ref index);
        return bytes;
    }

    public override int ReadBytes(byte[] bytes, int beginIndex = 0)
    {
        //反序列化不需要解析ID 因为在此之前将ID反序列化
        //以此判断用哪一个自定义类来序列化
        int index = beginIndex;
        playerID = ReadInt(bytes,ref index);
        playerData = ReadData<PlayerData>(bytes,ref index);
        return index - beginIndex;
    }
    public override int GetID()
    {
        return 1001;
    }
}