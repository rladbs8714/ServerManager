namespace ServerPlatform
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.06.06
     *  
     *  < 목적 >
     *  - Agent가 처리하는 작업 클래스
     *  
     *  < TODO >
     *  - 
     *  
     *  < History >
     *  2025.06.06 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    internal class Job
    {
        public string Name { get; set; }

        public string Message { get; set; }

        public object? Tag { get; set; }

        public Job(string name, string message, object? tag = default)
        {
            Name = name;
            Message = message;
            Tag = tag;
        }

        /// <summary>
        /// <seealso cref="Job"/>객체를 디스코드 슬래시 명령에 응답할 수 있도록 문자열로 바꿔 반환한다.
        /// </summary>
        /// <param name="sp">메시지와 아이디 구분자</param>
        /// <returns>디스코드 슬래시 명령 응답 문자열</returns>
        public string ToStringForDiscordSlashCommand(string sp = "<|ID|>")
        {
            if (Tag is not ulong)
                return Message;
            else
                return $"{Message}{sp}{Tag}";
        }
    }
}
