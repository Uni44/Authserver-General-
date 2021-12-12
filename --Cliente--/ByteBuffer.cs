using System;
using System.Collections.Generic;
using System.Text;

	class ByteBuffer
	{

		private List<byte> Buff;

		private byte[] readBuff;

		private int readpos;

		private bool buffUpdate;

		private bool disposedValue;

		public ByteBuffer()
		{
			Buff = new List<byte>();
			readpos = 0;
		}

		public int GetReadPos()
		{
			return readpos;
		}

		public byte[] ToArray()
		{
			return Buff.ToArray();
		}

		public int Count()
		{
			return Buff.Count;
		}

		public int Length()
		{
			return Count() - readpos;
		}

		public void Clear()
		{
			Buff.Clear();
			readpos = 0;
		}

		public void WriteByte(byte Input)
		{
			Buff.Add(Input);
			buffUpdate = true;
		}

		public void WriteBytes(byte[] Input)
		{
			Buff.AddRange(Input);
			buffUpdate = true;
		}

		public void WriteShort(short Input)
		{
			Buff.AddRange(BitConverter.GetBytes(Input));
			buffUpdate = true;
		}

		public void WriteInteger(int Input)
		{
			Buff.AddRange(BitConverter.GetBytes(Input));
			buffUpdate = true;
		}

		public void WriteFloat(float Input)
		{
			Buff.AddRange(BitConverter.GetBytes(Input));
			buffUpdate = true;
		}

		public void WriteString(string Input)
		{
			Buff.AddRange(BitConverter.GetBytes(Input.Length));
			Buff.AddRange(Encoding.ASCII.GetBytes(Input));
			buffUpdate = true;
		}

		public string ReadString(bool Peek = true)
		{
			int num = ReadInteger();
			if (buffUpdate)
			{
				readBuff = Buff.ToArray();
				buffUpdate = false;
			}
			string @string = Encoding.ASCII.GetString(readBuff, readpos, num);
			if ((Peek & (Buff.Count > readpos)) && @string.Length > 0)
			{
				readpos += num;
			}
			return @string;
		}

		public byte ReadByte(bool Peek = true)
		{
			if (Buff.Count > readpos)
			{
				if (buffUpdate)
				{
					readBuff = Buff.ToArray();
					buffUpdate = false;
				}
				byte result = readBuff[readpos];
				if (Peek & (Buff.Count > readpos))
				{
					readpos++;
				}
				return result;
			}
			throw new Exception("Byte Buffer Past Limit!");
		}

		public byte[] ReadBytes(int Length, bool Peek = true)
		{
			if (buffUpdate)
			{
				readBuff = Buff.ToArray();
				buffUpdate = false;
			}
			byte[] result = Buff.GetRange(readpos, Length).ToArray();
			if (Peek)
			{
				readpos += Length;
			}
			return result;
		}

		public float ReadFloat(bool Peek = true)
		{
			if (Buff.Count > readpos)
			{
				if (buffUpdate)
				{
					readBuff = Buff.ToArray();
					buffUpdate = false;
				}
				float result = BitConverter.ToSingle(readBuff, readpos);
				if (Peek & (Buff.Count > readpos))
				{
					readpos += 4;
				}
				return result;
			}
			throw new Exception("Byte Buffer Past Limit!");
		}

		public int ReadInteger(bool Peek = true)
		{
			if (Buff.Count > readpos)
			{
				if (buffUpdate)
				{
					readBuff = Buff.ToArray();
					buffUpdate = false;
				}
				int result = BitConverter.ToInt32(readBuff, readpos);
				if (Peek & (Buff.Count > readpos))
				{
					readpos += 4;
				}
				return result;
			}
			throw new Exception("Byte Buffer Past Limit!");
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Buff.Clear();
				}
				readpos = 0;
			}
			disposedValue = true;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
}