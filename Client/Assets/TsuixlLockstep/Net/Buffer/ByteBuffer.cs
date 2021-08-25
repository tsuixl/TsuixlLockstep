using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Network.Http;
using UnityEngine;

namespace Tsuixl.Net.Buffer
{
    public class ByteBuffer
    {
        private int _size;
        private byte[] _buffer;
        private int _startIndex = 0;
        
        private readonly byte[] _1Bytes = new byte[1];
        private readonly byte[] _2Bytes = new byte[2];
        private readonly byte[] _4Bytes = new byte[4];
        private readonly byte[] _8Bytes = new byte[8];
        private static readonly byte[] _emptyArray = new byte[0];
        
        private int _userInt32Data;

        public int Count => _size - _startIndex;

        public ref byte[] GetRefCache => ref _buffer;

        public int Index => _startIndex;

        public int Capacity
        {
            get
            {
                if (_buffer == null)
                    return 0;
                return _buffer.Length;
            }
            set
            {
                if (value <= _size)
                {
                    return;
                }

                if (value != _buffer.Length)
                {
                    if (value > 0) {
                        byte[] newItems = new byte[value];
                        if (_size > 0) {
                            Array.Copy(_buffer, 0, newItems, 0, _size);
                        }
                        _buffer = newItems;
                    }
                    else {
                        _buffer = _emptyArray;
                    }
                }
            }
        }

        public ByteBuffer()
        {
            _size = 0;
            _buffer = _emptyArray;
        }

        public ByteBuffer(int capacity)
        {
            if (capacity == 0)
            {
                _buffer = _emptyArray;
            }
            else
            {
                _buffer = new byte[capacity];
            }
        }

        public ArraySegment<byte> AsArraySegment()
        {
            return new ArraySegment<byte>(_buffer, _startIndex, Count);
        }
        
        private void EnsureCapacity(int count)
        {
            if (_buffer.Length >= count)
            {
                return;
            }
            int num = _buffer.Length == 0 ? 4 : _buffer.Length * 2;
            num = Mathf.Clamp(num, count, int.MaxValue);
            Capacity = num;
        }
        
        #region Read

        public byte[] ReadByteArray(int length)
        {
            if (length > Count)
            {
                return null;
            }

            var result = new byte[length];
            Array.Copy(_buffer, _startIndex, result, 0, length);
            _startIndex += length;
            return result;
        }

        //1字节
        public byte ReadByte()
        {
            if (Count < 1)
            {
                NetLog.LogError("[NetBuffer](ReadByte)数组越界");
                return 0;
            }

            _startIndex += 1;
            return _buffer[_startIndex - 1];
        }

        //4字节
        public Int32 ReadInt32()
        {
            if (Count < 4)
            {
                NetLog.LogError("[NetBuffer](ReadInt32)数组越界");
                return 0;
            }
            
            var reuslt = BitConverter.ToInt32(_buffer, _startIndex);
            _startIndex += 4;
            return IPAddress.NetworkToHostOrder(reuslt);
        }

        public Int32 PeekInt32(int offset = 0)
        {
            var targetIndex = _startIndex + offset;
            if (targetIndex + 4 > _size)
            {
                NetLog.LogError($"index out range {targetIndex}");
                return 0;
            }
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(_buffer, targetIndex));
        }

        //4字节
        public UInt32 ReadUInt32()
        {
            if (Count < 4)
            {
                NetLog.LogError("[NetBuffer](ReadUInt32)数组越界");
                return 0;
            }

            for (int i = 0; i < 4; i++)
            {
                _4Bytes[i] = _buffer[_startIndex + i];
            }

            _startIndex += 4;
            return BitConverter.ToUInt32(_4Bytes, 0);
        }

        //8字节
        public double ReadDouble()
        {
            if (Count < 8)
            {
                NetLog.LogError("[NetBuffer](ReadDouble)数组越界");
                return 0;
            }
            _startIndex += 8;
            return BitConverter.ToDouble(_buffer, _startIndex - 8);
        }

        //4字节
        public float ReadFloat()
        {
            if (Count < 4)
            {
                NetLog.LogError("[NetBuffer](ReadFloat)数组越界");
                return 0;
            }
            _startIndex += 4;
            return BitConverter.ToSingle(_buffer, _startIndex - 4);
        }

        //1字节
        public bool ReadBool()
        {
            if (Count < 1)
            {
                NetLog.LogError("[NetBuffer](ReadBool)数组越界");
                return false;
            }

            _startIndex += 1;
            return _buffer[_startIndex - 1] == 0;
        }

        //2字节
        public short ReadShort()
        {
            if (Count < 2)
            {
                NetLog.LogError("[NetBuffer](ReadShort)数组越界");
                return 0;
            }

            _startIndex += 2;
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(_buffer, _startIndex - 2));
        }

        public short PeekShort(int offset = 0)
        {
            var targetIndex = _startIndex + offset;
            if (targetIndex + 2 > _size)
            {
                NetLog.LogError($"index out range {targetIndex}");
                return 0;
            }
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(_buffer, targetIndex));
        }

        //2字节
        public ushort ReadUShort()
        {
            if (Count < 2)
            {
                NetLog.LogError("[NetBuffer](ReadUShort)数组越界");
                return 0;
            }
            
            _startIndex += 2;
            return BitConverter.ToUInt16(_buffer, _startIndex - 2);
        }

        //2字节长度 + 消息
        public string ReadString()
        {
            short length = ReadShort();
            return Encoding.UTF8.GetString(ReadByteArray(length));
        }

        //1字节
        public char ReadChar()
        {
            if (Count < 1)
            {
                NetLog.LogError("[NetBuffer](ReadChar)数组越界");
                return (char) 0;
            }

            _startIndex += 1;
            return BitConverter.ToChar(_buffer, _startIndex - 1);
        }

        //8字节
        public long ReadLong()
        {
            if (Count < 8)
            {
                NetLog.LogError("[NetBuffer](ReadLong)数组越界");
                return 0;
            }

            _startIndex += 8;
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(_buffer, _startIndex - 8));
        }

        //8字节
        public ulong ReadULong()
        {
            if (Count < 8)
            {
                NetLog.LogError("[NetBuffer](ReadULong)数组越界");
                return 0;
            }
            
            var result = BitConverter.ToUInt64(_buffer, _startIndex);
            _startIndex += 8;
            return result;
        }
        
        #endregion

        #region Write
        
        public void WriteByteArray(byte[] sourceArray)
        {
            if (_size + sourceArray.Length >= _buffer.Length)
            {
                EnsureCapacity(_size + sourceArray.Length + 1);
            }
            Array.Copy(sourceArray, 0, _buffer, _size, sourceArray.Length);
            _size += sourceArray.Length;
        }

        public void WriteByteArray(byte[] array, int startIndex, int length)
        {
            if (_size + length >= _buffer.Length)
            {
                EnsureCapacity(_size + length + 1);
            }
            Array.Copy(array, startIndex, _buffer, _startIndex, length);
            _startIndex += length;
        }

        public void WriteByte(byte value)
        {
            if (_size >= _buffer.Length)
            {
                EnsureCapacity(_size + 1);
            }
            _buffer[_size++] = value;
        }

        public void WriteString(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return;
            }

            WriteShort((short) source.Length);
            Encoding.UTF8.GetBytes(source, 0, source.Length, _buffer, _size);
            _size += source.Length;
        }

        public unsafe void WriteChar(char source)
        {
            fixed (byte* ptr = _1Bytes)
            {
                *(char*) ptr = source;
            }
            WriteByte(_1Bytes[0]);
        }

        public void WriteBool(bool source)
        {
            WriteByte((byte) ((source) ? 1 : 0));
        }

        public unsafe void WriteShort(short source)
        {
            source = IPAddress.HostToNetworkOrder(source);
            fixed (byte* ptr = _2Bytes)
            {
                *(short*) ptr = source;
            }

            WriteByteArray(_2Bytes);
        }

        public unsafe void WriteUShort(ushort source)
        {
            fixed (byte* ptr = _2Bytes)
            {
                *(short*) ptr = (short) source;
            }

            WriteByteArray(_2Bytes);
        }

        //3000次int写入【279KB】 相对C#Getbytes【394KB】少 115KB
        public unsafe void WriteInt32(Int32 source)
        {
            source = IPAddress.HostToNetworkOrder(source);
            fixed (byte* ptr = _4Bytes)
            {
                *(int*) ptr = source;
            }

            WriteByteArray(_4Bytes);
        }

        public unsafe void WriteUInt32(UInt32 source)
        {
            fixed (byte* ptr = _4Bytes)
            {
                *(int*) ptr = (int) source;
            }

            WriteByteArray(_4Bytes);
        }

        public unsafe void WriteLong(long source)
        {
            source = IPAddress.HostToNetworkOrder(source);
            fixed (byte* ptr = _8Bytes)
            {
                *(long*) ptr = source;
            }

            WriteByteArray(_8Bytes);
        }

        public unsafe void WriteUnLong(ulong source)
        {
            fixed (byte* ptr = _8Bytes)
            {
                *(long*) ptr = (long) source;
            }

            WriteByteArray(_8Bytes);
        }

        public unsafe void WriteDouble(double source)
        {
            fixed (byte* ptr = _8Bytes)
            {
                *(long*) ptr = *(long*) (&source);
            }

            WriteByteArray(_8Bytes);
        }

        public unsafe void WriteFloat(float source)
        {
            fixed (byte* ptr = _4Bytes)
            {
                *(int*) ptr = *(int*) (&source);
            }

            WriteByteArray(_4Bytes);
        }
        
        #endregion


        /// <summary>
        /// 索引跳过指定长度。
        /// </summary>
        /// <param name="count"></param>
        public void Advance(int count)
        {
            if (_startIndex + count > _size)
            {
                NetLog.LogError($"index out of range {_startIndex + count}");
                return;
            }
            _startIndex += count;
        }


        /// <summary>
        /// 增大指定有效数据大小
        /// </summary>
        /// <param name="size"></param>
        public void IncreaseSize(int size)
        {
            _size += size;
        }

        public void SetUserData(int data)
        {
            _userInt32Data = data;
        }

        public int GetUserData()
        {
            return _userInt32Data;
        }
        
        /// <summary>
        /// 重置索引与大小
        /// </summary>
        public void Reset()
        {
            _size = 0;
            _startIndex = 0;
        }

        /// <summary>
        /// 重置索引
        /// </summary>
        public void ResetPos()
        {
            _startIndex = 0;
        }

        public byte[] ToArray()
        {
            if (_size == 0)
            {
                return new byte[0];
            }
            var result = new byte[_size];
            Array.Copy(_buffer, 0, result, 0, _size);
            return result;
        }
    }
}