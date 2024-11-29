using System.Runtime.CompilerServices;

namespace DataRepository.Models
{
    public readonly record struct Fail
    {
        public Fail(string? msg, int? code)
        {
            Msg = msg;
            Code = code;
        }

        public string? Msg { get; }

        public int? Code { get; }

        public static Fail WithMsg(string? msg, int? code) => new(msg, code);

        public static Fail WithMsg<T>(string? msg, T? code) where T : Enum => new(msg, Unsafe.As<T?, int>(ref code));
    }
}
