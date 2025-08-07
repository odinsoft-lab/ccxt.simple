using CCXT.Simple.Services;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

namespace CCXT.Simple.Exchanges.Bitget.WS;

public class WebSocket : XBitget
{
    public WebSocket(Exchange mainXchg, string apiKey , string secretKey , string passPhrase)
        : base(mainXchg, apiKey, secretKey, passPhrase)
    {
    }

    public async Task Start(CancellationToken cancelToken, string instType, string channel, string[] symbols)
    {
        while (true)
        {
            var _buffer_size = 1024 * 16;
            var _buffer = new byte[_buffer_size];

            var _offset = 0;
            var _free = _buffer.Length;

            var _ping_timestamp = 0L;
            var _ping_sent = false;

            try
            {
                using (var _cws = new ClientWebSocket())
                {
                    await OpenWSAsync(cancelToken, _cws);

                    await LoginAsync(cancelToken, _cws);

                    Console.WriteLine(".. waiting 10 seconds for login...");
                    await Task.Delay(1000 * 10);

                    await SubscribeAsync(cancelToken, _cws, instType, channel, symbols);

                    while (!cancelToken.IsCancellationRequested)
                    {
                        var _result = await _cws.ReceiveAsync(new ArraySegment<byte>(_buffer, _offset, _free), cancelToken);

                        _offset += _result.Count;
                        _free -= _result.Count;

                        if (_result.EndOfMessage == false)
                        {
                            if (_free == 0)
                            {
                                Array.Resize(ref _buffer, _buffer.Length + _buffer_size);
                                _free = _buffer.Length - _offset;
                            }

                            continue;
                        }

                        if (_result.MessageType == WebSocketMessageType.Close)
                        {
                            await _cws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", cancelToken);
                            Console.WriteLine(".. receive close message from server...");

                            await Task.Delay(100);
                            break;
                        }

                        var _jstring = Encoding.UTF8.GetString(_buffer, 0, _offset);
                        Console.WriteLine($">> {_jstring}");

                        var _curr_timestamp = DateTimeXts.Now;
                        {
                            if (_jstring == "pong")
                            {
                                _ping_timestamp = _curr_timestamp + 30;
                                _ping_sent = false;
                            }
                            else
                            {
                                if (_ping_timestamp < _curr_timestamp)
                                {
                                    if (!_ping_sent)
                                    {
                                        _ping_timestamp = _curr_timestamp + 30;
                                        _ping_sent = true;

                                        await SendAsync(cancelToken, _cws, "ping");
                                        Console.WriteLine("<< ping");
                                    }
                                    else if (_ping_timestamp + 30 < _curr_timestamp)
                                    {
                                        Console.WriteLine(".. close...");

                                        await _cws.CloseAsync(WebSocketCloseStatus.NormalClosure, "close web-socket", cancelToken);
                                        break;
                                    }
                                }
                            }
                        }

                        _offset = 0;
                        _free = _buffer.Length;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                await Task.Delay(1000 * 3);         // waiting for reconnect
            }
        }
    }

    private async ValueTask SendAsync(CancellationToken cancelToken, ClientWebSocket cws, string message)
    {
        Console.WriteLine($"<< {message}");

        var _cmd_bytes = Encoding.UTF8.GetBytes(message.Replace('\'', '\"'));
        await cws.SendAsync(
                    new ArraySegment<byte>(_cmd_bytes),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken: cancelToken
                );
    }

    private async ValueTask OpenWSAsync(CancellationToken cancelToken, ClientWebSocket cws)
    {
        if (cws.State == WebSocketState.Open)
            await cws.CloseAsync(WebSocketCloseStatus.NormalClosure, "reopen", cancelToken);

        await cws.ConnectAsync(new Uri("wss://ws.bitget.com/spot/v1/stream"), cancelToken);
    }

    private async ValueTask SubscribeAsync(CancellationToken cancelToken, ClientWebSocket cws, string instType, string channel, string[] symbols)
    {
        var _args = String.Join(",", symbols.Select(x => $"{{'instType':'{instType}','channel':'{channel}','instId':'{x}'}}"));
        var _message = "{'op':'subscribe', 'args':[" + _args + "]}";
        await SendAsync(cancelToken, cws, _message);
    }

    private async ValueTask UnsubscribeAsync(CancellationToken cancelToken, ClientWebSocket cws, string instType, string channel, string[] symbols)
    {
        var _args = String.Join(",", symbols.Select(x => $"{{'instType':'{instType}','channel':'{channel}','instId':'{x}'}}"));
        var _message = "{'op':'unsubscribe', 'args':[" + _args + "]}";
        await SendAsync(cancelToken, cws, _message);
    }

    private async ValueTask LoginAsync(CancellationToken cancelToken, ClientWebSocket cws)
    {
        var _endpoint = "/user/verify";
        var _sign = this.CreateWsSignature("GET", _endpoint);

        var _args = new Dictionary<string, string>();
        {
            _args.Add("apiKey", this.ApiKey);
            _args.Add("passphrase", this.PassPhrase);
            _args.Add("timestamp", $"{_sign.timestamp}");
            _args.Add("sign", _sign.sign);
        }

        var _message = "{'op':'login', 'args':[" + JsonConvert.SerializeObject(_args) + "]}";
        await SendAsync(cancelToken, cws, _message);
    }
}