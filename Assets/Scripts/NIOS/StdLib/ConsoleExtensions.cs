
namespace StdLib
{
    public static class ConsoleExtensions
    {
        public static string ReadPassword(this Process.ConsoleClass console, char replacingChar = '*')
        {
            var str = string.Empty;
            while (true)
            {
                var c = console.ReadKey(true).KeyChar;
                if (c == ASCII.BackSpace)
                {
                    if (str.Length > 0)
                    {
                        str = str.Substring(0, str.Length - 1);
                        console.Write(ASCII.BackSpace);
                    }
                }
                else if (c == ASCII.NewLine || c == ASCII.CarriageReturn) break;
                else
                {
                    str += c.ToString();
                    console.Write(replacingChar);
                }
            }
            return str;
        }
    }
}
