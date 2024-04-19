
using System;
using System.Reflection;
using System.Text;

public abstract class BaseData
{
    //��ȡ�ֽ�����������С�ķ���
    public abstract int GetBytesNum();
    //��ȡ��Ա�������л�Ϊ ��Ӧ���ֽ�����
    public abstract byte[] WriteBytes();
    /// <summary>
    /// �����л��ֽ����� ����Ա������
    /// </summary>
    /// <param name="bytes">�����л�ʹ�õ��ֽ�����</param>
    /// <param name="beginIndex">���ֽ�����ĵڼ���λ�ÿ�ʼ���� Ĭ����0</param>
    /// <returns></returns>
    public abstract int ReadBytes(byte[] bytes,int beginIndex = 0);

    /// <summary>
    /// �洢int���ͱ�����ָ���ֽ�������
    /// </summary>
    /// <param name="bytes">ָ���ֽ�����</param>
    /// <param name="value">����intֵ</param>
    /// <param name="index">ÿ�δ洢���¼��ǰ����λ��</param>
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
        //�洢�ַ����ֽ����鳤��
        //BitConverter.GetBytes(num).CopyTo(bytes, index);
        //index += sizeof(int);
        WriteInt(bytes, strBytes.Length, ref index);
        //�洢�ַ����ֽ�����
        strBytes.CopyTo(bytes, index);
        index += strBytes.Length;
    }
    protected void WriteData(byte[] bytes, BaseData data, ref int index)
    {
        data.WriteBytes().CopyTo(bytes, index);
        index += data.GetBytesNum();
    }
    /// <summary>
    /// �����ֽ����� ��ȡ����
    /// </summary>
    /// <param name="bytes">�ֽ�����</param>
    /// <param name="index">��ʼ������</param>
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
        //������
        int length = ReadInt(bytes, ref index);
        //��ȡstring
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
