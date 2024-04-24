
using System;
using System.Reflection;
using System.Text;

public abstract class BaseData
{
    //获取字节数组容器大小的方法
    public abstract int GetBytesNum();
    //获取成员变量序列化为 对应的字节数组
    public abstract byte[] WriteBytes();
    /// <summary>
    /// 反序列化字节数组 到成员变量中
    /// </summary>
    /// <param name="bytes">反序列化使用的字节数组</param>
    /// <param name="beginIndex">从字节数组的第几个位置开始解析 默认是0</param>
    /// <returns></returns>
    public abstract int ReadBytes(byte[] bytes,int beginIndex = 0);

    /// <summary>
    /// 存储int类型变量到指定字节数组中
    /// </summary>
    /// <param name="bytes">指定字节数组</param>
    /// <param name="value">具体int值</param>
    /// <param name="index">每次存储后记录当前索引位置</param>
    protected void WriteInt(byte[] bytes, int value, ref int index)
    {
        BitConverter.GetBytes(value).CopyTo(bytes, index);
        index += sizeof(int);
    }
    protected void WriteShort(byte[] bytes, short value, ref int index)
    {
        BitConverter.GetBytes(value).CopyTo(bytes, index);
        index += sizeof(short);
    }
    protected void WriteLong(byte[] bytes, long value, ref int index)
    {
        BitConverter.GetBytes(value).CopyTo(bytes, index);
        index += sizeof(long);
    }
    protected void WriteFloat(byte[] bytes, float value, ref int index)
    {
        BitConverter.GetBytes(value).CopyTo(bytes, index);
        index += sizeof(float);
    }
    protected void WriteByte(byte[] bytes, byte value, ref int index)
    {
        bytes[index] = value;
        index += sizeof(byte);
    }
    protected void WriteBool(byte[] bytes, bool value, ref int index)
    {
        BitConverter.GetBytes(value).CopyTo(bytes, index);
        index += sizeof(bool);
    }
    protected void WriteString(byte[] bytes, string value, ref int index)
    {
        byte[] strBytes = Encoding.UTF8.GetBytes(value);
        //存储字符串字节数组长度
        //BitConverter.GetBytes(num).CopyTo(bytes, index);
        //index += sizeof(int);
        WriteInt(bytes, strBytes.Length, ref index);
        //存储字符串字节数组
        strBytes.CopyTo(bytes, index);
        index += strBytes.Length;
    }
    protected void WriteData(byte[] bytes, BaseData data, ref int index)
    {
        data.WriteBytes().CopyTo(bytes, index);
        index += data.GetBytesNum();
    }
    /// <summary>
    /// 根据字节数组 读取整形
    /// </summary>
    /// <param name="bytes">字节数组</param>
    /// <param name="index">开始索引数</param>
    /// <returns></returns>
    protected int ReadInt(byte[] bytes, ref int index)
    {
        int value = BitConverter.ToInt32(bytes, index);
        index += sizeof(int);
        return value;
    }
    protected short ReadShort(byte[] bytes, ref int index)
    {
        short value = BitConverter.ToInt16(bytes, index);
        index += sizeof(short);
        return value;
    }
    protected long ReadLong(byte[] bytes, ref int index)
    {
        long value = BitConverter.ToInt64(bytes, index);
        index += sizeof(long);
        return value;
    }
    protected float ReadFloat(byte[] bytes, ref int index)
    {
        float value = BitConverter.ToSingle(bytes, index);
        index += sizeof(float);
        return value;
    }
    protected byte ReadByte(byte[] bytes, ref int index)
    {
        byte value = bytes[index];
        index += sizeof(byte);
        return value;
    }
    protected bool ReadBool(byte[] bytes, ref int index)
    {
        bool value = BitConverter.ToBoolean(bytes, index);
        index += sizeof(bool);
        return value;
    }
    protected string ReadString(byte[] bytes, ref int index)
    {
        //读长度
        int length = ReadInt(bytes, ref index);
        //读取string
        string value = Encoding.UTF8.GetString(bytes, index, length);
        index += length;
        return value;
    }
    protected T ReadData<T>(byte[] bytes, ref int index) where T : BaseData,new()
    {
        T value = new();
        index += value.ReadBytes(bytes,index);
        return value;
    }
}
