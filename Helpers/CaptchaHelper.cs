using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using sjam.Dal.Enum;
using sjam.Models;
using System.Security.Cryptography;
using System.Text;

namespace sjam.Helpers
{
    public class CaptchaHelper
    {
        private readonly IDistributedCache _cache;
        private readonly IConfiguration? _Configuration;

        //private const int Width = 150;
        //private const int Height = 50;
        //private const int CodeLength = 6;
        //private const int ExpiryMinutes = 5;
        private const int Width = 280;
        private const int Height = 70;
        private const int ExpiryMinutes = 5;
        private const int CaptchaLength = 6;

        //private const string Characters = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";

        private const string Characters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public CaptchaHelper(IDistributedCache cache, IConfiguration? configuration)
        {
            _cache = cache;
            _Configuration = configuration;
        }

        public ServiceResponse<CaptchaResponse> GetCaptcha()
        {
            try
            {
                // Get application environment
                var environment =
                    _Configuration?.GetValue<AppEnvironment>(
                        "AppConfig:Environment")
                    ?? AppEnvironment.PROD;

                // ENV: DEV / UAT
                var checkEnv =
                    environment == AppEnvironment.DEV ||
                    environment == AppEnvironment.UAT;

                // Generate CAPTCHA ID
                var captchaId = GenerateCaptchaId();

                // Generate CAPTCHA code
                var captchaCode = GenerateCaptchaCode();

                // Generate CAPTCHA image
                var imageBytes = GenerateCaptchaImage(captchaCode);

                // Hash CAPTCHA code before storing
                var captchaHash = HashCaptchaCode(captchaCode);

                // Store CAPTCHA hash in cache
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(ExpiryMinutes)
                };

                _cache.SetString(
                    $"captcha:{captchaId}",
                    captchaHash,
                    cacheOptions);

                // Convert PNG to Base64
                var base64Image =
                    Convert.ToBase64String(imageBytes);

                return new ServiceResponse<CaptchaResponse>
                {
                    Result = new CaptchaResponse
                    {
                        CaptchaImg =
                            $"data:image/png;base64,{base64Image}",

                        CaptchaId = captchaId,

                        CaptchaCode = checkEnv ? captchaCode : null,

                        Autofill = checkEnv
                    },

                    ResponseStatus = APIResponseStatus.Success,

                    Message = "Captcha generated successfully."
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<CaptchaResponse>
                {
                    Result = null,

                    ResponseStatus = APIResponseStatus.Error,

                    Message = "Unable to generate captcha."
                };
            }
        }

        private static long GenerateCaptchaId()
        {
            var min = (int)Math.Pow(10, CaptchaLength - 1);
            var max = (int)Math.Pow(10, CaptchaLength);

            return RandomNumberGenerator.GetInt32(min, max);
        }

        private static string GenerateCaptchaCode()
        {
            Span<byte> bytes = stackalloc byte[CaptchaLength];

            RandomNumberGenerator.Fill(bytes);

            var result = new StringBuilder(CaptchaLength);

            foreach (var value in bytes)
            {
                result.Append(
                    Characters[value % Characters.Length]);
            }

            return result.ToString();
        }

        private static string HashCaptchaCode(string captchaCode)
        {
            var normalizedCode =
                captchaCode
                    .Trim()
                    .ToUpperInvariant();

            var hash =
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(
                        normalizedCode));

            return Convert.ToHexString(hash);
        }

        //private static byte[] GenerateCaptchaImage(
        //    string captchaCode)
        //{
        //    using var image =
        //        new Image<Rgba32>(
        //            Width,
        //            Height,
        //            Color.White.ToPixel<Rgba32>());

        //    image.Mutate(context =>
        //        context.Paint(canvas =>
        //        {
        //            // --------------------------------
        //            // Noise lines
        //            // --------------------------------
        //            for (var i = 0; i < 7; i++)
        //            {
        //                var x1 = RandomInt(0, Width);
        //                var y1 = RandomInt(0, Height);

        //                var x2 = RandomInt(0, Width);
        //                var y2 = RandomInt(0, Height);

        //                canvas.DrawLine(
        //                    Pens.Solid(
        //                        Color.LightGray,
        //                        1),
        //                    new PointF(x1, y1),
        //                    new PointF(x2, y2));
        //            }

        //            // --------------------------------
        //            // Font
        //            // --------------------------------
        //            var fontFamily =
        //                SystemFonts.Families.First();

        //            var font =
        //                SystemFonts.CreateFont(
        //                    fontFamily.Name,
        //                    27,
        //                    FontStyle.Bold);

        //            // --------------------------------
        //            // CAPTCHA text
        //            // --------------------------------
        //            var textOptions =
        //                new RichTextOptions(font)
        //                {
        //                    Origin =
        //                        new PointF(12, 9),

        //                    WrappingLength = -1
        //                };

        //            canvas.DrawText(
        //                textOptions,
        //                captchaCode,
        //                Brushes.Solid(Color.Black),
        //                pen: null);

        //            // --------------------------------
        //            // Noise dots
        //            // --------------------------------
        //            for (var i = 0; i < 50; i++)
        //            {
        //                var x = RandomInt(0, Width);
        //                var y = RandomInt(0, Height);

        //                canvas.Fill(
        //                    Brushes.Solid(Color.Gray),
        //                    new EllipsePolygon(
        //                        new PointF(x, y),
        //                        new SizeF(3, 3)));
        //            }
        //        }));

        //    using var stream =
        //        new MemoryStream();

        //    image.Save(
        //        stream,
        //        new PngEncoder());

        //    return stream.ToArray();
        //}

        //private static byte[] GenerateCaptchaImage(string captchaCode)
        //{
        //    using var image = new Image<Rgba32>(
        //        Width,
        //        Height,
        //        Color.White.ToPixel<Rgba32>());

        //    image.Mutate(context =>
        //        context.Paint(canvas =>
        //        {
        //            // --------------------------------
        //            // Background
        //            // --------------------------------
        //            canvas.Fill(Brushes.Solid(Color.Bisque));

        //            // --------------------------------
        //            // Paper / ink dots
        //            // --------------------------------
        //            for (var i = 0; i < 100; i++)
        //            {
        //                var x = RandomInt(0, Width);
        //                var y = RandomInt(0, Height);
        //                var size = RandomInt(1, 3);

        //                canvas.Fill(
        //                    Brushes.Solid(Color.Brown),
        //                    new EllipsePolygon(
        //                        new PointF(x, y),
        //                        new SizeF(size, size)));
        //            }

        //            // --------------------------------
        //            // Random ink lines
        //            // --------------------------------
        //            for (var i = 0; i < 7; i++)
        //            {
        //                var x1 = RandomInt(0, Width);
        //                var y1 = RandomInt(0, Height);

        //                var x2 = RandomInt(0, Width);
        //                var y2 = RandomInt(0, Height);

        //                canvas.DrawLine(
        //                    Pens.Solid(
        //                        Color.DarkGoldenrod,
        //                        1),
        //                    new PointF(x1, y1),
        //                    new PointF(x2, y2));
        //            }

        //            // --------------------------------
        //            // Font
        //            // --------------------------------
        //            var fontFamily =
        //                SystemFonts.Families.First();

        //            var font =
        //                SystemFonts.CreateFont(
        //                    fontFamily.Name,
        //                    30,
        //                    FontStyle.Bold);

        //            // --------------------------------
        //            // Character colors
        //            // --------------------------------
        //            var colors = new[]
        //            {
        //        Color.Brown,
        //        Color.DarkBlue,
        //        Color.DarkGoldenrod,
        //        Color.Teal,
        //        Color.DarkRed,
        //        Color.DarkSlateBlue
        //            };

        //            // --------------------------------
        //            // CAPTCHA characters
        //            // --------------------------------
        //            var xPosition = 8;

        //            for (var i = 0;
        //                 i < captchaCode.Length;
        //                 i++)
        //            {
        //                var yPosition =
        //                    RandomInt(8, 18);

        //                var textOptions =
        //                    new RichTextOptions(font)
        //                    {
        //                        Origin =
        //                            new PointF(
        //                                xPosition,
        //                                yPosition),

        //                        WrappingLength = -1
        //                    };

        //                canvas.DrawText(
        //                    textOptions,
        //                    captchaCode[i].ToString(),
        //                    Brushes.Solid(
        //                        colors[
        //                            i % colors.Length]),
        //                    pen: null);

        //                xPosition += 34;
        //            }

        //            // --------------------------------
        //            // Extra scratches
        //            // --------------------------------
        //            for (var i = 0; i < 35; i++)
        //            {
        //                var x = RandomInt(0, Width);
        //                var y = RandomInt(0, Height);

        //                canvas.Fill(
        //                    Brushes.Solid(Color.Gray),
        //                    new EllipsePolygon(
        //                        new PointF(x, y),
        //                        new SizeF(1, 1)));
        //            }
        //        }));

        //    // --------------------------------
        //    // Convert to PNG
        //    // --------------------------------
        //    using var stream = new MemoryStream();

        //    image.Save(
        //        stream,
        //        new PngEncoder());

        //    return stream.ToArray();
        //}



        private static byte[] GenerateCaptchaImage(string captchaCode)
        {
            using var image =
                new Image<Rgba32>(
                    Width,
                    Height,
                    Color.Bisque.ToPixel<Rgba32>());

            image.Mutate(context =>
                context.Paint(canvas =>
                {
                    // Background
                    canvas.Fill(
                        Brushes.Solid(
                            Color.Bisque));

                    // Light paper noise
                    for (var i = 0; i < 70; i++)
                    {
                        var x =
                            RandomInt(0, Width);

                        var y =
                            RandomInt(0, Height);

                        canvas.Fill(
                            Brushes.Solid(
                                Color.Brown),
                            new EllipsePolygon(
                                new PointF(x, y),
                                new SizeF(1, 1)));
                    }

                    
                    // Scratch lines
                    // Keep these behind characters
                    for (var i = 0; i < 5; i++)
                    {
                        var x1 =
                            RandomInt(0, Width);

                        var y1 =
                            RandomInt(0, Height);

                        var x2 =
                            RandomInt(0, Width);

                        var y2 =
                            RandomInt(0, Height);

                        canvas.DrawLine(
                            Pens.Solid(
                                Color.Brown,
                                1),
                            new PointF(x1, y1),
                            new PointF(x2, y2));
                    }

                    // CAPTCHA characters
                    var colors = new[]
                    {
                        Color.DarkRed,
                        Color.DarkBlue,
                        Color.DarkGoldenrod,
                        Color.Teal,
                        Color.DarkSlateBlue,
                        Color.Brown
                    };

                    // Character area
                    //
                    // 280px image
                    // 15px left padding
                    // 40px character slot
                    // 5px gap between characters
                    //
                    // Character positions:
                    // 15, 60, 105, 150, 195, 240

                    const int leftPadding = 15;
                    const int characterSpacing = 45;

                    for (var i = 0;
                         i < captchaCode.Length;
                         i++)
                    {
                        var x =
                            leftPadding +
                            (i * characterSpacing);

                        var y =
                            RandomInt(8, 14);

                        DrawCharacter(
                            canvas,
                            captchaCode[i],
                            x,
                            y,
                            colors[
                                i % colors.Length]);
                    }

                    // Small noise around characters
                    for (var i = 0; i < 25; i++)
                    {
                        var x =
                            RandomInt(0, Width);

                        var y =
                            RandomInt(0, Height);

                        canvas.Fill(
                            Brushes.Solid(
                                Color.Gray),
                            new EllipsePolygon(
                                new PointF(x, y),
                                new SizeF(1, 1)));
                    }
                }));

            // PNG
            using var stream =
                new MemoryStream();

            image.Save(
                stream,
                new PngEncoder());

            return stream.ToArray();
        }

        private static void DrawCharacter(DrawingCanvas canvas, char character, int startX, int startY, Color color)
        {
            if (!CharacterMap.TryGetValue(
                    character,
                    out var pattern))
            {
                return;
            }

            const int cellSize = 7;
            const float lineWidth = 7f;

            for (var row = 0; row < pattern.Length; row++)
            {
                for (var col = 0; col < pattern[row].Length; col++)
                {
                    if (pattern[row][col] != '1')
                    {
                        continue;
                    }

                    var x = startX + (col * cellSize);
                    var y = startY + (row * cellSize);

                    // Horizontal stroke
                    if (col + 1 < pattern[row].Length &&
                        pattern[row][col + 1] == '1')
                    {
                        canvas.DrawLine(
                            Pens.Solid(
                                color,
                                lineWidth),
                            new PointF(x, y),
                            new PointF(
                                x + cellSize,
                                y));
                    }

                    // Vertical stroke
                    if (row + 1 < pattern.Length &&
                        pattern[row + 1][col] == '1')
                    {
                        canvas.DrawLine(
                            Pens.Solid(
                                color,
                                lineWidth),
                            new PointF(x, y),
                            new PointF(
                                x,
                                y + cellSize));
                    }

                    // Rounded joint
                    canvas.Fill(
                        Brushes.Solid(color),
                        new EllipsePolygon(
                            new PointF(x, y),
                            new SizeF(
                                lineWidth,
                                lineWidth)));
                }
            }
        }


        private static int RandomInt(int min,int max)
        {
            return RandomNumberGenerator.GetInt32(min, max);
        }

        private static readonly Dictionary<char, string[]> CharacterMap = new()
        {
            ['A'] =
            [
                "01110",
                "10001",
                "10001",
                "11111",
                "10001",
                "10001",
                "10001"
            ],

            ['B'] =
            [
                "11110",
                "10001",
                "10001",
                "11110",
                "10001",
                "10001",
                "11110"
            ],

            ['C'] =
            [
                "01111",
                "10000",
                "10000",
                "10000",
                "10000",
                "10000",
                "01111"
            ],

            ['D'] =
            [
                "11110",
                "10001",
                "10001",
                "10001",
                "10001",
                "10001",
                "11110"
            ],

            ['E'] =
            [
                "11111",
                "10000",
                "10000",
                "11110",
                "10000",
                "10000",
                "11111"
            ],

            ['F'] =
            [
                "11111",
                "10000",
                "10000",
                "11110",
                "10000",
                "10000",
                "10000"
            ],

            ['G'] =
            [
                "01111",
                "10000",
                "10000",
                "10111",
                "10001",
                "10001",
                "01111"
            ],

            ['H'] =
            [
                "10001",
                "10001",
                "10001",
                "11111",
                "10001",
                "10001",
                "10001"
            ],

            ['J'] =
            [
                "00111",
                "00010",
                "00010",
                "00010",
                "10010",
                "10010",
                "01100"
            ],

            ['K'] =
            [
                "10001",
                "10010",
                "10100",
                "11000",
                "10100",
                "10010",
                "10001"
            ],

            ['L'] =
            [
                "10000",
                "10000",
                "10000",
                "10000",
                "10000",
                "10000",
                "11111"
            ],

            ['M'] =
            [
                "10001",
                "11011",
                "10101",
                "10101",
                "10001",
                "10001",
                "10001"
            ],

            ['N'] =
            [
                "10001",
                "11001",
                "10101",
                "10011",
                "10001",
                "10001",
                "10001"
            ],

            ['P'] =
            [
                "11110",
                "10001",
                "10001",
                "11110",
                "10000",
                "10000",
                "10000"
            ],

            ['Q'] =
            [
                "01110",
                "10001",
                "10001",
                "10001",
                "10101",
                "10010",
                "01101"
            ],

            ['R'] =
            [
                "11110",
                "10001",
                "10001",
                "11110",
                "10100",
                "10010",
                "10001"
            ],

            ['S'] =
            [
                "01111",
                "10000",
                "10000",
                "01110",
                "00001",
                "00001",
                "11110"
            ],

            ['T'] =
            [
                "11111",
                "00100",
                "00100",
                "00100",
                "00100",
                "00100",
                "00100"
            ],

            ['U'] =
            [
                "10001",
                "10001",
                "10001",
                "10001",
                "10001",
                "10001",
                "01110"
            ],

            ['V'] =
            [
                "10001",
                "10001",
                "10001",
                "10001",
                "10001",
                "01010",
                "00100"
            ],

            ['W'] =
            [
                "10001",
                "10001",
                "10001",
                "10101",
                "10101",
                "11011",
                "10001"
            ],

            ['X'] =
            [
                "10001",
                "10001",
                "01010",
                "00100",
                "01010",
                "10001",
                "10001"
            ],

            ['Y'] =
            [
                "10001",
                "10001",
                "01010",
                "00100",
                "00100",
                "00100",
                "00100"
            ],

            ['Z'] =
            [
                "11111",
                "00001",
                "00010",
                "00100",
                "01000",
                "10000",
                "11111"
            ],

            ['2'] =
            [
                "01110",
                "10001",
                "00001",
                "00010",
                "00100",
                "01000",
                "11111"
            ],

            ['3'] =
            [
                "11110",
                "00001",
                "00001",
                "01110",
                "00001",
                "00001",
                "11110"
            ],

            ['4'] =
            [
                "00010",
                "00110",
                "01010",
                "10010",
                "11111",
                "00010",
                "00010"
            ],

            ['5'] =
            [
                "11111",
                "10000",
                "10000",
                "11110",
                "00001",
                "00001",
                "11110"
            ],

            ['6'] =
            [
                "01110",
                "10000",
                "10000",
                "11110",
                "10001",
                "10001",
                "01110"
            ],

            ['7'] =
            [
                "11111",
                "00001",
                "00010",
                "00100",
                "01000",
                "01000",
                "01000"
            ],

            ['8'] =
            [
                "01110",
                "10001",
                "10001",
                "01110",
                "10001",
                "10001",
                "01110"
            ],

            ['9'] =
            [
                "01110",
                "10001",
                "10001",
                "01111",
                "00001",
                "00001",
                "01110"
            ]
        };
    }
}