using WhenFiled.Core.Disk;

namespace WhenFiled.Tests;

public class ScannerCorruptTests
{
    private static readonly DateOnly Today = new(2026, 11, 9);

    // A tiny JPEG with an EXIF block (TIFF, big-endian, one DateTimeOriginal-less IFD).
    private static byte[] Jpeg()
    {
        byte[] tiff =
        [
            0x4D, 0x4D, 0x00, 0x2A, 0, 0, 0, 8,
            0, 1, 0x87, 0x69, 0, 4, 0, 0, 0, 1, 0, 0, 0, 26, 0, 0, 0, 0,
            0, 1, 0x90, 0x03, 0, 2, 0, 0, 0, 20, 0, 0, 0, 44, 0, 0, 0, 0,
            (byte)'2', (byte)'0', (byte)'1', (byte)'6', (byte)':', (byte)'0', (byte)'3', (byte)':',
            (byte)'1', (byte)'5', (byte)' ', (byte)'1', (byte)'0', (byte)':', (byte)'0', (byte)'0',
            (byte)':', (byte)'0', (byte)'0', 0,
        ];
        var exif = new List<byte> { 0xFF, 0xD8, 0xFF, 0xE1 };
        var len = tiff.Length + 8;
        exif.Add((byte)(len >> 8)); exif.Add((byte)len);
        exif.AddRange("Exif\0\0"u8.ToArray());
        exif.AddRange(tiff);
        exif.AddRange([0xFF, 0xD9]);
        return exif.ToArray();
    }

    private static byte[] Mp4() =>
    [
        0, 0, 0, 24, (byte)'f', (byte)'t', (byte)'y', (byte)'p', (byte)'m', (byte)'p', (byte)'4', (byte)'2',
        0, 0, 0, 0, (byte)'m', (byte)'p', (byte)'4', (byte)'2', (byte)'i', (byte)'s', (byte)'o', (byte)'m',
        0, 0, 0, 40, (byte)'m', (byte)'o', (byte)'o', (byte)'v',
        0, 0, 0, 32, (byte)'m', (byte)'v', (byte)'h', (byte)'d', 0, 0, 0, 0,
        0xDC, 0x2A, 0x3C, 0x80, 0, 0, 0, 0, 0, 0, 3, 0xE8, 0, 0, 0x27, 0x10,
    ];

    [Fact]
    public void Corrupt_files_never_stop_the_Scan()
    {
        using var t = new TempFolder();
        var src = t.Dir("src");
        var dst = t.Dir("dst");
        var rng = new Random(12345);
        foreach (var (name, bytes) in new[] { ("a.jpg", Jpeg()), ("a.mp4", Mp4()) })
        {
            for (var i = 0; i < 200; i++)
            {
                var copy = (byte[])bytes.Clone();
                for (var k = 0; k < 1 + rng.Next(4); k++)
                    copy[rng.Next(copy.Length)] = (byte)rng.Next(256);
                if (i % 5 == 0) Array.Resize(ref copy, rng.Next(copy.Length));
                System.IO.File.WriteAllBytes(Path.Combine(src, $"{i}_{name}"), copy);
            }
        }

        var result = new Scanner().Scan(src, dst, Today, TimeZoneInfo.Utc, null);

        Assert.Null(result.Refusal);
    }
}
