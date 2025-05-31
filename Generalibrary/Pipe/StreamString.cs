using System.Text;

namespace Generalibrary
{
    public class StreamString : IDisposable
    {
        private Stream _stream;
        private UnicodeEncoding _streamEncoding;
        private bool disposedValue;

        public StreamString(Stream stream)
        {
            _stream = stream;
            _streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len = 0;
            len = _stream.ReadByte() * 256;
            len += _stream.ReadByte();
            byte[] inBuffer = new byte[len];
            _stream.Read(inBuffer, 0, len);

            return _streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = _streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            _stream.WriteByte((byte)(len / 256));
            _stream.WriteByte((byte)(len & 255));
            _stream.Write(outBuffer, 0, len);
            _stream.Flush();

            return outBuffer.Length + 2;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 관리형 상태(관리형 개체)를 삭제합니다.
                    _stream.Dispose();
                }

                // TODO: 비관리형 리소스(비관리형 개체)를 해제하고 종료자를 재정의합니다.
                // TODO: 큰 필드를 null로 설정합니다.
                _streamEncoding = null;
                disposedValue = true;
            }
        }

        // // TODO: 비관리형 리소스를 해제하는 코드가 'Dispose(bool disposing)'에 포함된 경우에만 종료자를 재정의합니다.
        // ~StreamString()
        // {
        //     // 이 코드를 변경하지 마세요. 'Dispose(bool disposing)' 메서드에 정리 코드를 입력합니다.
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 이 코드를 변경하지 마세요. 'Dispose(bool disposing)' 메서드에 정리 코드를 입력합니다.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
