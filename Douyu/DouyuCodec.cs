using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace LivestreamDanmuku.Douyu
{
    public class DouyuCodec
    {  
        public delegate void DecodeCallback(string message);

        public byte[] Encode(string data)
        {
            var body = Encoding.UTF8.GetBytes(data).Concat(new byte[] { 0 });
            int len = body.Count() + 8;

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((uint)len);
            bw.Write((uint)len);
            bw.Write((short)689);
            bw.Write((short)0);
            bw.Write(body.ToArray());
            byte[] ret = ms.ToArray();
            bw.Close();

            return ret;
        }

        private int packetLength = 0;
        private IEnumerable<byte> buffer = new List<byte>();

        public void Decode(byte[] buf, DecodeCallback callback)
        {
            buffer = buffer.Concat(buf);
            while (buffer.Count() > 0)
            {
                if(packetLength == 0)
                {
                    if (buffer.Count() < 4)
                        return;
                    packetLength = BitConverter.ToInt32(buffer.Take(4).ToArray());
                    buffer = buffer.Skip(4);
                }

                if (buffer.Count() < packetLength)
                    return;
                 
                string message = Encoding.UTF8.GetString(buffer.Skip(8).Take(packetLength - 9).ToArray());
                buffer = buffer.Skip(packetLength);
                packetLength = 0;
                callback(message);
            }
        }
    }
}
