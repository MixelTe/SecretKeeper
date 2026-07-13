namespace sek;

using System;
using System.IO;

public class ZipEntryStreamWrapper : Stream
{
	private readonly Stream _baseStream;
	private readonly IDisposable _parentZip;

	public ZipEntryStreamWrapper(Stream baseStream, IDisposable parentZip)
	{
		_baseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
		_parentZip = parentZip ?? throw new ArgumentNullException(nameof(parentZip));
	}

	public override bool CanRead => _baseStream.CanRead;
	public override bool CanSeek => _baseStream.CanSeek;
	public override bool CanWrite => _baseStream.CanWrite;
	public override long Length => _baseStream.Length;
	public override long Position
	{
		get => _baseStream.Position;
		set => _baseStream.Position = value;
	}

	public override void Flush() => _baseStream.Flush();
	public override int Read(byte[] buffer, int offset, int count) => _baseStream.Read(buffer, offset, count);
	public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);
	public override void SetLength(long value) => _baseStream.SetLength(value);
	public override void Write(byte[] buffer, int offset, int count) => _baseStream.Write(buffer, offset, count);

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_baseStream.Dispose();
			_parentZip.Dispose();
		}
		base.Dispose(disposing);
	}
}