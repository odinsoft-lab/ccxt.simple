using System.Diagnostics;
using System.IO.Compression;
using System.Text;

namespace CCXT.Simple.Core.Services
{
    /// <summary>
    /// Responsible to GZIP decompress
    /// </summary>
    public class GZipService
    {
        private const int buffer_size = 4096;

        /// <summary>
        /// Decompress the byte array to UTF8 string
        /// </summary>
        /// <param name="input">byte array</param>
        /// <param name="length"></param>
        /// <returns>UTF8 string</returns>
        public static string Decompress(byte[] input, int length)
        {
            var _result = "";

            try
            {
                if (input.Length > length && input[length] != 0x00)
                    input[length] = 0x00;

                using (var _gs = new GZipStream(new MemoryStream(input), CompressionMode.Decompress))
                {
                    var _buffer = new byte[buffer_size];

                    using (var _ms = new MemoryStream())
                    {
                        while (true)
                        {
                            var _count = _gs.Read(_buffer, 0, buffer_size);
                            if (_count <= 0)
                                break;

                            _ms.Write(_buffer, 0, _count);
                        }

                        _result = Encoding.UTF8.GetString(_ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        public static string Decompress(byte[] input)
        {
            var _result = "";

            try
            {
                using (var _gs = new GZipStream(new MemoryStream(input), CompressionMode.Decompress))
                {
                    var _buffer = new byte[buffer_size];

                    using (var _ms = new MemoryStream())
                    {
                        while (true)
                        {
                            var _count = _gs.Read(_buffer, 0, buffer_size);
                            if (_count <= 0)
                                break;

                            _ms.Write(_buffer, 0, _count);
                        }

                        _result = Encoding.UTF8.GetString(_ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// Compress the UTF8 string to byte array.
        /// This method is only used in Unit Test in SDK.
        /// </summary>
        /// <param name="input">UTF8 string</param>
        /// <returns>byte array</returns>
        public static byte[] Compress(string input)
        {
            var _array = Encoding.UTF8.GetBytes(input);

            using (var _ms = new MemoryStream())
            {
                using (var _gs = new GZipStream(_ms, CompressionMode.Compress, true))
                    _gs.Write(_array, 0, _array.Length);

                return _ms.ToArray();
            }
        }
    }
}