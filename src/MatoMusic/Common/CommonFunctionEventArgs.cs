
using MatoMusic.Infrastructure.Common;

namespace MatoMusic.Common
{
    public class CommonFunctionEventArgs
    {
        public CommonFunctionEventArgs(IBasicInfo info, string code)
        {
            Info = info;
            Code = code;
        }
        public IBasicInfo Info { get; set; }
        public string Code { get; set; }

    }
}