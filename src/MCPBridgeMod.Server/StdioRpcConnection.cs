using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MCPBridgeMod.Server;

public sealed class StdioRpcConnection
{
	private enum RpcTransportMode
	{
		Auto,
		ContentLength,
		NewlineDelimited
	}

	private const byte CarriageReturn = 13;

	private const byte LineFeed = 10;

	private readonly Stream _input;

	private readonly Stream _output;

	private RpcTransportMode _transportMode = RpcTransportMode.Auto;

	public StdioRpcConnection(Stream input, Stream output)
	{
		_input = input;
		_output = output;
	}

	public async Task<JsonDocument?> ReadMessageAsync(CancellationToken cancellationToken)
	{
		if (_transportMode == RpcTransportMode.NewlineDelimited)
		{
			return await ReadLineDelimitedMessageAsync(Array.Empty<byte>(), cancellationToken);
		}
		if (_transportMode == RpcTransportMode.ContentLength)
		{
			return await ReadContentLengthMessageAsync(Array.Empty<byte>(), cancellationToken);
		}
		byte? firstByte = await ReadFirstMeaningfulByteAsync(cancellationToken);
		if (!firstByte.HasValue)
		{
			return null;
		}
		bool flag;
		switch (firstByte)
		{
		case 91:
		case 123:
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			_transportMode = RpcTransportMode.NewlineDelimited;
			return await ReadLineDelimitedMessageAsync(new _003C_003Ez__ReadOnlySingleElementList<byte>(firstByte.Value), cancellationToken);
		}
		_transportMode = RpcTransportMode.ContentLength;
		return await ReadContentLengthMessageAsync(new _003C_003Ez__ReadOnlySingleElementList<byte>(firstByte.Value), cancellationToken);
	}

	public async Task WriteMessageAsync(object payload, CancellationToken cancellationToken)
	{
		byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(payload);
		if (_transportMode == RpcTransportMode.NewlineDelimited)
		{
			await _output.WriteAsync(jsonBytes, cancellationToken);
			await _output.WriteAsync(new byte[1] { 10 }, cancellationToken);
			await _output.FlushAsync(cancellationToken);
		}
		else
		{
			byte[] headerBytes = Encoding.ASCII.GetBytes($"Content-Length: {jsonBytes.Length}\r\n\r\n");
			await _output.WriteAsync(headerBytes, cancellationToken);
			await _output.WriteAsync(jsonBytes, cancellationToken);
			await _output.FlushAsync(cancellationToken);
		}
	}

	private async Task<JsonDocument?> ReadContentLengthMessageAsync(IReadOnlyCollection<byte> prefix, CancellationToken cancellationToken)
	{
		List<byte> headerBytes = new List<byte>();
		foreach (byte value in prefix)
		{
			headerBytes.Add(value);
		}
		int count;
		do
		{
			byte[] nextByte = new byte[1];
			if (await _input.ReadAsync(nextByte, cancellationToken) == 0)
			{
				if (headerBytes.Count == 0)
				{
					return null;
				}
				throw new EndOfStreamException("Unexpected EOF while reading JSON-RPC headers.");
			}
			headerBytes.Add(nextByte[0]);
			count = headerBytes.Count;
		}
		while (count < 4 || headerBytes[count - 4] != 13 || headerBytes[count - 3] != 10 || headerBytes[count - 2] != 13 || headerBytes[count - 1] != 10);
		string headerText = Encoding.ASCII.GetString(headerBytes.ToArray());
		int contentLength = ParseContentLength(headerText);
		byte[] payload = new byte[contentLength];
		int read;
		for (int offset = 0; offset < contentLength; offset += read)
		{
			read = await _input.ReadAsync(payload.AsMemory(offset, contentLength - offset), cancellationToken);
			if (read == 0)
			{
				throw new EndOfStreamException("Unexpected EOF while reading JSON-RPC payload.");
			}
		}
		return JsonDocument.Parse(payload);
	}

	private async Task<JsonDocument?> ReadLineDelimitedMessageAsync(IReadOnlyCollection<byte> prefix, CancellationToken cancellationToken)
	{
		List<byte> lineBytes = new List<byte>();
		foreach (byte value in prefix)
		{
			lineBytes.Add(value);
		}
		byte[] nextByte;
		do
		{
			nextByte = new byte[1];
			if (await _input.ReadAsync(nextByte, cancellationToken) == 0)
			{
				if (lineBytes.Count == 0)
				{
					return null;
				}
				break;
			}
			lineBytes.Add(nextByte[0]);
		}
		while (nextByte[0] != 10);
		while (true)
		{
			int num;
			if (lineBytes.Count > 0)
			{
				if (lineBytes[lineBytes.Count - 1] != 10)
				{
					num = ((lineBytes[lineBytes.Count - 1] == 13) ? 1 : 0);
				}
				else
				{
					num = 1;
				}
			}
			else
			{
				num = 0;
			}
			if (num == 0)
			{
				break;
			}
			lineBytes.RemoveAt(lineBytes.Count - 1);
		}
		if (lineBytes.Count == 0)
		{
			return await ReadLineDelimitedMessageAsync(Array.Empty<byte>(), cancellationToken);
		}
		return JsonDocument.Parse(lineBytes.ToArray());
	}

	private async Task<byte?> ReadFirstMeaningfulByteAsync(CancellationToken cancellationToken)
	{
		byte[] nextByte;
		do
		{
			nextByte = new byte[1];
			if (await _input.ReadAsync(nextByte, cancellationToken) == 0)
			{
				return null;
			}
		}
		while (char.IsWhiteSpace((char)nextByte[0]));
		return nextByte[0];
	}

	private static int ParseContentLength(string headerText)
	{
		string[] array = headerText.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			if (text.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
			{
				string text2 = text;
				int length = "Content-Length:".Length;
				string s = text2.Substring(length, text2.Length - length).Trim();
				if (int.TryParse(s, out var result))
				{
					return result;
				}
			}
		}
		throw new InvalidDataException("Content-Length header is missing.");
	}
}
